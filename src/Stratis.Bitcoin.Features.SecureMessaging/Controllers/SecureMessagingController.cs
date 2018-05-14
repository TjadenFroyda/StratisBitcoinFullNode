using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Features.Api;
using Stratis.Bitcoin.Features.SecureMessaging.Interfaces;
using Stratis.Bitcoin.Features.SecureMessaging.Models;
using Stratis.Bitcoin.Features.Wallet;
using Stratis.Bitcoin.Features.Wallet.Controllers;
using Stratis.Bitcoin.Features.Wallet.Interfaces;
using Stratis.Bitcoin.Features.Wallet.Models;
using Stratis.Bitcoin.Utilities;
using Stratis.Bitcoin.Utilities.JsonErrors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Stratis.Bitcoin.Features.SecureMessaging.Tests")]

// TODO: Add Logging
// TODO: Add/improve comments
// TODO: Safety checks
namespace Stratis.Bitcoin.Features.SecureMessaging.Controllers
{
    /// <summary>
    /// Controller providing SecureMessaging operations
    /// </summary>
    [Route("api/[controller]")]
    public partial class SecureMessagingController : Controller
    {
        /// <summary>Instance logger.</summary>
        private readonly ILogger logger;

        /// <summary>Full Node.</summary>
        private readonly IFullNode fullNode;

        /// <summary>The wallet manager.</summary>
        private readonly IWalletManager walletManager;

        /// <summary>The wallet transaction handler.</summary>
        private readonly IWalletTransactionHandler walletTransactionHandler;

        /// <summary>Specification of the network the node runs on - regtest/testnet/mainnet.</summary>
        private readonly Network network;

        /// <summary>The secure messaging handler.</summary>
        private ISecureMessaging secureMessaging;

        /// <summary>Action to be performed by the node - encryption or decryption.</summary>
        internal enum Action { Encrypt, Decrypt };

        /// <summary></summary>
        private Uri apiURI;

        /// <summary>
        /// Initializes a new instance of the object.
        /// </summary>
        /// <param name="fullNode">Full Node.</param>
        /// <param name="loggerFactory">Factory to be used to create logger for the node.</param>
        /// <param name="walletManager">The wallet manager.</param>
        public SecureMessagingController(
            IFullNode fullNode,
            ILoggerFactory loggerFactory,
            IWalletManager walletManager,
            Network network,
            IWalletTransactionHandler walletTransactionHandler)
        {
            Guard.NotNull(fullNode, nameof(fullNode));
            Guard.NotNull(loggerFactory, nameof(loggerFactory));
            Guard.NotNull(walletManager, nameof(walletManager));
            Guard.NotNull(walletTransactionHandler, nameof(walletTransactionHandler));
            this.fullNode = fullNode;
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
            this.walletManager = walletManager;
            this.network = network;
            this.walletTransactionHandler = walletTransactionHandler;
            if (this.network == Network.StratisMain)
            {
                this.apiURI = new Uri("http://localhost:37221");
            }
            else if (this.network == Network.TestNet)
            {
                this.apiURI = new Uri("http://localhost:38220");
            }
            else if (this.network == Network.StratisTest)
            {
                this.apiURI = new Uri("http://localhost:38221");
            }
            else 
            {
                this.apiURI = new Uri("http://localhost:37220");
            }
        }
                
        /// <summary>
        /// Messages the action.
        /// </summary>
        /// <returns>The action.</returns>
        /// <param name="request">Request.</param>
        /// <param name="action">Action.</param>
        internal string MessageAction(SecureMessageRequest request, Action action)
        {
            Key privateKey = GetPrivateMessagingKey(request);
            PubKey receiverPubKey = request.ReceiverPublicKey == null ? new PubKey(request.ReceiverPublicKey) : throw new SecureMessageException("Please enter the receiver's public key");
            this.secureMessaging = new SecureMessaging(privateKey, receiverPubKey, this.network);
            if (action == Action.Encrypt)
            {
                return this.secureMessaging.EncryptMessage(request.Message);
            }
            else
            {
                return this.secureMessaging.DecryptMessage(request.Message);
            }
        }
        
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="sharedSecret"></param>
        /// <param name="name"></param>
        /// <param name="handshakeTime"></param>
        /// <returns></returns>
        internal Wallet.Wallet LoadWalletFromPrivateSeed(Key sharedSecret, string name, DateTime handshakeTime)
        {
            return this.fullNode.NodeService<WalletManager>().LoadWalletFromPrivateKeySeed(sharedSecret, name, handshakeTime);
        }
        
        /// <summary>
        /// Gets the private messaging key.
        /// </summary>
        /// <returns>The private messaging key.</returns>
        /// <param name="request">Request.</param>
        internal Key GetPrivateMessagingKey(SecureMessageKeyRequest request)
        {
            if (request.SenderPrivateKey != null)
            {
                return Key.Parse(request.SenderPrivateKey, this.network);
            }
            else if (request.WalletName != null || request.Passphrase != null)
            {
                string encryptedSeed = this.walletManager.GetWalletByName(request.WalletName).EncryptedSeed;
                Key decryptedSeed = HdOperations.DecryptSeed(encryptedSeed, request.Passphrase, this.network);
                return new Key(decryptedSeed.ToBytes());
            }
            else
            {
                throw new SecureMessageException("Missing (wallet and passphrase) or private key.");
            }
        }

