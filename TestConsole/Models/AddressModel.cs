using Dapper.SqlWriter;

namespace TestDatabaseLibrary
{
	[Table("Address")]
	public class AddressModel
	{
		[Key]
		[AutoIncrement]
		public ulong AddressID { get; set; }
		
		public ulong UserID { get; set; }

		[Column("Address")]
		public string UserAddress { get; set; } = string.Empty;
	}
}
