using DBAutomatorLibrary;

namespace TestDatabaseLibrary
{
    //[TableName("User")]
    public interface IUserModel : IDBObject
    {
        ulong UserID { get; set; }

        [ColumnName("UserNames")]
        string UserName { get; set; }

        NotMappedClass NotMappedClass { get; set; }

        [NotMapped]
        string NotMappedProperty { get; set; }

    }
}