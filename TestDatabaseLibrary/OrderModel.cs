using DBAutomatorLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestDatabaseLibrary
{
    public class OrderModel : IOrderModel
    {
        public long OrderID { get; set; }

        public long CustomerID { get; set; }

    }
}
