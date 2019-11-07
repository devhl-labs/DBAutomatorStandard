using System;
using System.Collections.Generic;
using System.Text;

namespace TestDatabaseLibrary
{
    public class UserAddressModel : IUserAddressModel
    {
        public ulong UserID { get; set; }

        public string UserAddress { get; set; }
    }
}
