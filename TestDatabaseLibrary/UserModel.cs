using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using devhl.DBAutomator.Interfaces;

using devhl.DBAutomator;

namespace TestDatabaseLibrary
{
	public enum UserType
	{
		User,
		Admin
	}



	[Table("User")]
	public class UserModel : IDBEvent
	{
		private const string _source = nameof(UserModel);

		[Key]
		public ulong UserID { get; set; }

		[Column("UserNames")]
		public string UserNames { get; set; }

		public NotMappedClass NotMappedClass { get; set; } //todo figure this out

		[NotMapped]
		public string NotMappedProperty { get; set; }

		[NotMapped]
		public bool IsNewRecord { get; set; } = true;

		public UserType UserType { get; set; } = UserType.User;



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







/*
BEGIN;

-- CREATE TABLE "User" -----------------------------------------
CREATE TABLE "public"."User" ( 
	"UserID" Bigint NOT NULL,
	"UserNames" Text NOT NULL,
	PRIMARY KEY ( "UserID" ) );
 ;
-- -------------------------------------------------------------

COMMIT;
*/
}
