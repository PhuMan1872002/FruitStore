//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DepartMentStore.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.Comments = new HashSet<Comment>();
            this.OrderDetails = new HashSet<OrderDetail>();
        }
    
        public int Product_Id { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> Price { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> Cate_Id { get; set; }
        public Nullable<bool> IsOnSale { get; set; }
        public Nullable<decimal> DiscountPercentage { get; set; }
    
        public virtual Category Category { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Comment> Comments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
