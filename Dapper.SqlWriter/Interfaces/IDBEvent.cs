using System.Threading.Tasks;

namespace Dapper.SqlWriter.Interfaces
{
    public interface IDBEvent
    {
        Task OnInsertAsync(SqlWriter dBAutomator);

        Task OnInsertedAsync(SqlWriter dBAutomator);

        Task OnUpdateAsync(SqlWriter dBAutomator);

        Task OnUpdatedAsync(SqlWriter dBAutomator);

        /// <summary>
        /// called after OnInsert or OnUpdate
        /// </summary>
        /// <param name="dBAutomator"></param>
        /// <returns></returns>
        Task OnSaveAsync(SqlWriter dBAutomator);

        /// <summary>
        /// called after OnInserted or OnUpdated
        /// </summary>
        /// <param name="dBAutomator"></param>
        /// <returns></returns>
        Task OnSavedAsync(SqlWriter dBAutomator);

        Task OnLoadedAsync(SqlWriter dBAutomator);

        Task OnDeleteAsync(SqlWriter dBAutomator);

        Task OnDeletedAsync(SqlWriter dBAutomator);
    }
}
