//using System;
//using System.Collections.Generic;
//using System.Text;
//using static DBAutomatorLibrary.DBAutomator;
//using static devhl.DBAutomator.Enums;

//namespace devhl.DBAutomatorLibrary
//{
//    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]

//    public class ForeignKey : Attribute
//    {
//        public readonly string ParentTableName;
//        public readonly string ParentColumnName;
//        public readonly string ChildTableName;
//        public readonly string ChildColumnName;
//        public readonly JoinType JoinType;

//        public ForeignKey(JoinType joinType, string parentTableName, string parentColumnName, string childTableName, string childColumnName)
//        {
//            JoinType = joinType;
//            ChildTableName = childTableName;
//            ChildColumnName = childColumnName;
//            ParentTableName = parentTableName;
//            ParentColumnName = parentColumnName;
//        }


//    }
//}
