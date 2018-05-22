using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using NBitcoin;
using Stratis.Bitcoin.Features.SecureMessaging.Interfaces;
using Stratis.Bitcoin.Features.Wallet;

[assembly: InternalsVisibleTo("Stratis.Bitcoin.Features.SecureMessaging.Tests")]

namespace Stratis.Bitcoin.Features.SecureMessaging
{
    class ContactManager : IContactManager
    {
        private Wallet.Wallet contactWallet;

        public ContactManager()
        {
        }
        public void AddContact(Contact contact)
        {
            throw new NotImplementedException();
        }

        public void CreateContactWallet()
        {
            throw new NotImplementedException();
        }

        public void DeleteContactByName(string name)
        {
            throw new NotImplementedException();
        }

        public Contact GetContactByName(string name)
        {
            throw new NotImplementedException();
        }

        public List<Contact> LoadContactList()
        {
            throw new NotImplementedException();
        }

        public void SaveContactList()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks to see if a contacts wallet has been created and generates a new wallet
        /// if it hasn't been created. 
        /// </summary>
        /// <returns></returns>
        internal Wallet.Wallet LoadOrGenerateContactWallet()
        {
            string name = "Contacts";
            
        }

        internal void SaveWallet(Wallet.Wallet wallet)
        {

        }
    }
}
