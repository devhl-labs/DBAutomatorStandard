# devhl.DBAutomator
This .NET Standard 2.1 library allows you to easily save and retrieve your objects from a database.

## [TestConsole](/TestConsole)
This console program shows you how to use this library.  Begin by instantiating a QueryOptions object, and pass that into the DBAutomator class.  Then register your database POCO classes with the library.
```csharp
QueryOptions queryOptions = new QueryOptions
{
    DataStore = DataStore.PostgreSQL,          
};

queryOptions.ConnectionString = $"Server=127.0.0.1;Port=5432;Database=AutomatorTest;User ID=postgres;Password={password};";

DBAutomator postgres = new DBAutomator(queryOptions, logService);

postgres.Register(new UserModel());
postgres.Register(new AddressModel());
postgres.Register(new UserAddressModel());
```

Now you can save and retrieve your objects using Linq.  
```csharp
//delete the entire table
var a = await postgres.DeleteAsync<UserModel>();

//insert a new row
var b = await postgres.InsertAsync(newUser1);

//update an existing row
newUser1.UserName = "changed";

var h = await postgres.UpdateAsync(newUser1);

//update all matching rows
var i = await postgres.UpdateAsync<UserModel>(u => u.UserName == "changed again", u => u.UserName == "changed");

//get the required rows
var j = await postgres.GetAsync<UserModel>(u => u.UserID > 2);
var n = await postgres.GetAsync<UserModel>(u => u.UserID == 2);
var o = await postgres.GetAsync<UserModel>(u => u.UserID == 2 || u.UserName == "changed again");
var p = await postgres.GetAsync(u => u.UserID == 2 || u.UserName == "changed again", orderBy);
```

