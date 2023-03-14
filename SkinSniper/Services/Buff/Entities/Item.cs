using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinSniper.Services.Buff.Entities
{
    internal class Item
    {        
        public int Id { get; set; } 
        public Listing[] Listings { get; set; }
        public Record[] Records { get; set; } 
    }
}
