using devhl.DBAutomator;

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
		public string UserAddress { get; set; }
	}







/*
BEGIN;

-- CREATE TABLE "Address" --------------------------------------
CREATE TABLE "public"."Address" ( 
	"AddressID" Bigint DEFAULT nextval('"Address_AddressID_seq"'::regclass) NOT NULL,
	"UserID" Bigint NOT NULL,
	"Address" Text NOT NULL,
	PRIMARY KEY ( "AddressID" ) );
 ;
-- -------------------------------------------------------------

COMMIT;
*/
}
