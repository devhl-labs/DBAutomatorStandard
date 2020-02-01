using System;
using System.Threading.Tasks;
using System.IO;

using Dapper.SqlWriter;

using TestDatabaseLibrary;
using Dapper.SqlWriter.Interfaces;
using System.Data;
using Npgsql;
using System.Linq;

namespace TestConsole
{
    public class Program
    {
#nullable disable

        public static SqlWriter Postgres { get; set; }

#nullable enable

        public static async Task Main()
        {
            ILogger logService = new LogService();

            string password = File.ReadAllText(@"E:\Desktop\password.txt");

            QueryOptions queryOptions = new QueryOptions();

            string connectionString = $"Server=127.0.0.1;Port=5432;Database=AutomatorTest;User ID=postgres;Password={password};";           

            IDbConnection connection = new NpgsqlConnection(connectionString);

            Postgres = new SqlWriter(connection, queryOptions, logService);

            var userModel = Postgres.Register<UserModel>();

            var p = userModel.RegisteredProperty(p => p.UserID!);

            p.ToDatabaseColumn = NullableULongToDatabaseColumn;

            Postgres.Register<AddressModel>();

            Postgres.Register<UserAddressModel>();

            //delete all rows
            var a = await Postgres.Delete<UserModel>().QueryAsync();

            await InsertNewRows();

            await TestUsingConstants();

            await TestUsingVariableClosure();

            await TestUsingComplexClosure();

            Console.WriteLine("done");

            Console.ReadLine();
        }

        private static async Task InsertNewRows()
        {
            UserModel newUser1 = new UserModel
            {
                UserID = 1,
                UserNames = "a"
            };
            UserModel newUser2 = new UserModel
            {
                UserID = 2,
                UserNames = "b"
            };
            UserModel newUser3 = new UserModel
            {
                UserID = 3,
                UserNames = "c"
            };
            UserModel newUser4 = new UserModel
            {
                UserID = 4,
                UserNames = "d"
            };
            UserModel newUser5 = new UserModel
            {
                UserID = 5,
                UserNames = "e"
            };
            UserModel newUser6 = new UserModel
            {
                UserID = 6,
                UserNames = "f"
            };

            var b = await Postgres.Insert(newUser1).QueryFirstOrDefaultAsync();

            var c = await Postgres.Insert(newUser2).QueryFirstOrDefaultAsync();

            var d = await Postgres.Insert(newUser3).QueryFirstOrDefaultAsync();

            var e = await Postgres.Insert(newUser4).QueryFirstOrDefaultAsync();

            var f = await Postgres.Insert(newUser5).QueryFirstOrDefaultAsync();

            var g = await Postgres.Insert(newUser6).QueryFirstOrDefaultAsync();
        }

        private static async Task TestUsingConstants()
        {
            var a = await Postgres.Select<UserModel>().Where(u => u.UserNames == "a").QueryAsync();

            var b = await Postgres.Update<UserModel>().Where(u => u.UserNames == "z").Set(u => u.UserNames == "a").QueryFirstOrDefaultAsync();

            var c = await Postgres.Delete<UserModel>().Where(u => u.UserNames == "z").QueryAsync();

            var d = await Postgres.Select<UserModel>().Where(u => u.UserID!.Value > 1).QueryAsync();

            var e = await Postgres.Select<UserModel>().Where(u => u.UserID!.Value > 3 || u.UserNames == "b").QueryAsync();

            var f = await Postgres.Select<UserModel>().Where(u => u.UserID!.Value > 4 && u.UserNames == "e").QueryAsync();

            var g = await Postgres.Select<UserModel>().Where(u => u.UserID!.Value == 1 && u.UserNames == "f" || u.UserType > UserType.User).QueryAsync();
        }

        private static async Task TestUsingVariableClosure()
        {
            var b = "b";

            var z = "z";

            var d = "f";

            ulong k = 1;

            ulong l = 3;

            ulong m = 4;

            UserType userType = UserType.Admin;

            var e = await Postgres.Select<UserModel>().Where(u => u.UserNames == b).QueryAsync();

            var f = await Postgres.Update<UserModel>().Set(u => u.UserNames == z).Where(u => u.UserNames == b).QueryFirstOrDefaultAsync();

            var g = await Postgres.Delete<UserModel>().Where(u => u.UserNames == b).QueryAsync();

            var h = await Postgres.Select<UserModel>().Where(u => u.UserID > k).QueryAsync();

            var i = await Postgres.Select<UserModel>().Where(u => u.UserID > l || u.UserNames == z).QueryAsync();

            var j = await Postgres.Select<UserModel>().Where(u => u.UserID > m && u.UserNames == d).QueryAsync();

            var n = await Postgres.Select<UserModel>().Where(u => u.UserID == k || u.UserNames != "d" && u.UserType == userType).QueryAsync();

        }

        private static async Task TestUsingComplexClosure()
        {
            var a = new UserModel { UserNames = "test", UserID = 2 };

            var h = new UserModel { UserNames = "z" };

            var i = new UserModel { UserNames = "f" };

            var b = await Postgres.Select<UserModel>().Where(u => u.UserID == a.UserID).QueryFirstOrDefaultAsync();

            var c = await Postgres.Update<UserModel>().Set(u => u.UserNames == a.UserNames).Where(u => u.UserID == a.UserID).QueryFirstOrDefaultAsync();

            var d = await Postgres.Delete<UserModel>().Where(u => u.UserID == a.UserID).QueryAsync();

            var e = await Postgres.Select<UserModel>().Where(u => u.UserID > a.UserID).QueryAsync();

            var f = await Postgres.Select<UserModel>().Where(u => u.UserID > a.UserID || u.UserNames == h.UserNames).QueryAsync();

            var g = await Postgres.Select<UserModel>().Where(u => u.UserID > a.UserID && u.UserNames == i.UserNames).QueryAsync();

            //the MiaPlaza library will not remove this closure without the .Value
            var j = await Postgres.Select<UserModel>().Where(u => (u.UserID!.Value == 1 && u.UserNames == i.UserNames) || u.UserType > UserType.User).QueryAsync();
        }

        public static object? NullableULongToDatabaseColumn<C>(RegisteredProperty<C> registeredProperty, object value)
        {
            if (value == null) return value;

            return value.ToString();
        }
    }
}