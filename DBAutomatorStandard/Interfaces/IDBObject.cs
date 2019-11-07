using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DBAutomatorLibrary
{
    public interface IDBObject
    {
        /// <summary>
        /// default this to true in your class
        /// </summary>
        bool IsNewRecord { get; set; }


        /// <summary>
        /// track this yourself however you wish
        /// </summary>
        bool IsDirty { get; set; }



        Task OnInsert(DBAutomator dBAutomator);

        Task OnInserted(DBAutomator dBAutomator);

        Task OnUpdate(DBAutomator dBAutomator);

        Task OnUpdated(DBAutomator dBAutomator);

        /// <summary>
        /// called after OnInsert or OnUpdate
        /// </summary>
        /// <param name="dBAutomator"></param>
        /// <returns></returns>
        Task OnSave(DBAutomator dBAutomator);

        /// <summary>
        /// called after OnInserted or OnUpdated
        /// </summary>
        /// <param name="dBAutomator"></param>
        /// <returns></returns>
        Task OnSaved(DBAutomator dBAutomator);

        Task OnLoaded(DBAutomator dBAutomator);

        Task OnDelete(DBAutomator dBAutomator);

        Task OnDeleted(DBAutomator dBAutomator);
    }
}
