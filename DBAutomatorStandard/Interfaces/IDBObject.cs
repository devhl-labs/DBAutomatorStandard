using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DBAutomatorStandard
{
    public interface IDBObject
    {
        Task OnInsertAsync(DBAutomator dBAutomator);

        Task OnInsertedAsync(DBAutomator dBAutomator);

        Task OnUpdateAsync(DBAutomator dBAutomator);

        Task OnUpdatedAsync(DBAutomator dBAutomator);

        /// <summary>
        /// called after OnInsert or OnUpdate
        /// </summary>
        /// <param name="dBAutomator"></param>
        /// <returns></returns>
        Task OnSaveAsync(DBAutomator dBAutomator);

        /// <summary>
        /// called after OnInserted or OnUpdated
        /// </summary>
        /// <param name="dBAutomator"></param>
        /// <returns></returns>
        Task OnSavedAsync(DBAutomator dBAutomator);

        Task OnLoadedAsync(DBAutomator dBAutomator);

        Task OnDeleteAsync(DBAutomator dBAutomator);

        Task OnDeletedAsync(DBAutomator dBAutomator);
    }
}
