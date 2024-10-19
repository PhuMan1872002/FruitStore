using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using DepartMentStore.Models;
using PagedList;

namespace DepartMentStore.Controllers
{
    public class HomeController : Controller
    {
        DepartmentStoreEntities da = new DepartmentStoreEntities();
        public ActionResult Index(int? page, string searchString,int ?id)
        {

            int pageSize = 6;
            int pageNum = (page ?? 1);
            IEnumerable<Product> ds;

            if (!String.IsNullOrEmpty(searchString)) // kiểm tra chuỗi tìm kiếm có rỗng/null hay không
            {

                ds = da.Products.OrderByDescending(s => s.Product_Id).Where(s => s.Name.Contains(searchString) || s.Category.Name.Contains(searchString)).Take(15).ToList();
            }
            else if(id!=null)
                ds=da.Products.Where(s => s.Cate_Id == id).OrderByDescending(s => s.IsOnSale).ThenByDescending(s => s.Product_Id).ToList();
            else
                ds = da.Products.OrderByDescending(s => s.IsOnSale).ThenByDescending(s => s.Product_Id).Take(15).ToList();

            return View(ds.ToPagedList(pageNum, pageSize));

        }
        public ActionResult ProductDetail(int id)
        {

            Product p = new Product();
            p = da.Products.FirstOrDefault(s => s.Product_Id == id);
            p.Comments = da.Comments.Where(s => s.ProId == id).ToList();
            return View(p);
        }
        public ActionResult Coupon()
        {
            IEnumerable<Coupon> c = da.Coupons.OrderByDescending(i => i.Id).ToList();
            return View(c);
        }
        [Authorize]
        public ActionResult UserInfo()
        {
            User u = (User)Session["User"];
            return View(u);
        }
        [Authorize]
        public ActionResult Order()
        {
            User u = (User)Session["User"];
            IEnumerable<Order> c = da.Orders.Where(i=>i.User.UserId==u.UserId).OrderByDescending(i => i.Order_id).ToList();
            return View(c);
        }
        [Authorize]
        public ActionResult OrderDetail(int id)
        {
            IEnumerable<OrderDetail> c = da.OrderDetails.Where(i => i.Order_id==id).ToList();
            return View(c);
        }

        public ActionResult Shop(int? page, string searchString)
        {
            int pageSize = 5;
            int pageNum = (page ?? 1);
            IEnumerable<Product> ds;

            if (!String.IsNullOrEmpty(searchString)) // kiểm tra chuỗi tìm kiếm có rỗng/null hay không
            {

                ds = da.Products.OrderByDescending(s => s.Product_Id).Where(s => s.Name.Contains(searchString) || s.Category.Name.Contains(searchString)).Take(15).ToList();
            }
            else
                ds = da.Products.OrderByDescending(s => s.Product_Id).Take(15).ToList();

            return View(ds.ToPagedList(pageNum, pageSize));
        }
        public bool IsEmailExists(string eMail)
        {
            var IsCheck = da.Users.Where(email => email.Email == eMail).FirstOrDefault();
            return IsCheck != null;
        }
        public ActionResult Contact()
        {
            return View();
        }
        [HttpPost]
        public JsonResult SendEmail(string email,string subject, string message)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Nhập email vào" });
            }

            var isExists = IsEmailExists(email);
            if (!isExists)
            {
                return Json(new { success = false, message = "Không tìm thấy Email" });
            }

            var objUsr = da.Users.Where(x => x.Email == email).FirstOrDefault();
            SendEmailToUser(objUsr.Email,subject,message);

            return Json(new { success = true, message = "Check your mail" });
        }
        public void SendEmailToUser(string emailId, string subject, string body)
        {
            var fromMail = new MailAddress("2151010220man@ou.edu.vn", "Store Fruitkha"); // set your email    
            var fromEmailpassword = "mottambayhailehai1872002"; // Set your password     
            var toEmail = new MailAddress("2151010220man@ou.edu.vn");

            var smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = new NetworkCredential(fromMail.Address, fromEmailpassword);

            var Message = new MailMessage(fromMail, toEmail);
            Message.Subject =subject;
            Message.Body = "<br/> từ mail " + emailId + "</br>" + body;
            Message.IsBodyHtml = true;

            smtp.Send(Message);

        }
        [Authorize]
        public ActionResult Comment()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public JsonResult Comment(int productId, string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                User u = Session["User"] as User;
                var newComment = new Comment
                {
                    Content = content,
                    ProId = productId,
                    UId = u.UserId ,
                    DateCreate = DateTime.Now
                };

                da.Comments.Add(newComment);
                da.SaveChanges();

                // Return the newly added comment details
                return Json(new
                {
                    success = true,
                    content = newComment.Content,
                    dateCreate = newComment.DateCreate.ToString(),
                    user = new
                    {
                        name = u.Name,
                        avatar = u.Avartar
                    }
                });
            }

            return Json(new { success = false });

        }
    }
}