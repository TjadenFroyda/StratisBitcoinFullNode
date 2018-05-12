using Microsoft.AspNetCore.Mvc;

// TODO: Add/improve comments
// TODO: Check coding style guide
namespace Stratis.Bitcoin.Features.SecureMessaging.Interfaces
{
    public interface IBatchTransactionBuilder
    {
		/// <summary>
        /// Gets the total fee in satoshis for the batch of transactions
        /// </summary>
        /// <returns>The total fee in satoshis.</returns>
		int GetTotalCostInSatoshis();

        /// <summary>
        /// Sends the batch.
        /// </summary>
        /// <returns>The batch.</returns>
        /// <param name="fullNode">Full node.</param>
		IActionResult SendBatch(FullNode fullNode);
    }
}
