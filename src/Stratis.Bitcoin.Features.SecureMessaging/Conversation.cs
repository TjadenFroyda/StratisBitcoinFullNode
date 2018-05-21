using NBitcoin;

namespace Stratis.Bitcoin.Features.SecureMessaging
{
    /// <summary>
    /// A watch only wallet specifically for turning OP_Return messages into conversations.
    /// A modification of the watch only wallet that will not perform any calculations 
    /// for balances. It will simply collect OP_Return messages for watched addresses
    /// and perform some processing to organize the messages into a conversation.
    /// </summary>
    public class Conversation : WatchOnlyWallet.WatchOnlyWallet
    {
        private ExtKey seedExtKey;

        public Conversation(ExtKey seed)
        {
            this.seedExtKey = seed;
        }
    }
}

