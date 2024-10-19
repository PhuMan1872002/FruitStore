using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DepartMentStore.Models
{
    public class ProductCart
    {
        public IEnumerable<Product> Products { get; set; }
        public Cart Cart { get; set; }
    }
}