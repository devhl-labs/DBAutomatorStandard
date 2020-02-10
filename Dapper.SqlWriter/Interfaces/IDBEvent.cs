using System.Threading.Tasks;

namespace Dapper.SqlWriter.Interfaces
{
    /// <summary>
    /// These events will only fire if you provide this library an instantiated object. 
    /// </summary>
    public interface IDBEvent
    {
        Task OnInsertAsync(SqlWriter dBAutomator);

        Task OnInsertedAsync(SqlWriter dBAutomator);

        Task OnUpdateAsync(SqlWriter dBAutomator);

        Task OnUpdatedAsync(SqlWriter dBAutomator);

        Task OnDeleteAsync(SqlWriter dBAutomator);

        Task OnDeletedAsync(SqlWriter dBAutomator);
    }
}
