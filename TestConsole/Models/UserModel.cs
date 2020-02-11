using System.Threading.Tasks;
using Dapper.SqlWriter.Interfaces;

using Dapper.SqlWriter;
using TestConsole;

namespace TestDatabaseLibrary
{
	[Table("User")]
	public class UserModel : IDBEvent
	{
		public ulong UserID { get; set; }

		public string FirstName { get; set; } = string.Empty;

		public string LastName { get; set; } = string.Empty;

		public UserType UserType { get; set; } = UserType.User;

		public long? SomeLongNumber { get; set; }

		public NotMappedClass NotMappedClass { get; set; } = new NotMappedClass();

		public string NotMappedProperty { get; set; } = string.Empty;





		public Task OnDeleteAsync(SqlWriter sqlWriter)
		{
			return Task.CompletedTask;
		}

		public Task OnDeletedAsync(SqlWriter sqlWriter)
		{
			return Task.CompletedTask;
		}

		public Task OnInsertAsync(SqlWriter sqlWriter)
		{
			return Task.CompletedTask;
		}

		public Task OnInsertedAsync(SqlWriter sqlWriter)
		{
			return Task.CompletedTask;
		}

		public Task OnUpdateAsync(SqlWriter sqlWriter)
		{
			return Task.CompletedTask;
		}

		public Task OnUpdatedAsync(SqlWriter sqlWriter)
		{
			return Task.CompletedTask;
		}
	}
}
