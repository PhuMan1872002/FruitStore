using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using DepartMentStore.Models;
namespace DepartMentStore.Controllers
{
    public class ListCartController : Controller
    {
        // GET: ListCart
        DepartmentStoreEntities da = new DepartmentStoreEntities();
      

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
        public JsonResult GetCartCount()
        {
            var cart = GetCart();
            return Json(new { cartCount = GetCart().Items.Sum(i => i.Quantity) }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }
        [HttpPost]
        public JsonResult AddToCart(int Product_Id,int quantity)
        {
            var product = da.Products.FirstOrDefault(p => p.Product_Id == Product_Id);

            if (product != null)
            {
                var cart = GetCart();
                cart.AddItem(product, quantity);
            }

            return Json(new { success = true, cartCount = GetCart().Items.Sum(i => i.Quantity),TotalPrice=GetCart().TotalValue });
        }
        [HttpPost]
        public JsonResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCart();
            cart.UpdateQuantity(productId, quantity);
            var totalItems = cart.GetTotalItems();
            var itemTotalPrice = cart.GetItemTotalPrice(productId);
            var TotalPrice = cart.TotalValue;
            return Json(new { totalItems,itemTotalPrice,TotalPrice });
        }
        [HttpPost]
        public JsonResult DeleteCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveFromCart(productId);
            var totalItems = cart.GetTotalItems();
            var TotalPrice = cart.TotalValue;
            return Json(new { totalItems , TotalPrice});
        }
    
        [Authorize]
        public ActionResult DatHang()
        {
            Cart carts = GetCart();
            if (Session["CouponCode"] != null)
                ViewBag.Coupon = Session["CouponCode"].ToString();
            return View(carts);
        }

        [HttpPost]
        public ActionResult DatHang(FormCollection collection)
        {
            Order o = new Order();
            Coupon coupon = new Coupon();
            User c = (User)Session["User"];
            Cart carts = GetCart();
            //lay thong tin sp
            o.userid = c.UserId;
            o.BookingDate = DateTime.Now;
            if(Session["CouponCode"]!=null)
            {
                string CouponCode = Session["CouponCode"].ToString();
                coupon = da.Coupons.First(i => i.CouponCode == CouponCode);
                decimal discountPercentage = (decimal)coupon.DiscountPercentage;

                // Giả sử bạn có giá trị đơn hàng hiện tại (ví dụ: từ session hoặc model)
                decimal totalValue = carts.TotalValue; // Ví dụ giá trị đơn hàng ban đầu là 1 triệu

                // Tính toán số tiền giảm giá và tổng tiền sau khi giảm
                decimal discountAmount = totalValue * discountPercentage/100;
                decimal totalValueAfterDiscount = totalValue - discountAmount;
                o.TotalPrice = totalValueAfterDiscount;
                o.Coupon = coupon;
                Session["CouponCode"] = null;
            }    
            else
                o.TotalPrice = (decimal)carts.TotalValue;
            da.Orders.Add(o);
            da.SaveChanges();
            //Them chi tiet don dat hang
            foreach (var item in carts.Items)
            {
                OrderDetail ctdh = new OrderDetail();
                ctdh.Order_id = o.Order_id;
                Session["OrderId"] = o.Order_id;
                ctdh.Product_id = item.Product.Product_Id;
                if (item.Product.IsOnSale == true)
                    ctdh.UnitPrice = item.Product.Price * (1 - item.Product.DiscountPercentage / 100);
                else
                    ctdh.UnitPrice = item.Product.Price;
                ctdh.Quantity = (short)item.Quantity;
                da.OrderDetails.Add(ctdh);
            }
            da.SaveChanges();
            
            return Redirect(UrlPayment(o.Order_id));
        }
        [HttpPost]
        public JsonResult ApplyCoupon(string couponCode)
        {
            Coupon c = da.Coupons.FirstOrDefault(i => i.CouponCode == couponCode);
            Cart cart = GetCart();
            // Kiểm tra xem couponCode có hợp lệ không
            if (c!=null)
            {
                // Lấy giá trị giảm giá từ coupon
                decimal discountPercentage = (decimal)c.DiscountPercentage;

                // Giả sử bạn có giá trị đơn hàng hiện tại (ví dụ: từ session hoặc model)
                decimal totalValue = cart.TotalValue; // Ví dụ giá trị đơn hàng ban đầu là 1 triệu

                // Tính toán số tiền giảm giá và tổng tiền sau khi giảm
                decimal discountAmount = totalValue * discountPercentage/100;
                decimal totalValueAfterDiscount = totalValue - discountAmount;
                Session["CouponCode"] = couponCode;
                ViewBag.TotalValueAfterDiscount = totalValueAfterDiscount;
                // Trả về JSON chứa các thông tin cần cập nhật
                return Json(new
                {
                    success = true,
                    coupon = couponCode,
                    originalPrice = totalValue,
                    discountAmount = discountPercentage,
                    totalAfterDiscount = totalValueAfterDiscount
                });
            }

            // Nếu mã giảm giá không hợp lệ, trả về JSON báo lỗi
            return Json(new { success = false, message = "Coupon không hợp lệ!" });
        }

