using System;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text.Json;

using devhl.DBAutomator;
using static devhl.DBAutomator.Enums;

using TestDatabaseLibrary;

namespace TestConsole
{
    public class Program
    {
        public static DBAutomator Postgres { get; set; }

        public static async Task Main()
        {
            LogService logService = new LogService();

            string password = File.ReadAllText(@"E:\Desktop\password.txt");

            QueryOptions queryOptions = new QueryOptions
            {
                DataStore = DataStore.PostgreSQL          
            };

            queryOptions.ConnectionString = $"Server=127.0.0.1;Port=5432;Database=AutomatorTest;User ID=postgres;Password={password};";

            Postgres = new DBAutomator(queryOptions, logService);

            Postgres.Register<UserModel>();

            Postgres.Register<AddressModel>();

            Postgres.Register<UserAddressModel>();

            //delete all rows
            var a = await Postgres.DeleteAsync<UserModel>();

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

            var b = await Postgres.InsertAsync(newUser1);

            var c = await Postgres.InsertAsync(newUser2);

            var d = await Postgres.InsertAsync(newUser3);

            var e = await Postgres.InsertAsync(newUser4);

            var f = await Postgres.InsertAsync(newUser5);

            var g = await Postgres.InsertAsync(newUser6);
        }

        private static async Task TestUsingConstants()
        {
            var a = await Postgres.GetAsync<UserModel>(u => u.UserNames == "a");

            var b = await Postgres.UpdateAsync<UserModel>(u => u.UserNames == "z", u => u.UserNames == "a");

            var c = await Postgres.DeleteAsync<UserModel>(u => u.UserNames == "z");

            var d = await Postgres.GetAsync<UserModel>(u => u.UserID > 1);

            var e = await Postgres.GetAsync<UserModel>(u => u.UserID > 3 || u.UserNames == "b");

            var f = await Postgres.GetAsync<UserModel>(u => u.UserID > 4 && u.UserNames == "e");

            var g = await Postgres.GetAsync<UserModel>(u => u.UserID == 1 && u.UserNames == "f" || u.UserType > UserType.User);
        }

        private static async Task TestUsingVariableClosure()
        {
            var a = "a";

            var b = "b";

            var z = "z";

            var d = "f";

            ulong k = 1;

            ulong l = 3;

            ulong m = 4;

            UserType userType = UserType.Admin;

            var e = await Postgres.GetAsync<UserModel>(u => u.UserNames == b);

            var f = await Postgres.UpdateAsync<UserModel>(u => u.UserNames == z, u => u.UserNames == b);

            var g = await Postgres.DeleteAsync<UserModel>(u => u.UserNames == b);

            var h = await Postgres.GetAsync<UserModel>(u => u.UserID > k);

            var i = await Postgres.GetAsync<UserModel>(u => u.UserID > l || u.UserNames == z);

            var j = await Postgres.GetAsync<UserModel>(u => u.UserID > m && u.UserNames == d);

            var n = await Postgres.GetAsync<UserModel>(u => u.UserID == k || u.UserNames != "d" && u.UserType == userType);
        }

        private static async Task TestUsingComplexClosure()
        {
            var a = new UserModel { UserNames = "test", UserID = 2 };

            var h = new UserModel { UserNames = "z" };

            var i = new UserModel { UserNames = "f" };

            var b = await Postgres.GetFirstOrDefaultAsync<UserModel>(u => u.UserID == a.UserID);

            var c = await Postgres.UpdateAsync<UserModel>(u => u.UserNames == a.UserNames, u => u.UserID == a.UserID);

            var d = await Postgres.DeleteAsync<UserModel>(u => u.UserID == a.UserID);

            var e = await Postgres.GetAsync<UserModel>(u => u.UserID > a.UserID);

            var f = await Postgres.GetAsync<UserModel>(u => u.UserID > a.UserID || u.UserNames == h.UserNames);

            var g = await Postgres.GetAsync<UserModel>(u => u.UserID > a.UserID && u.UserNames == i.UserNames);

            var j = await Postgres.GetAsync<UserModel>(u => (u.UserID == 1 && u.UserNames == i.UserNames) || u.UserType > UserType.User);
        }
    }
}
