using System;
using Reinforced.SqlStroke.Demo.Data;
using Reinforced.Stroke;

namespace Reinforced.SqlStroke.Demo
{
    class Program
    {
        static void Main(string[] args)
        {

            using (var dc = new MyDbContext())
            {
                //----------
                //dc.Stroke<Order>(x => $"DELETE FROM {x} WHERE {x.Subtotal} = 0");

                //----------
                //var old = DateTime.Today.AddDays(-30);
                //dc.Stroke<Customer>(x => $"UPDATE {x} SET {x.IsActive} = 0 WHERE {x.RegisterDate} < {old}");

                //----------
                dc.Stroke<Item, Order>((i, o) => $@"
UPDATE {i} SET {i.Name} = '[FREE] ' + {i.Name} 
FROM {i}
INNER JOIN {o} ON {i.OrderId} = {o.Id}
WHERE {o.Subtotal} = 0"
, fullQualified:true);

            }
        }
    }
}
