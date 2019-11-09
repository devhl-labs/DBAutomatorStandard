using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using DBAutomatorStandard;
using Microsoft.Extensions.Logging;

namespace TestDatabaseLibrary
{
    [Table("User")]
    public class UserModel : IUserModel
    {
        private const string _source = nameof(UserModel);

        [Key]
        public ulong UserID { get; set; }

        [Column("UserNames")]
        public string UserName { get; set; }

        public NotMappedClass NotMappedClass { get; set; }

        [NotMapped]
        public string NotMappedProperty { get; set; }

        [NotMapped]
        public bool IsNewRecord { get; set; } = true;





        public Task OnDeleteAsync(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnDelete", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnDeletedAsync(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnDeleted", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnInsertAsync(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnInsert", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnInsertedAsync(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnInserted", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnLoadedAsync(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnLoadedAsync", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnSaveAsync(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnSave", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnSavedAsync(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnSaved", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnUpdateAsync(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnUpdate", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnUpdatedAsync(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnUpdated", nameof(UserModel));
            return Task.CompletedTask;
        }
    }
}
