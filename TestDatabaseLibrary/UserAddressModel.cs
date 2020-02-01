using Dapper.SqlWriter;

namespace TestDatabaseLibrary
{
    [Table("UserAddress")] //view
    public class UserAddressModel
    {
        //UserModel
        public ulong UserID { get; set; }

        [Column("UserNames")]
        public string UserName { get; set; }

        public NotMappedClass NotMappedClass { get; set; }

        [NotMapped]
        public string NotMappedProperty { get; set; }

        [NotMapped]
        public bool IsNewRecord { get; set; } = true;







        //AddressModel
        public ulong AddressID { get; set; }

        [Column("Address")]
        public string UserAddress { get; set; }
    }







/*
BEGIN;

-- CREATE VIEW "UserAddress" -----------------------------------
CREATE OR REPLACE VIEW "public"."UserAddress" AS  SELECT u."UserID",
    u."UserNames",
    a."AddressID",
    a."Address"
   FROM ("User" u
     JOIN "Address" a ON ((u."UserID" = a."UserID")));;
-- -------------------------------------------------------------

COMMIT;
*/



}
