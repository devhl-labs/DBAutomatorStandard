using System;
using System.Collections.Generic;
using System.Text;

namespace devhl.DBAutomator.Interfaces
{
    interface IDBObject
    {
        bool IsNew { get; set; }

        bool IsDirty { get; set; }
    }
}
