using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reinforced.SqlStroke.Demo.Data
{
    public class Item
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Cost { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; }
    }
}
