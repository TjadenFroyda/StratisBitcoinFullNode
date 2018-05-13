﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Features.SecureMessaging.Interfaces;
using Stratis.Bitcoin.Features.SecureMessaging.Models;
using Stratis.Bitcoin.Features.Wallet;
using Stratis.Bitcoin.Features.Wallet.Interfaces;
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
// TODO: Check coding style guide
// TODO: Safety checks
namespace Stratis.Bitcoin.Features.SecureMessaging.Controllers
{
    /// <summary>
    /// Controller providing SecureMessaging operation
    /// </summary>
    [Route("api/[controller]")]
    public class SecureMessagingController : Controller
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

        /// <summary>The secure messaging.</summary>
        private ISecureMessaging secureMessaging;

        /// <summary>Action.</summary>
        internal enum Action { Encrypt, Decrypt };

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
        }
                
        /// <summary>
        /// Messages the action.
        /// </summary>
        /// <returns>The action.</returns>
        /// <param name="request">Request.</param>
        /// <param name="action">Action.</param>
        internal string MessageAction(SecureMessageRequest request, Action action)
        {
            Key privateKey = GetPrivateKey(request);
            PubKey receiverPubKey = request.ReceiverPublicKey == null ? new PubKey(request.ReceiverPublicKey) : throw new SecureMessageException("Please enter the receiver's public key");
            this.secureMessaging = new SecureMessaging(privateKey, receiverPubKey, this.network);
            if (action == Action.Encrypt)
            {
                return this.secureMessaging.EncryptMessage(request.Message);
            } else {
                return this.secureMessaging.DecryptMessage(request.Message);
            }
        }
        
        internal Wallet.Wallet LoadWalletFromPrivateSeed(Key sharedSecret, string name, DateTime handshakeTime)
        {
            return this.fullNode.NodeService<WalletManager>().LoadWalletFromPrivateKeySeed(sharedSecret, name, handshakeTime);
        }
        
        /// <summary>
        /// Gets the private key.
        /// </summary>
        /// <returns>The private key.</returns>
        /// <param name="request">Request.</param>
        internal Key GetPrivateKey(GetPrivateKeyRequest request)
        {
            if (request.SenderPrivateKey != null)
            {
                return Key.Parse(request.SenderPrivateKey, this.network);
            }
            else if (request.WalletName != null || request.Passphrase != null)
            {
                string encryptedSeed = this.walletManager.GetWalletByName(request.WalletName).EncryptedSeed;
                return HdOperations.DecryptSeed(encryptedSeed, request.Passphrase, this.network);
            }
            else
            {
                throw new SecureMessageException("Missing (wallet and passphrase) or private key.");
            }
        }
        
        /// <summary>
        /// Gets the destination script pub key.
        /// </summary>
        /// <returns>The destination script pub key.</returns>
        /// <param name="request">Request.</param>
        internal Script GetDestScriptPubKey(GetDestScriptPubKeyRequest request)
        {
            if (request.DestinationAddress == null)
            {
                return this.secureMessaging.GetDestScriptPubKey();
            }
            else
            {
                BitcoinAddress destAddress = BitcoinAddress.Create(request.DestinationAddress);
                return destAddress.ScriptPubKey;
            }
        }

        /// <summary>
        /// Builds the transaction batch.
        /// </summary>
        /// <returns>The transaction batch.</returns>
        internal TransactionBatchBuilder BuildTransactionBatch(SecureMessageRequest request)
        {
            string encryptedMessage = this.MessageAction(request, Action.Encrypt);

            List<string> chunkedEncryptedMessages = this.secureMessaging.prepareOPReturnMessageList(encryptedMessage);

            GetDestScriptPubKeyRequest scriptPubKeyRequest = new GetDestScriptPubKeyRequest();
            scriptPubKeyRequest.DestinationAddress = request.DestinationAddress;
            Script destScriptPubKey = this.GetDestScriptPubKey(scriptPubKeyRequest);
            
            List<TransactionBuildContext> contexts = this.secureMessaging.TransactionBuilder(
                request.WalletName,
                request.AccountName,
                destScriptPubKey,
                request.Passphrase,
                chunkedEncryptedMessages
            );

            return new TransactionBatchBuilder(contexts, (FullNode)this.fullNode);
        }

        /// <summary>
        /// Encrypts the message.
        /// </summary>
        /// <returns>The message.</returns>
        /// <param name="request">Request.</param>
        [Route("encrypt-message")]
        [HttpPost]
        public IActionResult EncryptMessage([FromBody] SecureMessageRequest request)
        {
            Guard.NotNull(request, nameof(request));

            // checks the request is valid
            if (!this.ModelState.IsValid)
            {
                return BuildErrorResponse(this.ModelState);
            }
            try
            {
                return Json(MessageAction(request, Action.Encrypt));
            }
            catch (Exception e)
            {
                this.logger.LogError("Exception occurred: {0}", e.ToString());
                return ErrorHelpers.BuildErrorResponse(HttpStatusCode.BadRequest, e.Message, e.ToString());
            }
        }

        /// <summary>
        /// Decrypts the message.
        /// </summary>
        /// <returns>The message.</returns>
        /// <param name="request">Request.</param>
        [Route("decrypt-message")]
        [HttpPost]
        public IActionResult DecryptMessage([FromBody] SecureMessageRequest request)
        {
            Guard.NotNull(request, nameof(request));

            // checks the request is valid
            if (!this.ModelState.IsValid)
            {
                return BuildErrorResponse(this.ModelState);
            }
            try
            {
                return Json(MessageAction(request,Action.Decrypt));
            }
            catch (Exception e)
            {
                this.logger.LogError("Exception occurred: {0}", e.ToString());
                return ErrorHelpers.BuildErrorResponse(HttpStatusCode.BadRequest, e.Message, e.ToString());
            }
        }
        
        /// <summary>
        /// Sends the secure message.
        /// </summary>
        /// <returns>The secure message.</returns>
        /// <param name="request">Request.</param>
        [Route("send")]
        [HttpPost]        
        public IActionResult SendSecureMessage([FromBody] SecureMessageRequest request)
        {
            Guard.NotNull(request, nameof(request));

            // checks the request is valid
            if (!this.ModelState.IsValid)
            {
                return BuildErrorResponse(this.ModelState);
            }
            try
            {
                TransactionBatchBuilder batchBuilder = BuildTransactionBatch(request);
                return Json(batchBuilder.SendBatch((FullNode)this.fullNode));                
            }
            catch (Exception e)
            {
                this.logger.LogError("Exception occurred: {0}", e.ToString());
                return ErrorHelpers.BuildErrorResponse(HttpStatusCode.BadRequest, e.Message, e.ToString());
            }
        }

        /// <summary>
        /// Calculates the transaction cost given the size of the message. Each message must be 
        /// chunked into a 40 byte transaction with 1 satoshi amount and associated miner fee. 
        /// </summary>
        /// <returns>The total transaction cost.</returns>
        /// <param name="request">HTTP post request with required parameters. See model for details.</param>
        [Route("get-transaction-cost")]
        [HttpPost]        
        public IActionResult GetTransactionCost([FromBody] SecureMessageRequest request)
        {
            Guard.NotNull(request, nameof(request));

            // checks the request is valid
            if (!this.ModelState.IsValid)
            {
                return BuildErrorResponse(this.ModelState);
            }
            try
            {
                TransactionBatchBuilder batchBuilder = BuildTransactionBatch(request);
                return Json(batchBuilder.GetTotalCostInSatoshis());                
            }
            catch (Exception e)
            {
                this.logger.LogError("Exception occurred: {0}", e.ToString());
                return ErrorHelpers.BuildErrorResponse(HttpStatusCode.BadRequest, e.Message, e.ToString());
            }
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
