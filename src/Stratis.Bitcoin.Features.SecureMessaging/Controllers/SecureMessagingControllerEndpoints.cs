using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stratis.Bitcoin.Features.SecureMessaging.Models;
using Stratis.Bitcoin.Utilities;
using Stratis.Bitcoin.Utilities.JsonErrors;
using System;
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
    public partial class SecureMessagingController : Controller
    {      
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
    }
}
