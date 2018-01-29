using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reinforced.SqlStroke.Demo.Data
{
    public class Order
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Subtotal { get; set; }

        public ICollection<Item> Items { get; set; }

        public int CustomerId { get; set; }

        public Customer Customer { get; set; }

        public Order()
        {
            Items = new HashSet<Item>();
        }
    }
}