        public string UrlPayment( int orderCode)
        {
            var urlPayment = "";
            var cart = GetCart();
            var order = da.Orders.FirstOrDefault(x => x.Order_id == orderCode);
            //Get Config Info
            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Secret Key

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();
            
            var Price = (int)Math.Round((decimal)order.TotalPrice, 0);
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", (Price*100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
                                                                        //if (TypePaymentVN == 1)
                                                                        //{
            //vnpay.AddRequestData("vnp_BankCode", "VNPAYQR");
            //}
            //else if (TypePaymentVN == 2)
            //{
            //vnpay.AddRequestData("vnp_BankCode", "VNBANK");
            //}
            //else if (TypePaymentVN == 3)
            //{
            //    vnpay.AddRequestData("vnp_BankCode", "INTCARD");
            //}

            vnpay.AddRequestData("vnp_CreateDate", order.BookingDate.Value.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng :" + order.Order_id);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.Order_id.ToString()); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            urlPayment = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            //log.InfoFormat("VNPAY URL: {0}", paymentUrl);
            return urlPayment;
        }
        public void SendEmailToUser(string emailId)
        {
            var fromMail = new MailAddress("2151010220man@ou.edu.vn", "PM"); // set your email    
            var fromEmailpassword = "mottambayhailehai1872002"; // Set your password     
            var toEmail = new MailAddress(emailId);

            var smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = new NetworkCredential(fromMail.Address, fromEmailpassword);

            var Message = new MailMessage(fromMail, toEmail);
            Message.Subject = "Booking Order Completed";
            Message.Body = "<br/> Bạn vừa đặt thành công đơn hàng tại Store" +
                           "<br/> Mã đơn là "+Session["OrderId"].ToString();
                          
            Message.IsBodyHtml = true;
            smtp.Send(Message);

        }
        public void SendEmailToAdmin()
        {
            var fromMail = new MailAddress("2151010220man@ou.edu.vn", "PM"); // set your email    
            var fromEmailpassword = "mottambayhailehai1872002"; // Set your password
            User u = da.Users.First(i => i.UserRole.RoleID == 1);
            var toEmail = new MailAddress(u.Email);

            var smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = new NetworkCredential(fromMail.Address, fromEmailpassword);

            var Message = new MailMessage(fromMail, toEmail);
            Message.Subject = "Booking Order Notification";
            Message.Body = "<br/> Có một đơn hàng vừa mới được đặt" +
                           "<br/> Mã đơn là " + Session["OrderId"].ToString();

            Message.IsBodyHtml = true;
            smtp.Send(Message);

        }


        public ActionResult VnpayReturn()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                   
                        vnpay.AddResponseData(s, vnpayData[s]);
                }

                long orderCode = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                String TerminalID = Request.QueryString["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                String bankCode = Request.QueryString["vnp_BankCode"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        var itemOrder = da.Orders.FirstOrDefault(x => x.Order_id == orderCode);
                        if (itemOrder != null)
                        {
                            itemOrder.DeliveryStatus = true;
                            da.Orders.Attach(itemOrder);
                            da.Entry(itemOrder).State = System.Data.Entity.EntityState.Modified;
                            da.SaveChanges();
                        }
                        //Thanh toan thanh cong
                        ViewBag.InnerText = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ";
                        //log.InfoFormat("Thanh toan thanh cong, OrderId={0}, VNPAY TranId={1}", orderId, vnpayTranId);
                    }
                    else
                    {
                        //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                        ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                        //log.InfoFormat("Thanh toan loi, OrderId={0}, VNPAY TranId={1},ResponseCode={2}", orderId, vnpayTranId, vnp_ResponseCode);
                    }
                    //displayTmnCode.InnerText = "Mã Website (Terminal ID):" + TerminalID;
                    //displayTxnRef.InnerText = "Mã giao dịch thanh toán:" + orderId.ToString();
                    //displayVnpayTranNo.InnerText = "Mã giao dịch tại VNPAY:" + vnpayTranId.ToString();
                     ViewBag.ThanhToanThanhCong = "Số tiền thanh toán (VND):" + vnp_Amount.ToString();
                     User c = (User)Session["User"];
                     SendEmailToUser(c.Email);
                     SendEmailToAdmin();
                    //displayBankCode.InnerText = "Ngân hàng thanh toán:" + bankCode;
                }
            }
          
            return View();
        }
       
        public ActionResult XacNhanDonHang()
        {

            return View();
        }
    }
}