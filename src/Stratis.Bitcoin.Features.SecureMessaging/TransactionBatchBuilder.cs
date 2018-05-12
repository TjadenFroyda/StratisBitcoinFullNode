using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using Stratis.Bitcoin.Features.SecureMessaging.Interfaces;
using Stratis.Bitcoin.Features.Wallet;
using Stratis.Bitcoin.Features.Wallet.Models;
using Stratis.Bitcoin.Features.SecureMessaging.Controllers;
using Stratis.Bitcoin.Features.Wallet.Controllers;

namespace Stratis.Bitcoin.Features.SecureMessaging
{
	public class TransactionBatchBuilder : IBatchTransactionBuilder
    {
		private List<WalletBuildTransactionModel> transactionModelList;
		private decimal totalFee;
        
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:Stratis.Bitcoin.Features.SecureMessaging.TransactionBatchBuilder"/> class.
        /// </summary>
        /// <param name="contexts">Contexts.</param>
        /// <param name="fullNode">Full node.</param>
		public TransactionBatchBuilder(List<TransactionBuildContext> contexts, FullNode fullNode)
        {
			this.totalFee = 0.0M;
			this.transactionModelList = new List<WalletBuildTransactionModel>();
			this.buildTransactionModels(contexts, fullNode);
        }

        /// <summary>
        /// Gets the total fee in satoshis.
        /// </summary>
        /// <returns>The total fee in satoshis.</returns>
		public int GetTotalFeeInSatoshis()
		{
			return Decimal.ToInt32(this.totalFee);
		}
        
        /// <summary>
        /// Sends the batch.
        /// </summary>
        /// <returns>The batch.</returns>
        /// <param name="fullNode">Full node.</param>
        public IActionResult SendBatch(FullNode fullNode)
		{
			List<IActionResult> txResultList = new List<IActionResult>();
			foreach(WalletBuildTransactionModel model in this.transactionModelList)
			{
				JsonResult result = (JsonResult)fullNode.NodeService<WalletController>().SendTransaction(new SendTransactionRequest(model.Hex));
				txResultList.Add(fullNode.NodeService<SecureMessagingController>().Json(result));
			}
			return fullNode.NodeService<SecureMessagingController>().Json(txResultList);
		}

        /// <summary>
        /// Builds the transaction models.
        /// </summary>
        /// <param name="contexts">Contexts.</param>
        /// <param name="fullnode">Fullnode.</param>
		internal void buildTransactionModels(List<TransactionBuildContext> contexts, FullNode fullnode)
		{
			foreach (TransactionBuildContext context in contexts)
			{
				Transaction transactionResult = fullnode.NodeService<WalletTransactionHandler>().BuildTransaction(context);
                if(!StandardScripts.IsStandardTransaction(transactionResult))
				{
					throw new SecureMessageException("Non-standard transaction.");
				}
				WalletBuildTransactionModel model = new WalletBuildTransactionModel
				{
					Hex = transactionResult.ToHex(),
					Fee = context.TransactionFee,
					TransactionId = transactionResult.GetHash()
				};
				this.totalFee += context.TransactionFee.ToDecimal(MoneyUnit.Satoshi);
				this.transactionModelList.Add(model);
			}                         
		}


    }
}
