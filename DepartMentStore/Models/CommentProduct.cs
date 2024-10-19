using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DepartMentStore.Models
{
    public class CommentProduct
    {
        public Product Product { get; set; }
        public IEnumerable<Comment> Comment{ get; set; }
}
}