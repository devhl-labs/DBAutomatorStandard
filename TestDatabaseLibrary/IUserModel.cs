using DBAutomatorStandard;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestDatabaseLibrary
{
    //[TableName("User")]
    public interface IUserModel : IDBObject
    {
        ulong UserID { get; set; }

        [Column("UserNames")]
        string UserName { get; set; }

        NotMappedClass NotMappedClass { get; set; }

        [NotMapped]
        string NotMappedProperty { get; set; }

    }
}