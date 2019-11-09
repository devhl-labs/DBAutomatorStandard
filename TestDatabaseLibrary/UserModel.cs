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

        //[ColumnName("mycolumn")]
        [Key]
        public ulong UserID { get; set; }

        [Column("UserNames")]
        public string UserName { get; set; }

        public NotMappedClass NotMappedClass { get; set; }

        [NotMapped]
        public string NotMappedProperty { get; set; }

        [NotMapped]
        public bool IsNewRecord { get; set; } = true;

        [NotMapped]
        public bool IsDirty { get; set; }

        public Task OnDelete(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnDelete", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnDeleted(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnDeleted", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnInsert(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnInsert", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnInserted(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnInserted", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnLoaded(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnLoaded", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnSave(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnSave", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnSaved(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnSaved", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnUpdate(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnUpdate", nameof(UserModel));
            return Task.CompletedTask;
        }

        public Task OnUpdated(DBAutomator dBAutomator)
        {
            dBAutomator.Logger?.LogDebug("{source}: {method} {type}", _source, "OnUpdated", nameof(UserModel));
            return Task.CompletedTask;
        }
    }
}
