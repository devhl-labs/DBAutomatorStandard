# Dapper.SqlWriter
This .NET Standard 2.1 library allows you to easily save and retrieve your objects from a database.

## [Test Program](/TestConsole/Program.cs)
Begin by instantiating a QueryOptions object, and pass that into the SqlWriter class.  Then register your database POCO classes with the library.
```csharp
QueryOptions queryOptions = new QueryOptions();

queryOptions.ConnectionString = $"Server=127.0.0.1;Port=5432;Database=AutomatorTest;User ID=postgres;Password={password};";

SqlWriter postgres = new SqlWriter(queryOptions, logService);

postgres.Register<UserModel>();
postgres.Register<AddressModel>();
postgres.Register<UserAddressModel>();
```
 
Now you can save and retrieve your objects using Linq.  
```csharp
//delete all rows in the table
var a = await postgres.Delete<UserModel>().QueryAsync();

//insert a new row
var b = await postgres.Insert(newUser1).QuerySingleOrDefaultAsync();

//update an existing row
newUser1.UserName = "changed";
var h = await postgres.Update(newUser1).QuerySingleOrDefaultAsync();

//update all matching rows
var i = await postgres.Update<UserModel>().Set(u => u.UserName == "changed again").Where(u => u.UserName == "changed").QueryAsync();

//get the required rows
var j = await postgres.Select<UserModel>().Where(u => u.UserID > 2).QueryAsync();
var n = await postgres.Select<UserModel>().Where(u => u.UserID == 2).QueryAsync();
var o = await postgres.Select<UserModel>().Where(u => u.UserID == 2 || u.UserName == "changed again").QueryAsync();
var p = await postgres.Select<UserModel>().Where(u => u.UserID == 2 || u.UserName == "changed again").OrderBy(u => u.UserID).QueryAsync();
```

## Configuring Your Classes
The Register method returns a RegisteredClass object.  You may edit the TableName and ColumnName properties to point your class to the property database object.  You may also decorate your class with attributes.  This library uses five attributes: Key, NotMapped, TableName, ColumnName, and AutoIncrement.  The library will also work with views so you can easily get joins working.  

## Callbacks
Your classes can optionally implement the IDBObject or IDBEvent interfaces, or inherit the DBObject class.  This will add callbacks in your POCO when the library inserts, updates, deletes, or selects your object.

## Compatibility
This library was tested with Postgres.  If this library struggles to insert your data type into your database, set the RegisteredProperty's ToDatabaseColumn function to one of your own functions.  The output of which should be a type that can be inserted into your table.  See the test program for more details and other ways to use this library. 