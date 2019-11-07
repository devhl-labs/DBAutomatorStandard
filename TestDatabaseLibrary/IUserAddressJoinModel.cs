//using DBAutomatorLibrary;
//using static DBAutomatorStandard.Enums;

//namespace TestDatabaseLibrary
//{
//    interface IUserAddressJoinModel
//    {
//        NotMappedClass NotMappedClass { get; set; }
//        string NotMappedProperty { get; set; }
//        string UserAddress { get; set; }

//        [ForeignKey(JoinType.inner, "User", "UserID", "UserAddress", "UserID")]
//        ulong UserID { get; set; }
//        string UserName { get; set; }
//    }
//}