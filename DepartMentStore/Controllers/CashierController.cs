using DepartMentStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rotativa;
namespace DepartMentStore.Controllers
{
    [Authorize(Roles="Employee")]
    public class CashierController : Controller
    {
        // GET: Cashier
        DepartmentStoreEntities da = new DepartmentStoreEntities();
        public ActionResult Index(string searchString,int ?id)
        {
            
            IEnumerable<Product> ds;
            if (!String.IsNullOrEmpty(searchString)) // kiểm tra chuỗi tìm kiếm có rỗng/null hay không
            {
                ds = da.Products.OrderByDescending(s => s.Product_Id).Where(s => s.Name.Contains(searchString) || s.Category.Name.Contains(searchString)).ToList();
            }
            else if (id != null)
                ds = da.Products.Where(s => s.Cate_Id == id).ToList();
            else
                ds = da.Products.OrderByDescending(s => s.Product_Id).ToList();
            var cart = GetCart();
            var ProductCart = new ProductCart
            {
                Products = ds,
                Cart = cart
            };
            return View(ProductCart);
        }

        public ActionResult GetCartItems()
        {
            var cart = GetCart(); // Lấy giỏ hàng từ session
            var viewModel = new ProductCart
            {
                Cart = cart
            };

            return PartialView("_CartItems", viewModel); // Trả về nội dung giỏ hàng dưới dạng HTML
        }
        private Cart GetCart()
        {
            var cart = (Cart)Session["Cart"];
            if (cart == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }
        [HttpPost]
        public JsonResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCart();
            cart.UpdateQuantity(productId, quantity);
            var itemTotalPrice = cart.GetItemTotalPrice(productId);
            return Json(new { success = true,itemTotalPrice=itemTotalPrice });
        }
        public JsonResult GetCartCount()
        {
            var cart = GetCart();
            return Json(new { cartCount = GetCart().Items.Sum(i => i.Quantity) }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult AddToCart(int Product_Id)
        {
            var product = da.Products.FirstOrDefault(p => p.Product_Id == Product_Id);

            if (product != null)
            {
                var cart = GetCart();
                cart.AddItem(product,1);
               
            }

            return Json(new { success = true, cartCount = GetCart().Items.Sum(i => i.Quantity), TotalPrice = GetCart().TotalValue });
        }

        [HttpPost]
        public JsonResult DeleteCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveFromCart(productId);
            var TotalPrice = cart.TotalValue;
            return Json(new
            {
                success = true,
                totalPrice=TotalPrice,
                message = "Product removed successfully."
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PrintOrder()
        {
            // Lấy danh sách giỏ hàng từ Session hoặc Model
            var cart = Session["Cart"] as Cart;

            if (cart != null)
            {
                var pdf = new ViewAsPdf("OrderReceipt", cart)
                {
                    FileName = "OrderReceipt.pdf"
                };
                Order o = new Order();
                User c = (User)Session["User"];
                Cart carts = GetCart();
                //lay thong tin sp
                o.userid = c.UserId;
                o.BookingDate = DateTime.Now;
                o.TotalPrice = cart.TotalValue;
                da.Orders.Add(o);
                da.SaveChanges();
                //Them chi tiet don dat hang
                foreach (var item in carts.Items)
                {
                    OrderDetail ctdh = new OrderDetail();
                    ctdh.Order_id = o.Order_id;
                    ctdh.Product_id = item.Product.Product_Id;
                    if (item.Product.IsOnSale == true)
                        ctdh.UnitPrice = item.Product.Price * (1 - item.Product.DiscountPercentage / 100);
                    else
                        ctdh.UnitPrice = item.Product.Price;
                    ctdh.Quantity = (short)item.Quantity;
                    da.OrderDetails.Add(ctdh);
                }
                da.SaveChanges();
                Session["Cart"] = null;
                return pdf;
            }
            else
            {
                return new HttpStatusCodeResult(400, "No cart found.");
            }

        }

    }
}