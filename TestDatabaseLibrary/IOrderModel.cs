using DBAutomatorLibrary;

namespace TestDatabaseLibrary
{
    //[TableName("Order")]
    public interface IOrderModel
    {
        long CustomerID { get; set; }

        [Identity]
        long OrderID { get; set; }
    }
}