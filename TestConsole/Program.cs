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

        public static SqlWriter SqlWriter { get; set; }

#nullable enable

        public static async Task Main()
        {
            SqlWriter = ConfigureSqlWriter();

            ConfigureClasses();

            await TestUsingConstants();

            await TestUsingVariableClosure();

            await TestUsingComplexClosure();

            Console.WriteLine("done");

            Console.ReadLine();
        }

        public static SqlWriter ConfigureSqlWriter()
        {
            ILogger logService = new LogService();

            string password = File.ReadAllText(@"E:\Desktop\password.txt");

            QueryOptions queryOptions = new QueryOptions();

            string connectionString = $"Server=127.0.0.1;Port=5432;Database=AutomatorTest;User ID=postgres;Password={password};";           

            IDbConnection connection = new NpgsqlConnection(connectionString);

            return new SqlWriter(connection, queryOptions, logService);
        }

        public static void ConfigureClasses()
        {
            var userModel = SqlWriter.Register<UserModel>().Key(u => u.UserID).NotMapped(u => u.NotMappedProperty).ColumnName(u => u.SomeLongNumber!, "SomeBigInt");

            userModel.RegisteredProperty(p => p.UserID!).ToDatabaseColumn = NullableULongToDatabaseColumn;

            userModel.RegisteredProperty(p => p.SomeLongNumber!).ToDatabaseColumn = NullableULongToDatabaseColumn;
        }

        private static async Task InsertNewRows()
        {
            UserModel newUser1 = new UserModel
            {
                UserID = 1,
                FirstName = "Adam",
                LastName = "Warlock",
                NotMappedProperty = "not mapped property",
                UserType = UserType.User
            };

            UserModel newUser2 = new UserModel
            {
                UserID = 2,
                FirstName = "Bucky",
                LastName = "Barnes",
                NotMappedProperty = "abcdef",
                UserType = UserType.User
            };
            UserModel newUser3 = new UserModel
            {
                UserID = 3,
                FirstName = "Chalie",
                LastName = "Cox",
                //NotMappedProperty = "not mapped property",
                UserType = UserType.Admin,
                SomeLongNumber = 1
            };
            UserModel newUser4 = new UserModel
            {
                UserID = 4,
                FirstName = "Dexter",
                LastName = "Morgan",
                NotMappedProperty = "some value",
                UserType = UserType.Admin,
                SomeLongNumber = 1000
            };

            UserModel newUser5 = new UserModel
            {
                UserID = 5,
                FirstName = "Debra",
                LastName = "Morgan",
                NotMappedProperty = "not mapped property",
                UserType = UserType.Admin,
                SomeLongNumber = 1000
            };

            UserModel newUser6 = new UserModel
            {
                UserID = 6,
                FirstName = "Electra",
                LastName = "Natchios",
                //NotMappedProperty = "not mapped property",
                UserType = UserType.User
            };
            UserModel newUser7 = new UserModel
            {
                UserID = 7,
                FirstName = "Fool",
                LastName = "Killer",
                NotMappedProperty = "not mapped property",
                UserType = UserType.User
            };

            var b = await SqlWriter.Insert(newUser1).QueryFirstAsync();

            var c = await SqlWriter.Insert(newUser2).QueryFirstAsync();

            var d = await SqlWriter.Insert(newUser3).QueryFirstAsync();

            var e = await SqlWriter.Insert(newUser4).QueryFirstAsync();

            var f = await SqlWriter.Insert(newUser5).QueryFirstAsync();

            var g = await SqlWriter.Insert(newUser6).QueryFirstAsync();

            var h = await SqlWriter.Insert(newUser7).QueryFirstAsync();
        }

        private static async Task TestUsingConstants()
        {
            var a = await SqlWriter.Delete<UserModel>().QueryAsync();

            await InsertNewRows();

            var b = await SqlWriter.Select<UserModel>().QueryAsync();

            var c = await SqlWriter.Select<UserModel>().Limit(2).Where(u => u.UserType == UserType.User).QueryAsync();

            var d = await SqlWriter.Select<UserModel>().Where(u => u.FirstName == "Adam").QueryFirstAsync();

            var e = await SqlWriter.Select<UserModel>().Where(u => u.UserType == UserType.Admin && u.SomeLongNumber == 1000 && (u.FirstName == "Debra" || u.FirstName == "Dexter")).QueryAsync();

            var f = await SqlWriter.Select<UserModel>().Where(u => u.SomeLongNumber > 0).QueryAsync();

            var g = await SqlWriter.Select<UserModel>().Where(u => u.SomeLongNumber > 0 || (u.FirstName == "Adam" && u.LastName == "Warlock") || u.FirstName == "Fool").QueryAsync();

            d.SomeLongNumber = 9000;

            var h = await SqlWriter.Update(d).QueryFirstAsync();

            var i = await SqlWriter.Update<UserModel>().Set(u => u.SomeLongNumber == 42).Where(u => u.FirstName == "Dexter" && u.LastName == "Morgan").QueryFirstAsync();

            var j = await SqlWriter.Delete(h).QueryAsync();

            var k = await SqlWriter.Delete<UserModel>().Where(u => u.UserID > 5).QueryAsync();
        }

        private static async Task TestUsingVariableClosure()
        {
            int zero = 0;

            int one = 1;

            int two = 2;

            ulong five = 5;

            int fortytwo = 42;

            long onethousand = 1000;

            long ninethousand = 9000;

            string adam = "Adam";

            string warlock = "Warlock";

            string debra = "Debra";

            string dexter = "Dexter";

            string morgan = "Morgan";

            string fool = "Fool";

            var a = await SqlWriter.Delete<UserModel>().QueryAsync();

            await InsertNewRows();

            var b = await SqlWriter.Select<UserModel>().QueryAsync();

            UserType userTypeZero = (UserType) zero;

            UserType userTypeOne = (UserType) one;

            var c = await SqlWriter.Select<UserModel>().Limit(two).Where(u => u.UserType == userTypeZero).QueryAsync(); //casting in the Where clause is not supported by the Mia Plaza library

            var d = await SqlWriter.Select<UserModel>().Where(u => u.FirstName == adam).QueryFirstAsync();

            var e = await SqlWriter.Select<UserModel>().Where(u => u.UserType == userTypeOne && u.SomeLongNumber == onethousand && (u.FirstName == debra || u.FirstName == dexter)).QueryAsync();

            var f = await SqlWriter.Select<UserModel>().Where(u => u.SomeLongNumber > zero).QueryAsync();

            var g = await SqlWriter.Select<UserModel>().Where(u => u.SomeLongNumber > zero || (u.FirstName == adam && u.LastName == warlock) || u.FirstName == fool).QueryAsync();

            d.SomeLongNumber = ninethousand;

            var h = await SqlWriter.Update(d).QueryFirstAsync();

            var i = await SqlWriter.Update<UserModel>().Set(u => u.SomeLongNumber == fortytwo).Where(u => u.FirstName == dexter && u.LastName == morgan).QueryFirstAsync();

            var j = await SqlWriter.Delete(h).QueryAsync();

            var k = await SqlWriter.Delete<UserModel>().Where(u => u.UserID > five).QueryAsync();
        }

        private static async Task TestUsingComplexClosure()
        {
            UserModel adam = new UserModel { FirstName = "Adam", LastName = "Warlock", SomeLongNumber = 0 };

            UserModel debra = new UserModel { FirstName = "Debra", UserType = UserType.Admin, SomeLongNumber = 1000 };

            UserModel dexter = new UserModel { FirstName = "Dexter", LastName = "Morgan", UserType = UserType.Admin, SomeLongNumber = 1000 };

            UserModel fool = new UserModel { FirstName = "Fool", SomeLongNumber = 42 };

            UserModel five = new UserModel { UserID = 5 };

            var a = await SqlWriter.Delete<UserModel>().QueryAsync();

            await InsertNewRows();

            var b = await SqlWriter.Select<UserModel>().QueryAsync();

            var c = await SqlWriter.Select<UserModel>().Limit(2).Where(u => u.UserType == adam.UserType).QueryAsync();

            var d = await SqlWriter.Select<UserModel>().Where(u => u.FirstName == adam.FirstName).QueryFirstAsync();

            var e = await SqlWriter.Select<UserModel>().Where(u => u.UserType == dexter.UserType && u.SomeLongNumber == dexter.SomeLongNumber && (u.FirstName == debra.FirstName || u.FirstName == dexter.FirstName)).QueryAsync();

            var f = await SqlWriter.Select<UserModel>().Where(u => u.SomeLongNumber > adam.SomeLongNumber.Value).QueryAsync();            

            var g = await SqlWriter.Select<UserModel>().Where(u => u.SomeLongNumber > adam.SomeLongNumber.Value || (u.FirstName == adam.FirstName && u.LastName == adam.LastName) || u.FirstName == fool.FirstName).QueryAsync();

            d.SomeLongNumber = 9000;

            var h = await SqlWriter.Update(d).QueryFirstAsync();

            var i = await SqlWriter.Update<UserModel>().Set(u => u.SomeLongNumber == fool.SomeLongNumber.Value).Where(u => u.FirstName == dexter.FirstName && u.LastName == dexter.LastName).QueryFirstAsync();

            var j = await SqlWriter.Delete(h).QueryAsync();

            var k = await SqlWriter.Delete<UserModel>().Where(u => u.UserID > five.UserID).QueryAsync();
        }

        public static object? NullableULongToDatabaseColumn<C>(RegisteredProperty<C> registeredProperty, object value)
        {
            if (value == null) return value;

            return value.ToString();
        }
    }
}