# Reinforced.Stroke
Simple EntityFramework 6.1 enchancement for strongly typed raw SQL queries

# Usage

Clone, build, then reference ```Reinforced.Stroke.dll```. After that, all your EF6 DbContexts will have method ```Stroke```. Use it as follows:

```csharp
context.Stroke<Order>(x => $"DELETE FROM {x} WHERE {x.Subtotal} = 0");
```

This strongly-typed invokation will produce and execute correct SQL, replacing ```{x}``` with ```Orders``` table name and ```{x.Subtotal}``` with ```[Subtotal]``` column name. All with respect to EF's mappings. So finally, following code will be executed on your DB:

```sql
DELETE FROM [Orders] WHERE [Subtotal] = 0
```

# Use closures

Closures are being correctly calculated and collected to SQL query parameters:

```csharp
var old = DateTime.Today.AddDays(-30);
context.Stroke<Customer>(x => $"UPDATE {x} SET {x.IsActive} = 0 WHERE {x.RegisterDate} < {old}");
```

```sql
UPDATE [Customers] SET [IsActive] = 0 WHERE [RegisterDate] < @p0
```

Where ```@p0``` will be passed as SQL command parameter

# Use multiple tables

```C#
dc.Stroke<Item, Order>((i, o) => $@"
    UPDATE {i} SET {i.Name} = '[FREE] ' + {i.Name} 
    FROM {i}
    INNER JOIN {o} ON {i.OrderId} = {o.Id}
    WHERE {o.Subtotal} = 0"
, fullQualified: true);
```

Parameter ```fullQualified``` set to true enables automatic tables aliasing (aliases will be declared after ```JOIN``` or ```FROM``` keyword):

```sql
UPDATE i SET [i].[Name] = '[FREE] ' + [i].[Name] 
FROM [Goods] [i]
INNER JOIN [Orders] [o] ON [i].[OrderId] = [o].[Id]
WHERE [o].[Total] = 0
```

(columns and tables changed according to EF mappings)
