using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using NBitcoin;
using Stratis.Bitcoin.Base;
using Stratis.Bitcoin.Interfaces;
using Stratis.Bitcoin.Features.BlockStore.Models;
using Stratis.Bitcoin.Utilities.JsonErrors;

namespace Stratis.Bitcoin.Features.BlockStore.Controllers
{
    /// <summary>
    /// Controller providing operations on a blockstore.
    /// </summary>
    [Route("api/[controller]")]
    public class BlockStoreController : Controller
    {
        private readonly ILogger logger;
        protected readonly ILoggerFactory loggerFactory;
        private readonly IPooledTransaction pooledTransaction;
        private readonly IFullNode fullNode;
        private readonly IBlockStoreCache blockStoreCache;
        private readonly ChainBase chain;
        private readonly IChainState chainState;
        private readonly Network network;

        public BlockStoreController(
            ILoggerFactory loggerFactory, 
            IBlockStoreCache blockStoreCache,
            IFullNode fullNode,
            ChainBase chain,
            IChainState chainState,
            Network network,
            IPooledTransaction pooledTransaction = null)
        {
            this.blockStoreCache = blockStoreCache;         
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
            this.loggerFactory = loggerFactory;
            this.pooledTransaction = pooledTransaction;
            this.fullNode = fullNode;
            this.chain = chain;
            this.chainState = chainState;
            this.network = network;
        }

        [Route("block")]
        [HttpGet]
        public async Task<IActionResult> GetBlockAsync([FromQuery] SearchByHashRequest query)
        {
            if (!this.ModelState.IsValid)
            {
                return BuildErrorResponse(this.ModelState);
            }

            this.logger.LogTrace("({0}:'{1}')", nameof(SearchByHashRequest.Hash), query.Hash);

            try
            {
                var block = await this.blockStoreCache.GetBlockAsync(uint256.Parse(query.Hash)).ConfigureAwait(false);
                if(block == null) return new NotFoundObjectResult("Block not found");
                return query.OutputJson 
                    ? this.Json(new BlockModel(block))
                    : this.Json(block);
            } 
            catch (Exception e)
            {
                this.logger.LogError("Exception occurred: {0}", e.ToString());
                return ErrorHelpers.BuildErrorResponse(HttpStatusCode.BadRequest, e.Message, e.ToString());
            }
        }

        /// <summary>
        /// Builds an <see cref="IActionResult"/> containing the transaction for the given txid.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The transaction</returns>
        [Route("getrawtransaction")]
        [HttpGet]
        public async Task<IActionResult> GetRawTransaction([FromQuery] GetRawTransactionRequest request)
        {
            if (!this.ModelState.IsValid)
            {
                return BuildErrorResponse(this.ModelState);
            }

            this.logger.LogTrace("({0}:'{1}')", nameof(GetRawTransactionRequest.txid), request.txid);

            try
            {
                uint256 trxid; 
                if (!uint256.TryParse(request.txid, out trxid)) 
                {
                    throw new ArgumentException(nameof(request.txid)); 
                }
                Transaction trx = this.pooledTransaction != null ? await this.pooledTransaction.GetTransaction(trxid) : null;
                if (trx == null)
                {
                    IBlockStore blockStore = this.fullNode.NodeFeature<IBlockStore>();
                    trx = blockStore != null ? await blockStore.GetTrxAsync(trxid) : null;
                }
                if (trx == null)
                {
                    throw new Exception("Txid not found.");
                }
                if (request.verbose != 0)
                {
                    ChainedHeader block = await this.GetTransactionBlockAsync(trxid);
                    return this.Json(new TransactionVerboseModel(trx, this.network, block, this.chainState?.ConsensusTip));
                }
                else
                {
                    return this.Json(new TransactionBriefModel(trx));
                }
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
                string.Join(Environment.NewLine, errors.Select(m => m.Exception?.Message)));
        }

        private async Task<ChainedHeader> GetTransactionBlockAsync(uint256 trxid)
        {
            ChainedHeader block = null;
            var blockStore = this.fullNode.NodeFeature<IBlockStore>();
            uint256 blockid = blockStore != null ? await blockStore.GetTrxBlockIdAsync(trxid) : null;
            if (blockid != null)
            {
                block = this.chain?.GetBlock(blockid);
            }
            return block;
        }
    }
}