        /// <summary>
        /// Gets the private messaging pub key.
        /// </summary>
        /// <returns>The private messaging pub key.</returns>
        /// <param name="request">Request.</param>
        internal PubKey GetPrivateMessagingPubKey(SecureMessageRequest request){
            Key privateKey = GetPrivateMessagingKey(request);
            return privateKey.PubKey;
        }

        /// <summary>
        /// A method to determine total transaction cost for the message in satoshis. 
        /// </summary>
        /// <param name="buildTransactionResults"></param>
        /// <returns></returns>
        internal Money GetSecureMessageTransactionCost(List<IActionResult> buildTransactionResults)
        {
            Money cost = new Money(0);
            foreach(IActionResult buildTransactionResult in buildTransactionResults)
            {
                WalletBuildTransactionModel walletTransactionModel = (WalletBuildTransactionModel)(buildTransactionResult as JsonResult)?.Value;
                cost += walletTransactionModel.Fee;
            }
            return cost;
        }

        /// <summary>
        /// Builds the transaction batch but does not sent. Mainly useful to preview the transaction before sending.
        /// </summary>
        /// <returns>The transaction batch.</returns>
        internal List<BuildTransactionRequest> PrepareTransactionBatch(SecureMessageRequest request)
        {
            string encryptedMessage = this.MessageAction(request, Action.Encrypt);

            List<string> chunkedEncryptedMessages = this.secureMessaging.prepareOPReturnMessageList(encryptedMessage);

            List<BuildTransactionRequest> prepareTransactionRequests = new List<BuildTransactionRequest>();

            foreach (string message in chunkedEncryptedMessages){
                BuildTransactionRequest transactionRequest = new BuildTransactionRequest
                {
                    WalletName = request.WalletName,
                    AccountName = request.AccountName,
                    DestinationAddress = request.DestinationAddress,
                    Amount = new Money(0).ToString(),
                    FeeAmount = "Low",
                    Password = request.Passphrase,
                    OpReturnData = message,
                    AllowUnconfirmed = true,
                    ShuffleOutputs = true
                };
                prepareTransactionRequests.Add(transactionRequest);
            }
            return prepareTransactionRequests;
        }

        /// <summary>
        /// Builds a batch of transaction requests using the API internals.  
        /// </summary>
        /// <param name="buildTransactionRequests">A list of valid transaction requests</param>
        /// <returns>A List of json responses from the api stating success/failure of each message.</returns>
        internal List<IActionResult> BuildTransactionBatch(List<BuildTransactionRequest> buildTransactionRequests)
        {
            List<IActionResult> walletBuildTransactionResults = new List<IActionResult>();
            foreach (BuildTransactionRequest buildTransactionRequest in buildTransactionRequests)
            {
                IActionResult transactionBuildResult = this.fullNode.NodeService<WalletController>().BuildTransaction(buildTransactionRequest);
                walletBuildTransactionResults.Add(transactionBuildResult);    
            }
            return walletBuildTransactionResults;
        }
       
        /// <summary>
        /// Builds and sends a secure message with the provided parameters using the API internals. 
        /// </summary>
        /// <param name="request">A HttpPost reuqest with the SecureMessageRequest parameters</param>
        /// <returns></returns>
        internal IActionResult BuildAndSendTransactionBatch(SecureMessageRequest request)
        {
            List<BuildTransactionRequest> preparedTransactionRequests = PrepareTransactionBatch(request);
            List<IActionResult> buildTransactionRequestResults = BuildTransactionBatch(preparedTransactionRequests);
            Money cost = GetSecureMessageTransactionCost(buildTransactionRequestResults);
            List<WalletSendTransactionModel> sendRequestResults = new List<WalletSendTransactionModel>();
            foreach (IActionResult buildTransactionRequestResult in buildTransactionRequestResults)
            {
                WalletBuildTransactionModel walletTransactionModel = (WalletBuildTransactionModel)(buildTransactionRequestResult as JsonResult)?.Value;
                IActionResult sendRequestResult = this.fullNode.NodeFeature<WalletController>().SendTransaction(new SendTransactionRequest(walletTransactionModel.Hex));
                sendRequestResults.Add((WalletSendTransactionModel)(sendRequestResult as JsonResult)?.Value);
            }
            return this.Json(new SecureMessageSendResult { Cost = cost, SendTransactionResults = sendRequestResults });
        }

        /// <summary>
        /// Builds an <see cref="IActionResult"/> containing errors contained in the <see cref="ControllerBase.ModelState"/>.
        /// </summary>
        /// <returns>A result containing the errors.</returns>
        private static IActionResult BuildErrorResponse(ModelStateDictionary modelState)
        {
            List<ModelError> errors = modelState.Values.SelectMany(e => e.Errors).ToList();
            return ErrorHelpers.BuildErrorResponse(
                HttpStatusCode.BadRequest,
                string.Join(Environment.NewLine, errors.Select(m => m.ErrorMessage)),
                string.Join(Environment.NewLine, errors.Select(m => m.Exception?.Message))
            );
        }
    }
}
