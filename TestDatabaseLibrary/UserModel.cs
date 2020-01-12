using System.Threading.Tasks;
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
		[Key]
		public ulong? UserID { get; set; }

		[Column("UserNames")]
		public string UserNames { get; set; }

		public NotMappedClass NotMappedClass { get; set; }

		[NotMapped]
		public string NotMappedProperty { get; set; }

		[NotMapped]
		public bool IsNewRecord { get; set; } = true;

		public UserType UserType { get; set; } = UserType.User;



		public Task OnDeleteAsync(DBAutomator dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnDeletedAsync(DBAutomator dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnInsertAsync(DBAutomator dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnInsertedAsync(DBAutomator dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnLoadedAsync(DBAutomator dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnSaveAsync(DBAutomator dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnSavedAsync(DBAutomator dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnUpdateAsync(DBAutomator dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnUpdatedAsync(DBAutomator dBAutomator)
		{
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
