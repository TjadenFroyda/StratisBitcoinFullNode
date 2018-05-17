using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NBitcoin;
using Stratis.Bitcoin.Features.Wallet.Models;

namespace Stratis.Bitcoin.Features.SecureMessaging.Models
{
    class SecureMessageSendResult
    {
        [Required(ErrorMessage = "The message sending results are missing")]
        public List<WalletSendTransactionModel> SendTransactionResults { get; set; }

        [Required(ErrorMessage = "The cost of the transaction are missing")]
        public Money Cost { get; set; }
    }
}
