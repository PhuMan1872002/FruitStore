using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DepartMentStore.Models
{
    public class CartModel
    {
        public int CartItemId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}