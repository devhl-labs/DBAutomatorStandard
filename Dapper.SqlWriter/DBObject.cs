using Dapper.SqlWriter.Interfaces;
using System.Threading.Tasks;

namespace Dapper.SqlWriter
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

        public abstract Task<T> Save(SqlWriter dBAutomator);
    }
}
