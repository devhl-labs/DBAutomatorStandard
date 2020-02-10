using Dapper.SqlWriter.Interfaces;
using System.Threading.Tasks;

namespace Dapper.SqlWriter
{


    public abstract class DBObject<T>
    {
        internal string _oldValues = string.Empty;

        [NotMapped]
        internal ObjectState ObjectState { get; set; } = ObjectState.New;

        public abstract Task Save(SqlWriter sqlWriter);
    }
}
