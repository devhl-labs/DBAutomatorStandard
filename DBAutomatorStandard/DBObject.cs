using devhl.DBAutomator.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;

namespace devhl.DBAutomator
{
    public class DBObject : IDBObject
    {
        /// <summary>
        /// Defaults to true
        /// </summary>
        [NotMapped]
        public virtual bool IsNew { get; set; } = true;

        /// <summary>
        /// Defaults to false
        /// </summary>
        [NotMapped]
        public virtual bool IsDirty { get; set; } = false;

        public virtual async Task Save(DBAutomator dBAutomator)
        {
            if (IsNew)
            {
                await dBAutomator.InsertAsync(this);
            }
            else if (IsDirty)
            {
                await dBAutomator.UpdateAsync(this);
            }
        }
    }
}
