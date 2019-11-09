using DBAutomatorStandard;
using System.ComponentModel.DataAnnotations;

namespace TestDatabaseLibrary
{
    //[TableName("Order")]
    public interface IOrderModel
    {
        long CustomerID { get; set; }

        [Key]
        long OrderID { get; set; }
    }
}