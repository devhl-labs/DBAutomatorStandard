using DBAutomatorStandard;

namespace DBAutomatorStandard
{
    //[TableName("UserAddress")]
    public interface IUserAddressModel
    {
        string UserAddress { get; set; }

        ulong UserID { get; set; }
    }
}