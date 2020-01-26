using devhl.DBAutomator.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;

namespace devhl.DBAutomator
{
    public abstract class DBObject<T> : IDBObject
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

        public abstract Task<T> Save(DBAutomator dBAutomator);
    }
}
