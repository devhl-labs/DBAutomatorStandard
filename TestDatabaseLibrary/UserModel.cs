using System.Threading.Tasks;
using Dapper.SqlWriter.Interfaces;

using Dapper.SqlWriter;

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



		public Task OnDeleteAsync(SqlWriter dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnDeletedAsync(SqlWriter dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnInsertAsync(SqlWriter dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnInsertedAsync(SqlWriter dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnLoadedAsync(SqlWriter dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnSaveAsync(SqlWriter dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnSavedAsync(SqlWriter dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnUpdateAsync(SqlWriter dBAutomator)
		{
			return Task.CompletedTask;
		}

		public Task OnUpdatedAsync(SqlWriter dBAutomator)
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
