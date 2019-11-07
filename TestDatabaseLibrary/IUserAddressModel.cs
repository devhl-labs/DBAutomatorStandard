using DBAutomatorLibrary;

namespace TestDatabaseLibrary
{
    //[TableName("UserAddress")]
    public interface IUserAddressModel
    {
        string UserAddress { get; set; }

        ulong UserID { get; set; }
    }
}