using System;
using System.Threading.Tasks;
using System.Linq;
using TestDatabaseLibrary;
using static DBAutomatorStandard.Enums;
using System.IO;
using DBAutomatorStandard;

namespace TestConsole
{
    public class Program
    {
        public static async Task Main()
        {
            LogService logService = new LogService();

            string password = File.ReadAllText(@"E:\Desktop\password.txt");

            QueryOptions queryOptions = new QueryOptions
            {
                DataStore = DataStore.PostgreSQL,          
            };

            queryOptions.ConnectionString = $"Server=127.0.0.1;Port=5432;Database=AutomatorTest;User ID=postgres;Password={password};";

            DBAutomator postgres = new DBAutomator(queryOptions, logService);

            postgres.Register(new UserModel
            {
                UserName = "test"
            });

            postgres.Register(new OrderModel());

            postgres.OnSlowQueryDetected += Postgres_OnSlowQueryDetected;

            UserModel newUser1 = new UserModel
            {
                UserID = 1,
                UserName = "abc"
            };
            UserModel newUser2 = new UserModel
            {
                UserID = 2,
                UserName = "abc"
            };
            UserModel newUser3 = new UserModel
            {
                UserID = 3,
                UserName = "abc"
            };
            UserModel newUser4 = new UserModel
            {
                UserID = 4,
                UserName = "abc"
            };
            UserModel newUser5 = new UserModel
            {
                UserID = 5,
                UserName = "abc"
            };
            UserModel newUser6 = new UserModel
            {
                UserID = 6,
                UserName = "abc"
            };

            OrderByClause<UserModel> orderBy = new OrderByClause<UserModel>(postgres);

            orderBy.Asc(nameof(UserModel.UserID));
            orderBy.Asc(nameof(UserModel.UserName));

            var a = await postgres.DeleteAsync<UserModel>();

            var b = await postgres.InsertAsync(newUser1);
            var c = await postgres.InsertAsync(newUser2);
            var d = await postgres.InsertAsync(newUser3);
            var e = await postgres.InsertAsync(newUser4);
            var f = await postgres.InsertAsync(newUser5);
            var g = await postgres.InsertAsync(newUser6);

            newUser1.UserName = "changed";

            var h = await postgres.UpdateAsync(newUser1);

            var i = await postgres.UpdateAsync<UserModel>(u => u.UserName == "changed again", u => u.UserName == "changed");

            var j = await postgres.GetAsync<UserModel>(u => u.UserID > 2);
            var k = await postgres.GetAsync<UserModel>(u => u.UserID < 2);
            var l = await postgres.GetAsync<UserModel>(u => u.UserID >= 2);
            var m = await postgres.GetAsync<UserModel>(u => u.UserID <= 2);
            var n = await postgres.GetAsync<UserModel>(u => u.UserID == 2);
            var o = await postgres.GetAsync<UserModel>(u => u.UserID == 2 || u.UserName == "changed again");
            var p = await postgres.GetAsync<UserModel>(u => u.UserID == 2 || u.UserName == "changed again", orderBy);

            var q = await postgres.DeleteAsync<UserModel>(u => u.UserID == 6);

            var r = await postgres.DeleteAsync(newUser5);





            await Task.Delay(-1);
        }

        private static void Postgres_OnSlowQueryDetected(string methodName, TimeSpan timeSpan)
        {
            
        }

 
    }
}
