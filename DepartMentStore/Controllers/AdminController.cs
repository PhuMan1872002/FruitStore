using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DepartMentStore.Models;
namespace DepartMentStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        DepartmentStoreEntities da = new DepartmentStoreEntities();
        private readonly CloudinaryService _cloudinaryService = new CloudinaryService();
        // GET: Admin

        public ActionResult Index()
        {
            var revenues = da.OrderDetails
               .Where(od => od.Order.BookingDate.Value.Month == DateTime.Now.Month && od.Order.BookingDate.Value.Year == DateTime.Now.Year)
               .GroupBy(od => new { od.Product.Name })
               .Select(g => new
               {
                   ProductName = g.Key.Name,
                   Revenue = g.Sum(od => od.Quantity * od.UnitPrice)
               })
               .ToList();
            var revenuesYear = da.OrderDetails
               .Where(od => od.Order.BookingDate.Value.Year == DateTime.Now.Year)
               .GroupBy(od => new { od.Product.Name })
               .Select(g => new
               {
                   ProductName = g.Key.Name,
                   Revenue = g.Sum(od => od.Quantity * od.UnitPrice)
               })
               .ToList();
            var revenuesDate = da.OrderDetails
               .Where(od => od.Order.BookingDate.Value.Day == DateTime.Now.Day && 
               od.Order.BookingDate.Value.Month==DateTime.Now.Month && 
               od.Order.BookingDate.Value.Year==DateTime.Now.Year)
               .GroupBy(od => new { od.Product.Name })
               .Select(g => new
               {
                   ProductName = g.Key.Name,
                   Revenue = g.Sum(od => od.Quantity * od.UnitPrice)
               })
               .ToList();
            User u = (User)Session["User"];
            ViewBag.MonthRevenue = revenues.Sum(i => i.Revenue);
            ViewBag.YearRevenue = revenuesYear.Sum(i => i.Revenue);
            ViewBag.revenuesDate = revenuesDate.Sum(i => i.Revenue);
            return View(u);
        }

        // GET: Admin/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        
        public ActionResult ListProducts()
        {
            IEnumerable<Product> ds = da.Products.OrderByDescending(s => s.Product_Id);
            return View(ds);
        }
        public ActionResult ListCoupons()
        {
            IEnumerable<Coupon> ds = da.Coupons.OrderByDescending(s => s.CouponCode);
            return View(ds);
        }
        public ActionResult CreateCoupon(FormCollection collection)
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateCoupon(HttpPostedFileBase ImageUpload, FormCollection collection)
        {
            Coupon p = new Coupon();
            if (ImageUpload != null)
            {
                var imageUrl = _cloudinaryService.UploadImage(ImageUpload);
                p.Image = imageUrl;
            }
            else
                p.Image = "loi-hinh-anh.jpg";
            //Chưa có NCC là LSP
            //Gán giá trị MaNCC, MaLSP
            string CouponCode = collection["CouponCode"].ToString();
            Coupon c = da.Coupons.FirstOrDefault(i => i.CouponCode == CouponCode);
            if (c == null)
                p.CouponCode = collection["CouponCode"].ToString();
            else
            {
                ViewBag.SameCoupon = "Trùng mã giảm giá";
                return View();
            }
            p.Description = collection["Description"].ToString();
            bool IsOnSale = collection["Status"] == "on";
            p.IsActive = IsOnSale;
            p.DiscountPercentage = decimal.Parse(collection["DiscountPercentage"]);
            p.ExpiryDate = DateTime.Parse(collection["ExpireDate"].ToString());
            // Thêm SP vào bảng SanPham
            da.Coupons.Add(p);

            //Cập nhật thay đổi db
            da.SaveChanges();

            //Hiển thị DSSP
            return RedirectToAction("ListCoupons");
        }
        public ActionResult EditCoupon(int id)
        {
            try
            {
                Coupon p = da.Coupons.First(s => s.Id == id);
                return View(p);
            }
            catch (Exception)
            {

                return RedirectToAction("ListCoupons");
            }
        }
        [HttpPost]
        public ActionResult EditCoupon(HttpPostedFileBase ImageUpload, int id, FormCollection collection)
        {
            try
            {
                Coupon p = da.Coupons.First(s => s.Id==id);
                if(collection["Description"]!=null)
                    p.Description = collection["Description"].ToString();
                string CouponCode = collection["CouponCode"].ToString();
                p.CouponCode = CouponCode;
                bool IsOnSale = collection["Status"] == "on";
                p.IsActive = IsOnSale;
                if(collection["DiscountPercentage"]!=null)
                    p.DiscountPercentage = decimal.Parse(collection["DiscountPercentage"]);
                if(collection["ExpireDate"]!=null)
                    p.ExpiryDate = DateTime.Parse(collection["ExpireDate"].ToString());
                if (ImageUpload != null)
                {
                    var imageUrl = _cloudinaryService.UploadImage(ImageUpload);
                    p.Image = imageUrl;
                }

                //Lưu xuống da
                da.SaveChanges();

                return RedirectToAction("ListCoupons");
            }
            catch//chưa xử lý bắt lỗi
            {

                return View();
            }
        }
        public ActionResult EditComment(int id)
        {
            try
            {
                Comment p = da.Comments.First(s => s.Comment_id == id);
                return View(p);
            }
            catch (Exception)
            {

                return RedirectToAction("ListComment");
            }
        }
        [HttpPost]
        public ActionResult EditComment(int id, FormCollection collection)
        {
            try
            {
                Comment p = da.Comments.First(s => s.Comment_id == id);
               
                p.Content = collection["Content"].ToString();
                
               
                //Lưu xuống da
                da.SaveChanges();

                return RedirectToAction("ListComment");
            }
            catch//chưa xử lý bắt lỗi
            {

                return View();
            }
        }
        public ActionResult ListUser()
        {
            IEnumerable<User> ds = da.Users.OrderByDescending(s => s.UserId);
            return View(ds);
        }
        public ActionResult AdminInfo()
        {
            return View();
        }
        public ActionResult ListComment()
        {
            IEnumerable<Comment> ds = da.Comments.OrderByDescending(s => s.Comment_id);
            return View(ds);
        }
        public ActionResult ListOrder()
        {
            IEnumerable<Order> ds = da.Orders.OrderByDescending(s => s.Order_id);
            return View(ds);
        }
        public ActionResult OrderDetail(int id)
        {
            IEnumerable<OrderDetail> ds = da.OrderDetails.Where(s => s.Order_id==id);
            ViewBag.OrderId = id; 
            return View(ds);
        }
        public ActionResult EditOrder(int id)
        {
            try
            {
                Order p = da.Orders.First(s => s.Order_id == id);
                return View(p);
            }
            catch (Exception)
            {

                return RedirectToAction("ListOrder");
            }

        }

        //Xử lý cập nhật SP
        [HttpPost]
        public ActionResult EditOrder(int id, FormCollection collection)
        {

            try
            {
                // Xác định SP cần sửa trong da
                Order p = da.Orders.First(s => s.Order_id == id);
                bool DeliveryStatus = collection["DeliveryStatus"] == "on";
                //Thực hiện cập nhật da
                if (collection["DeliveryStatus"] != null)
                    p.DeliveryStatus = DeliveryStatus;
                if (collection["DeliveryDate"] != null)
                {
                    // Parse the DeliveryDate from the collection as a DateTime
                    DateTime deliveryDate = DateTime.Parse(collection["DeliveryDate"].ToString());

                    // Convert the DateTime back to a string with the desired format
                    string deliveryDateString = deliveryDate.ToString("dd MMM yyyy"); // Example format

                    // Assign the DateTime to p.DeliveryDate and the formatted string to DeliveryDate
                    p.DeliveryDate = deliveryDate;
                    SendEmailToUser(p.User.Email, p.Order_id.ToString(),deliveryDateString);

                }
                //Lưu xuống da
                da.SaveChanges();

                return RedirectToAction("ListOrder");
            }
            catch//chưa xử lý bắt lỗi
            {

                return View();
            }
        }
        public void SendEmailToUser(string emailId,string OrderId,string deliveryDate)
        {
            var fromMail = new MailAddress("2151010220man@ou.edu.vn", "Store Fruitkha"); // set your email    
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
            Message.Subject = "STORE XIN THÔNG BÁO NGÀY NHẬN HÀNG";
            Message.Body = "<br/> Chúng tôi xin thông báo chuẩn bị giao đơn hàng bạn đã đặt tại store" +
                           "<br/> Mã đơn là " + OrderId+
                           "</br> Ngày giao là "+ deliveryDate;

            Message.IsBodyHtml = true;
            smtp.Send(Message);

        }
        public ActionResult EditUser(int id)
        {
            try
            {
                User p = da.Users.First(s => s.UserId == id);
                ViewData["StatusList"] = new SelectList(new[]
                {
                        new { Value = "false", Text = "Normal" },
                        new { Value = "true", Text = "Locked" }
                }, "Value", "Text", p.IsLocked.ToString().ToLower());
                return View(p);
            }
            catch (Exception)
            {

                return RedirectToAction("ListUser");
            }

        }

        //Xử lý cập nhật SP
        [HttpPost]
        public ActionResult EditUser(int id, FormCollection collection)
        {

            try
            {
                // Xác định SP cần sửa trong da
                User p = da.Users.First(s => s.UserId == id);

                //Thực hiện cập nhật da
                p.IsLocked = bool.Parse(collection["StatusList"]);

                //Lưu xuống da
                da.SaveChanges();

                return RedirectToAction("ListCate");
            }
            catch//chưa xử lý bắt lỗi
            {

                return View();
            }
        }
        public ActionResult CreateCate()
        {
            if (Session["Role"] == null)
                return RedirectToAction("DangNhap", "Register");
            var Role = Session["Role"].ToString();
            if (int.Parse(Role) != 1)
                return RedirectToAction("DangNhap", "Register");
            return View();
        }
        // POST: Category/Create
        [HttpPost]
        public ActionResult CreateCate(FormCollection collection)   
        {
            try
            {
                // TODO: Add insert logic here
                Category c = new Category();
                c.Name = collection["TenLoai"];
                da.Categories.Add(c);
                da.SaveChanges();
                return RedirectToAction("ListCate");
            }
            catch
            {
                return View();
            }
        }
        public ActionResult ListCate()
        {
            if (Session["Role"] == null)
                return RedirectToAction("DangNhap", "Register");
            var Role = Session["Role"].ToString();
            if (int.Parse(Role) != 1)
                return RedirectToAction("DangNhap", "Register");
            IEnumerable<Category> ds = da.Categories.OrderByDescending(s => s.Cate_id);
            return View(ds);
        }
        
        
        public ActionResult CreateProduct()
        {
            ViewData["LSP"] = new SelectList(da.Categories, "Cate_id", "Name");
            return View();

        }
        
        public JsonResult SalesRevenue(int? day, int? month, int? year, int? quarter)
        {
            var revenues = da.OrderDetails
               .Where(od => od.Order.BookingDate.Value.Month == DateTime.Now.Month && od.Order.BookingDate.Value.Year == DateTime.Now.Year)
               .GroupBy(od => new { od.Product.Name})
               .Select(g => new
               {
                   ProductName = g.Key.Name,
                   Revenue = g.Sum(od => od.Quantity * od.UnitPrice)
               })
               .ToList();
            
            if (day != null && month != null && year != null)
            {
                // Statistics by day, month, and year
                revenues = da.OrderDetails
                    .Where(od => od.Order.BookingDate.Value.Day == day
                              && od.Order.BookingDate.Value.Month == month
                              && od.Order.BookingDate.Value.Year == year)
                    .GroupBy(od => new { od.Product.Name })
                    .Select(g => new
                    {
                        ProductName = g.Key.Name,
                        Revenue = g.Sum(od => od.Quantity * od.UnitPrice)
                    })
                    .ToList();
            }
            else if (month != null && year != null)
            {
                // Statistics by month and year
                revenues = da.OrderDetails
                    .Where(od => od.Order.BookingDate.Value.Month == month
                              && od.Order.BookingDate.Value.Year == year)
                    .GroupBy(od => new { od.Product.Name })
                    .Select(g => new
                    {
                        ProductName = g.Key.Name,
                        Revenue = g.Sum(od => od.Quantity * od.UnitPrice)
                    })
                    .ToList();
            }
            else if (quarter != null && year != null)
            {
                // Statistics by quarter and year
                var startMonth = (quarter.Value - 1) * 3 + 1;
                var endMonth = startMonth + 2;

                revenues = da.OrderDetails
                    .Where(od => od.Order.BookingDate.Value.Month >= startMonth
                              && od.Order.BookingDate.Value.Month <= endMonth
                              && od.Order.BookingDate.Value.Year == year)
                    .GroupBy(od => new { od.Product.Name })
                    .Select(g => new
                    {
                        ProductName = g.Key.Name,
                        Revenue = g.Sum(od => od.Quantity * od.UnitPrice)
                    })
                    .ToList();
            }
            else if (year != null)
            {
                // Statistics by year
                revenues = da.OrderDetails
                    .Where(od => od.Order.BookingDate.Value.Year == year)
                    .GroupBy(od => new { od.Product.Name })
                    .Select(g => new
                    {
                        ProductName = g.Key.Name,
                        Revenue = g.Sum(od => od.Quantity * od.UnitPrice)
                    })
                    .ToList();
            }

            return Json(revenues);
        }


        public JsonResult SalesQuantity(int? day, int? month, int? year, int? quarter)
        {
            // Lấy dữ liệu theo mặc định là tháng và năm hiện tại
            var query = da.OrderDetails.AsQueryable();

            // Nếu người dùng chọn theo ngày
            if (day != null && month != null && year != null)
            {
                query = query.Where(od => od.Order.BookingDate.Value.Day == day
                                          && od.Order.BookingDate.Value.Month == month
                                          && od.Order.BookingDate.Value.Year == year);
            }
            // Nếu người dùng chọn theo tháng và năm
            else if (month != null && year != null)
            {
                query = query.Where(od => od.Order.BookingDate.Value.Month == month
                                          && od.Order.BookingDate.Value.Year == year);
            }
            // Nếu người dùng chọn theo quý và năm
            else if (quarter != null && year != null)
            {
                // Xác định phạm vi tháng theo quý
                int startMonth = (quarter.Value - 1) * 3 + 1;
                int endMonth = startMonth + 2;

                query = query.Where(od => od.Order.BookingDate.Value.Month >= startMonth
                                          && od.Order.BookingDate.Value.Month <= endMonth
                                          && od.Order.BookingDate.Value.Year == year);
            }
            // Nếu người dùng chọn theo năm
            else if (year != null)
            {
                query = query.Where(od => od.Order.BookingDate.Value.Year == year);
            }
            // Mặc định lấy theo tháng và năm hiện tại nếu không chọn tiêu chí nào
            else
            {
                query = query.Where(od => od.Order.BookingDate.Value.Month == DateTime.Now.Month
                                          && od.Order.BookingDate.Value.Year == DateTime.Now.Year);
            }

            // Thực hiện nhóm và tính tổng số lượng theo sản phẩm
            var quantities = query
                .GroupBy(od => new { od.Product.Name })
                .Select(g => new
                {
                    Name = g.Key.Name,
                    Quantity = g.Sum(od => od.Quantity)
                })
                .ToList();




            // Trả về dữ liệu dưới dạng JSON
            return Json(quantities, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteComment(int id)
        {
            try
            {
                // Xác định SP cần xóa trong da
                Comment p = da.Comments.First(s => s.Comment_id == id);
                //Thực hiện xóa
                da.Comments.Remove(p);
                //Cập nhật thay đổi da
                da.SaveChanges();

                return RedirectToAction("ListComment");
            }
            catch//chưa xử lý bắt lỗi
            {

                return RedirectToAction("ListComment");
            }
        }
        public JsonResult TableQuery(int? day, int? month, int? year, int? quarter)
        {
            // Mặc định khi không có input nào, lấy tháng và năm hiện tại
            month = month ?? DateTime.Now.Month;
            year = year ?? DateTime.Now.Year;

            // Khởi tạo query
            var TableQueries = da.OrderDetails.AsQueryable();

            // Nếu người dùng chọn theo ngày
            if (day != null && month != null && year != null)
            {
                TableQueries = TableQueries.Where(od => od.Order.BookingDate.Value.Day == day
                                                        && od.Order.BookingDate.Value.Month == month
                                                        && od.Order.BookingDate.Value.Year == year);
            }
            // Nếu người dùng chọn theo tháng và năm
            else if (month != null && year != null)
            {
                TableQueries = TableQueries.Where(od => od.Order.BookingDate.Value.Month == month
                                                        && od.Order.BookingDate.Value.Year == year);
            }
            // Nếu người dùng chọn theo quý và năm
            else if (quarter != null && year != null)
            {
                int startMonth = (quarter.Value - 1) * 3 + 1;
                int endMonth = startMonth + 2;

                TableQueries = TableQueries.Where(od => od.Order.BookingDate.Value.Month >= startMonth
                                                        && od.Order.BookingDate.Value.Month <= endMonth
                                                        && od.Order.BookingDate.Value.Year == year);
            }
            // Nếu chỉ có năm, thống kê theo năm
            else if (year != null)
            {
                TableQueries = TableQueries.Where(od => od.Order.BookingDate.Value.Year == year);
            }

            // Nhóm theo tên sản phẩm, tính tổng số lượng và doanh thu
            var result = TableQueries
                .GroupBy(od => new { od.Product.Name })
                .Select(g => new
                {
                    Name = g.Key.Name,
                    Quantity = g.Sum(od => od.Quantity),
                    Revenue = g.Sum(od => od.Quantity * od.UnitPrice)
                })
                .ToList();

            // Trả về JSON chứa dữ liệu đã thống kê
            return Json(result, JsonRequestBehavior.AllowGet);
        }



        //Xử lý thêm SP
        [HttpPost]

        //Xử lý thêm SP
        public ActionResult CreateProduct(HttpPostedFileBase ImageUpload, FormCollection collection) //FormCollection collection: toàn bộ dữ liệu có trên View
        {
                Product p = new Product();
                ViewData["LSP"] = new SelectList(da.Categories, "Cate_id", "Name");
                if (ImageUpload != null)
                {
                    var imageUrl = _cloudinaryService.UploadImage(ImageUpload);
                    p.Image = imageUrl;
                }
                else
                p.Image = "loi-hinh-anh.jpg";
                //Chưa có NCC là LSP
                //Gán giá trị MaNCC, MaLSP
                p.Name = collection["TenSP"].ToString();
                p.Price = decimal.Parse(collection["GiaBan"]);
                p.Quantity = int.Parse(collection["SL"]);
                p.Cate_Id = int.Parse(collection["LSP"]);
                p.Description = collection["Description"].ToString();
                // Thêm SP vào bảng SanPham
                da.Products.Add(p);

                //Cập nhật thay đổi db
                da.SaveChanges();

                //Hiển thị DSSP
                return RedirectToAction("ListProducts");
        }

        public ActionResult DeleteProduct(int id, FormCollection collection)
        {
            try
            {
                // Xác định SP cần xóa trong da
                Product p = da.Products.First(s => s.Product_Id == id);
                //Thực hiện xóa
                da.Products.Remove(p);
                //Cập nhật thay đổi da
                da.SaveChanges();

                return RedirectToAction("ListProducts");
            }
            catch//chưa xử lý bắt lỗi
            {

                return RedirectToAction("ListProducts");
            }
        }

        public ActionResult EditProduct(int id)
        {
            if (Session["Role"] == null)
                return RedirectToAction("DangNhap", "Register");
            var Role = Session["Role"].ToString();
            if (int.Parse(Role) != 1)
                return RedirectToAction("DangNhap", "Register");
            try
            {
                ViewData["LSP"] = new SelectList(da.Categories, "Cate_id", "Name");
                Product p = da.Products.First(s => s.Product_Id == id);
                return View(p);
            }
            catch (Exception)
            {

                return RedirectToAction("ListProducts");
            }

        }

        //Xử lý cập nhật SP
        [HttpPost]
        public ActionResult EditProduct(int id, HttpPostedFileBase ImageUpload, FormCollection collection)
        {

            try
            {
                Product p = da.Products.First(s => s.Product_Id == id);
                if(collection["TenSP"]!=null)
                    p.Name = collection["TenSP"];
                if (collection["GiaBan"] != null)
                    p.Price = decimal.Parse(collection["GiaBan"]);
                if (collection["SL"] != null)
                    p.Quantity = int.Parse(collection["SL"]);
                if (collection["LSP"] != null)
                    p.Cate_Id = int.Parse(collection["LSP"]);
                p.Description = collection["Description"].ToString();
                //Cập nhật ảnh
                if (ImageUpload != null)
                { 
                    var imageUrl = _cloudinaryService.UploadImage(ImageUpload);
                    p.Image = imageUrl;
                }

                //Lưu xuống da
                da.SaveChanges();

                return RedirectToAction("ListProducts");
            }
            catch//chưa xử lý bắt lỗi
            {

                return View();
            }
        }
        public ActionResult CreateDiscountForProduct(int id)
        {
            try
            {
                Product p = da.Products.First(s => s.Product_Id == id);
                return View(p);
            }
            catch (Exception)
            {

                return RedirectToAction("ListProducts");
            }

        }
        [HttpPost]
        public ActionResult CreateDiscountForProduct(int id, FormCollection collection)
        {
            try 
            {
                Product p = da.Products.First(s => s.Product_Id == id);
                bool IsOnSale = collection["DiscountCheck"] == "on";
                p.DiscountPercentage = decimal.Parse(collection["DiscountPercentage"]);
                p.IsOnSale = bool.Parse(IsOnSale.ToString());
                da.SaveChanges();
                return RedirectToAction("ListProducts");
            }
            catch
            {
                return RedirectToAction("ListProducts");
            }

        }

        public ActionResult EditCate(int id)
        {
            try
            {
                Category p = da.Categories.First(s => s.Cate_id == id);
                return View(p);
            }
            catch (Exception)
            {

                return RedirectToAction("ListProducts");
            }

        }

        //Xử lý cập nhật SP
        [HttpPost]
        public ActionResult EditCate(int id, FormCollection collection)
        {

            try
            {
                // Xác định SP cần sửa trong da
                Category p = da.Categories.First(s => s.Cate_id == id);

                //Thực hiện cập nhật da
                if (collection["TenLoai"] != null)
                    p.Name = collection["TenLoai"];
                
                //Lưu xuống da
                da.SaveChanges();

                return RedirectToAction("ListCate");
            }
            catch//chưa xử lý bắt lỗi
            {

                return View();
            }
        }
        public ActionResult DeleteCate(int id)
        {
            try
            {
                // Xác định SP cần xóa trong da
                Category p = da.Categories.First(s => s.Cate_id == id);
                //Thực hiện xóa
                da.Categories.Remove(p);
                //Cập nhật thay đổi da
                da.SaveChanges();

                return RedirectToAction("ListCate");
            }
            catch//chưa xử lý bắt lỗi
            {

                return RedirectToAction("ListCate");
            }
        }
    }
}


