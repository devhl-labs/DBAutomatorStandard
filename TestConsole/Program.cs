using DBAutomatorLibrary;
using System;
using System.Threading.Tasks;
using System.Linq;
using TestDatabaseLibrary;
using System.Collections.Generic;
using static DBAutomatorStandard.Enums;

namespace TestConsole
{
    public class Program
    {
        public static async Task Main()
        {
            LogService logService = new LogService();

            DBAutomator postgres = new DBAutomator(DataStore.PostgreSQL, "Server=127.0.0.1;Port=5432;Database=AutomatorTest;User ID=postgres;Password=*;", logger: logService);

            postgres.Register(new UserModel
            {
                UserName = "test"
            });

            postgres.Register(new OrderModel());

            postgres.OnSlowQueryDetected += Postgres_OnSlowQueryDetected;

            IUserModel newUser = new UserModel
            {
                UserID = 11,
                UserName = "another guy"
            };


            await postgres.InsertAsync<IUserModel, UserModel>(newUser);

            //var user = await postgres.GetListAsync<IUserModel, UserModel>(a => a.UserName == "another guy");

            //var all = await postgres.GetListAsync<IUserModel, UserModel>();

            //IOrderModel orderModel = new OrderModel
            //{
            //    CustomerID = 1
            //};

            //await postgres.InsertAsync<IOrderModel, OrderModel>(orderModel);

            //List<IOrderModel> deleted = await postgres.DeleteAsync<IOrderModel, OrderModel>();

            //List<IUserModel> updated = await postgres.UpdateAsync<IUserModel, UserModel>(u => u.UserName == "b", u => u.UserID > 6);

            var user = await postgres.GetAsync<IUserModel, UserModel>(u => u.UserID == 1);

            user.UserName = "b";

            await postgres.UpdateAsync<IUserModel, UserModel>(u => u.UserName == "b", u => u.UserID == 1);


            //IOrderModel orderModel = new OrderModel
            //{
            //    CustomerID = 1
            //};

            //await postgres.InsertAsync<IOrderModel, OrderModel>(orderModel);

            //await postgres.InsertAsync<IOrderModel, OrderModel>(orderModel);

            //IOrderModel orderModel = await postgres.GetAsync<IOrderModel, OrderModel>(o => o.OrderID == 44);

            ////await postgres.InsertAsync<IOrderModel, OrderModel>(orderModel);

            ////await postgres.InsertAsync<IOrderModel, OrderModel>(orderModel);

            ////orderModel.CustomerID = 2;

            ////await postgres.UpdateAsync<IOrderModel, OrderModel>(orderModel);

            //await postgres.DeleteAsync<IOrderModel, OrderModel>(orderModel);


            //IUserModel userModel = new UserModel
            //{
            //    UserID = 2,
            //    UserName = "test"
            //};

            //await postgres.InsertAsync<IUserModel, UserModel>(userModel);

            //var a = await postgres.GetAsync<IUserModel, UserModel>(u => u.UserID == 2);

            await Task.Delay(-1);
        }

        private static void Postgres_OnSlowQueryDetected(string methodName, TimeSpan timeSpan)
        {
            
        }

 
    }
}
