# Dapper.SqlWriter 
This .NET Standard 2.1 library allows you to easily save and retrieve your objects from a database.

## Help  
![Discord Banner 2](https://discordapp.com/api/guilds/701245583444279328/widget.png?style=banner2)

## [Test Program](/TestConsole/Program.cs)
Begin by instantiating and configuring the SqlWriter class. Then register your database classes with the library.
```csharp
SqlWriterConfiguration sqlWriterConfiguration = new SqlWriterConfiguration();

SqlWriterConfiguration.ConnectionString = $"Server=127.0.0.1;Port=5432;Database=MyDatabase;User ID=postgres;Password=MyPassword;";

SqlWriter sqlWriter = new SqlWriter(sqlWriterConfiguration, logService);

//configure how your C# types get converted to database types if necesssary
sqlWriter.AddPropertyMap(new PropertyMap(typeof(ulong?), NullableULongToDatabaseColumn));

sqlWriter.AddPropertyMap(new PropertyMap(typeof(ulong), NullableULongToDatabaseColumn));

//if all your tables and column names are upper or lower, change this
sqlWriter.Capitalization = Capitalization.Default;

//the user object will give more options
var user = sqlWriter.Register<User>();

```
 
Now you can save and retrieve your objects using Linq.  
```csharp
//delete all rows in the table
var a = await sqlWriter.Delete<User>().QueryAsync();

//insert a new row
var b = await sqlWriter.Insert(user).QueryFirstAsync();

//update an existing row
user.UserName = "Alice";
var c = await sqlWriter.Update(user).QueryFirstAsync();

//update all matching rows
var d = await sqlWriter.Update<User>().Set(u => u.UserName == "Bob").Where(u => u.UserName == "Alice").QueryAsync();

//get the required rows
var e = await sqlWriter.Select<User>().Where(u => u.UserID > 2).QueryAsync();
var f = await sqlWriter.Select<User>().Where(u => u.UserID == 2).QueryAsync();
var g = await sqlWriter.Select<User>().Where(u => u.UserID == 2 || u.UserName == "Bob").QueryAsync();
var h = await sqlWriter.Select<User>().Where(u => u.UserID == 2 || u.UserName == "Bob").OrderBy(u => u.UserID).QueryAsync();
```

## Configuring Your Classes
The Register method returns a RegisteredClass object.  Use this object to configure table names, columns names, primary keys, and database generated columns.  You may also decorate your class with attributes.  This library uses five attributes: Key, NotMapped, TableName, ColumnName, and AutoIncrement.  The library will also work with views so you can easily get joins working.  

## Change Tracking
To enable change tracking, modify your class to inherit DBObject.  This will give your class a Save method.  When called, this method will either insert or update the object into your database.  If no changes were made, no action will occur.

## Callbacks
Your classes can implement the IDBEvent interface.  This will add callbacks to your class when the library inserts, updates, or deletes your object.

## Compatibility
This library was tested with Postgres, though it should work with any relational database supported by Dapper.  If a property must be converted or processed before sending it to the database, set the RegisteredProperty's ToDatabaseColumn function to one of your own functions.  The output of which should be a type that can be inserted into your table.  See the test program for more details and other ways to use this library. 