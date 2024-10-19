using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DepartMentStore.Models
{
    public class Cart
    {
        private List<CartModel> items = new List<CartModel>();

        public void AddItem(Product product, int quantity)
        {
            var item = items.FirstOrDefault(p => p.Product.Product_Id == product.Product_Id);
            if (item == null)
            {
                if (quantity > 0 && quantity != null)
                    items.Add(new CartModel { Product = product, Quantity = quantity });
                else
                    items.Add(new CartModel { Product = product, Quantity = 1 });
            }
            else
            {
                item.Quantity += quantity;
            }
        }
        public void UpdateQuantity(int productId, int quantity)
        {
            var item = items.FirstOrDefault(i => i.Product.Product_Id == productId);
            if (item != null)
            {
                item.Quantity = quantity;
            }
        }
        public int GetTotalItems()
        {
            return items.Sum(i => i.Quantity);
        }
        public decimal GetItemTotalPrice(int productId)
        {
            var item = items.FirstOrDefault(i => i.Product.Product_Id == productId);
            if (item != null)
            {
                return (decimal)(item.Quantity * item.Product.Price);
            }
            return 0;
        }
        public IEnumerable<CartModel> Items => items;
        public void RemoveFromCart(int productId)
        {
            var item = items.FirstOrDefault(i => i.Product.Product_Id == productId);
            if (item != null)
            {
                items.Remove(item);
            }
        }
        public decimal TotalValue => items.Sum(i =>
        {
            decimal productPrice;

            // Kiểm tra nếu sản phẩm đang giảm giá
            if (i.Product.IsOnSale==true && i.Product.DiscountPercentage > 0)
            {
                // Tính giá sau khi giảm
                decimal discountAmount = (decimal)(i.Product.Price * i.Product.DiscountPercentage / 100);
                productPrice = (decimal)(i.Product.Price - discountAmount);
            }
            else
            {
                // Nếu không giảm giá, sử dụng giá gốc
                productPrice = (decimal)i.Product.Price;
            }

            // Tính tổng giá trị cho sản phẩm này (giá x số lượng)
            return productPrice * i.Quantity;
        });


        public void Clear() => items.Clear();

      
    }
}