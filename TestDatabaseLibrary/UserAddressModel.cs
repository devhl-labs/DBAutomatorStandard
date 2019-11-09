using DBAutomatorStandard;

namespace TestDatabaseLibrary
{
    public class UserAddressModel : IUserAddressModel
    {
        public ulong UserID { get; set; }

        public string UserAddress { get; set; }
    }
}
