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

            IUserModel newUser = new UserModel
            {
                UserID = 13,
                UserName = "abc"
            };


            //await postgres.InsertAsync<IUserModel, UserModel>(newUser);



            OrderByClause<UserModel> orderBy = new OrderByClause<UserModel>(postgres);

            orderBy.Asc(nameof(UserModel.UserID));
            orderBy.Asc(nameof(UserModel.UserName));
            

            var a = await postgres.GetAsync(u => u.UserID > 2, orderBy);
            var b = await postgres.GetAsync<UserModel>(u => u.UserID < 2);
            var c = await postgres.GetAsync<UserModel>(u => u.UserID >= 2);
            var d = await postgres.GetAsync<UserModel>(u => u.UserID <= 2);
            var e = await postgres.GetAsync<UserModel>(u => u.UserID == 13);

            var f = await postgres.DeleteAsync(newUser as UserModel);

            var g = await postgres.UpdateAsync<UserModel>(u => u.UserName == "changed", u => u.UserID == 13);
            var h = await postgres.UpdateAsync<UserModel>(u => u.UserName == "changedagain");
            e.First().UserName = "xyz";
            var i = await postgres.UpdateAsync(e.First());



            await Task.Delay(-1);
        }

        private static void Postgres_OnSlowQueryDetected(string methodName, TimeSpan timeSpan)
        {
            
        }

 
    }
}
