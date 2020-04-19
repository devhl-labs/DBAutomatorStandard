# Dapper.SqlWriter 
This .NET Standard 2.1 library allows you to easily save and retrieve your objects from a database.

## Help
Join me on Discord for help.
<iframe src="https://discordapp.com/widget?id=701245583444279328&theme=dark" width="350" height="500" allowtransparency="true" frameborder="0"></iframe>

## [Test Program](/TestConsole/Program.cs)
Begin by instantiating a QueryOptions object, and pass that into the SqlWriter class.  Then register your database POCO classes with the library.
```csharp
QueryOptions queryOptions = new QueryOptions();

queryOptions.ConnectionString = $"Server=127.0.0.1;Port=5432;Database=AutomatorTest;User ID=postgres;Password={password};";

SqlWriter sqlWriter = new SqlWriter(queryOptions, logService);

sqlWriter.Register<UserModel>();

```
 
Now you can save and retrieve your objects using Linq.  
```csharp
//delete all rows in the table
var a = await sqlWriter.Delete<UserModel>().QueryAsync();

//insert a new row
var b = await sqlWriter.Insert(newUser1).QueryFirstAsync();

//update an existing row
newUser1.UserName = "Alice";
var h = await sqlWriter.Update(newUser1).QueryFirstAsync();

//update all matching rows
var i = await sqlWriter.Update<UserModel>().Set(u => u.UserName == "Bob").Where(u => u.UserName == "Alice").QueryAsync();

//get the required rows
var j = await sqlWriter.Select<UserModel>().Where(u => u.UserID > 2).QueryAsync();
var n = await sqlWriter.Select<UserModel>().Where(u => u.UserID == 2).QueryAsync();
var o = await sqlWriter.Select<UserModel>().Where(u => u.UserID == 2 || u.UserName == "Bob").QueryAsync();
var p = await sqlWriter.Select<UserModel>().Where(u => u.UserID == 2 || u.UserName == "Bob").OrderBy(u => u.UserID).QueryAsync();
```

## Configuring Your Classes
The Register method returns a RegisteredClass object.  Use this object to configure table names, columns names, primary keys, and database generated columns.  You may also decorate your class with attributes.  This library uses five attributes: Key, NotMapped, TableName, ColumnName, and AutoIncrement.  The library will also work with views so you can easily get joins working.  

## Callbacks
Your classes can optionally implement the IDBEvent interface.  This will add callbacks in your POCO when the library inserts, updates, deletes, or selects your object.

## Change Tracking
Your classes can inherit the abstract DBObject class.  The library will then track changes to your objects and you can then call the save method on your class.

## Compatibility
This library was tested with Postgres, though Dapper supports more RDMSs.  If a property must be converted or processed before sending it to the database, set the RegisteredProperty's ToDatabaseColumn function to one of your own functions.  The output of which should be a type that can be inserted into your table.  See the test program for more details and other ways to use this library. 