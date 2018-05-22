using System;
using System.Collections.Generic;
using System.Text;

namespace Stratis.Bitcoin.Features.SecureMessaging.Interfaces
{
    public interface IContactManager
    {
        void CreateContactWallet();
        List<Contact> LoadContactList();
        void SaveContactList();
        void AddContact(Contact contact);
        Contact GetContactByName(string name);
        void DeleteContactByName(string name);
    }
}
