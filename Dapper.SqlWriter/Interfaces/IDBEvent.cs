using System.Threading.Tasks;

namespace Dapper.SqlWriter.Interfaces
{
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
