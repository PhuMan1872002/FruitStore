using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using DepartMentStore.Models;
using Google.Authenticator;

namespace DepartMentStore.Controllers
{
    public class RegisterController : Controller
    {
        // GET: Register
        DepartmentStoreEntities objCon = new DepartmentStoreEntities();
        private readonly CloudinaryService _cloudinaryService = new CloudinaryService();
        public bool IsEmailExists(string eMail)
        {
            var IsCheck = objCon.Users.Where(email => email.Email == eMail).FirstOrDefault();
            return IsCheck != null;
        }
        public ActionResult UserVerification(string id)
        {
            bool Status = false;

            objCon.Configuration.ValidateOnSaveEnabled = false; // Ignor to password confirmation
            var IsVerify = objCon.Users.Where(u => u.ActiveCode == new Guid(id)).FirstOrDefault();
            //bool IsVerify=false;
            //Guid code = new Guid(id);
            if (IsVerify != null)
            {
                IsVerify.EmailVerification = true;
               
                objCon.SaveChanges();
                ViewBag.Message = "Email Verification completed";
                Status = true;
            }
            else
            {
                ViewBag.Message = "Invalid Request...Email not verify";
                ViewBag.Status = false;
            }

            return View();
        }
        public void SendEmailToUser(string emailId, string OTP)
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
            Message.Subject = "Registration Completed";
            Message.Body = "<br/> Your registration completed succesfully." +
                           "<br/> Your OTP: " + OTP;
                           
            Message.IsBodyHtml = true;
            smtp.Send(Message);

        }
        public void ForgetPasswordEmailToUser(string emailId, string activationCode,string OTP)
        {
            var GenarateUserVerificationLink = "/Register/ChangePassword/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, GenarateUserVerificationLink);

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
            Message.Subject = "Password Reset Completed-Demo";
            Message.Body = 
                           "<br/> please click on the below link for password change" +
                           "<br/><br/><a href=" + link + ">" + link + "</a>"+
                           "<br/> OTP for password change: "+OTP;
            Message.IsBodyHtml = true;
            smtp.Send(Message);

        }
        public ActionResult Index(FormCollection collection)
        {
            //try
            //{
            //var IsExists = IsEmailExists(collection["Email"]);

            //if (IsExists)
            //{
            //    ModelState.AddModelError("EmailExists", "Email already exists");
            //    return View();
            //}
            //var ActiveCode = Guid.NewGuid();
            //Session["ActiveCode"] = ActiveCode;
            //Session["Email"] = collection["Email"];
            //Session["Name"] = collection["Name"];
            //// TODO: Add insert logic here
            //var TypePassWord = collection["Password"];


            //var Password = DepartMentStore.Models.encryptPassword.textToEncrypt(TypePassWord.ToString());
            //Session["Password"] = Password;
            //SendEmailToUser(collection["Email"], ActiveCode.ToString());
            //var Message = "Registration complete,check mail " + collection["Email"];
            //ViewBag.Message = Message;

            ////send mail to user


            return View();
            //}
            //catch
            //{
            //    return View();
            //}
        }

        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Register(string email,string password,string rePassword,string name,string address, HttpPostedFileBase avatar)
        {
            var IsExists = IsEmailExists(email);

            if (IsExists)
            {
                return Json(new { success = false, message = "Email already exists" });
            }
            if(password!=rePassword)
                return Json(new { success = false, message = "Mật khẩu không khớp" });
            User user = new User();
            var OTP = GenerateOTP();
            user.OTP = OTP;
            user.Email = email;
            user.Name = name;
            // TODO: Add insert logic here
            var TypePassWord = password;
            String Password = "";
            Password = DepartMentStore.Models.encryptPassword.textToEncrypt(TypePassWord.ToString());
            user.Password = Password;
            user.Address = address;
            if (avatar != null)
            {
                var imageUrl = _cloudinaryService.UploadImage(avatar);
                user.Avartar = imageUrl;
            }
            user.EmailVerification = false;
            user.UserRoleID = 3;
            user.FailedLoginAttempts = 0;
            user.IsLocked = false;
            objCon.Users.Add(user);
            objCon.SaveChanges();
            SendEmailToUser(email, OTP.ToString());
            string returnUrl = Url.Action("OTP", "Register");
            ViewBag.Message = "Kiểm tra email và nhập OTP";

            //send mail to user


            return Json(new { success = true, returnUrl = returnUrl });

        }

        public ActionResult OTP(string id, FormCollection collection)
        {
            bool Status = false;

            objCon.Configuration.ValidateOnSaveEnabled = false; // Ignor to password confirmation     
            //bool IsVerify=false;
            //Guid code = new Guid(id);
            if (collection["OTP"] != null)
            {
                string OTP = collection["OTP"].ToString();
                var objUsr = objCon.Users.Where(x => x.OTP == OTP).FirstOrDefault();

                if (objUsr != null)
                {
                    objUsr.EmailVerification = true;
                    objCon.SaveChanges();
                    ViewBag.Message = "Đăng ký thành công";
                    return RedirectToAction("Login");
                }
                else
                    ViewBag.Message = "Sai OTP";
            }
            else
            {
                ViewBag.Message = "Check Email và nhập OTP";
                ViewBag.Status = false;
            }

            return View();
        }
        public string GenerateOTP()
        {
            string OTPLength = "4";
            string OTP = string.Empty;

            string Chars = string.Empty;
            Chars = "1,2,3,4,5,6,7,8,9,0";

            char[] seplitChar = { ',' };
            string[] arr = Chars.Split(seplitChar);
            string NewOTP = "";
            string temp = "";
            Random rand = new Random();
            for (int i = 0; i < Convert.ToInt32(OTPLength); i++)
            {
                temp = arr[rand.Next(0, arr.Length)];
                NewOTP += temp;
                OTP = NewOTP;
            }
            return OTP;
        }
       
        public ActionResult ForgetPassword()
        {
           
            return View();

        }
        [HttpPost]
        public JsonResult SendEmailForgetPassword(string email)
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

            var objUsr = objCon.Users.Where(x => x.Email == email).FirstOrDefault();

            // Generate OTP
            string OTP = GenerateOTP();

            objUsr.ActiveCode = Guid.NewGuid();
            objUsr.OTP = OTP;
            Session["OTP"] = OTP;
            objCon.Entry(objUsr).State = System.Data.Entity.EntityState.Modified;
            objCon.SaveChanges();

            ForgetPasswordEmailToUser(objUsr.Email, objUsr.ActiveCode.ToString(), objUsr.OTP);

            return Json(new { success = true, message = "Check your mail" });
        }

        public ActionResult ChangePassword(string id,FormCollection collection)
        {
            bool Status = false;

            objCon.Configuration.ValidateOnSaveEnabled = false; // Ignor to password confirmation     
            //bool IsVerify=false;
            //Guid code = new Guid(id);
            if (collection["OTP"]!=null)
            {
                string OTP = collection["OTP"].ToString();
                var objUsr = objCon.Users.Where(x => x.OTP == OTP).FirstOrDefault();
              
                if(objUsr!=null)
                {
                    
                    String Password= collection["Password"].ToString();

                    objUsr.Password = DepartMentStore.Models.encryptPassword.textToEncrypt(Password.ToString());
                    objCon.SaveChanges();
                }    

                Status = true;
            }
            else
            {
                ViewBag.Message = "Wrong OTP";
                ViewBag.Status = false;
            }

            return View();
        }
        private bool KTUser(string Email, string Password)
        {

            bool isValid = false;
            String PasswordHash = DepartMentStore.Models.encryptPassword.textToEncrypt(Password);
            User User = objCon.Users.FirstOrDefault(u => u.Email == Email);
            if (User.Password != PasswordHash)
            {
                User.FailedLoginAttempts = User.FailedLoginAttempts + 1;
                if (User.FailedLoginAttempts > 3)
                    User.IsLocked = true;
                objCon.SaveChanges();
            }    
            if (User.Password == PasswordHash && User.IsLocked ==false && User.FailedLoginAttempts<=3)
            {
                Session["Name"] = User.Name;
                Session["Role"] = User.UserRole.RoleID;
                isValid = true;

            }

            return isValid;
        }
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();

        }
        [HttpPost]
        public JsonResult Login(string email, string password,string returnUrl)
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                if (ModelState.IsValid)
                {
                    User k = new User();
                    k= objCon.Users.FirstOrDefault(n => n.Email == email);
                    if (KTUser(email, password))
                    {
                        String Password = DepartMentStore.Models.encryptPassword.textToEncrypt(password);
                        k = objCon.Users.FirstOrDefault(n => n.Email == email && n.Password == Password);

                        if (k != null)
                        {
                            FormsAuthentication.SetAuthCookie(k.Email, false);
                            var authTicket = new FormsAuthenticationTicket(
                                1,
                                k.Email, // Lưu email làm tên người dùng
                                DateTime.Now,
                                DateTime.Now.AddMinutes(30),
                                false, // Không sử dụng cookie vĩnh viễn
                                k.UserRole.RoleName // RoleName từ bảng Role
                            );

                            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                            Response.Cookies.Add(authCookie);
                            Session["User"] = k;
                            if(string.IsNullOrEmpty(returnUrl))
                            {
                                if (k.UserRoleID == 1)
                                    returnUrl = Url.Action("Index", "Admin");
                                else if(k.UserRoleID == 2)
                                    returnUrl = Url.Action("Index", "Cashier");
                                else
                                    returnUrl = Url.Action("Index", "Home");
                            }    
                            return Json(new { success = true, returnUrl=returnUrl });
                        }
                       
                    }
                    if (k.FailedLoginAttempts > 3 || k.IsLocked==true)
                        return Json(new
                        {
                            success = false,
                            message = "Tài khoản của bạn đã bị khóa"
                        }) ;
                    if (k.EmailVerification==false)
                        return Json(new
                        {
                            success = false,
                            message = "Tài khoản của bạn chưa được xác thực email"
                        });
                    return Json(new
                    {
                        success = false,
                        message = "Tên đăng nhập hoặc mật khẩu không đúng, bạn đã nhập sai " + k.FailedLoginAttempts + " lần, nhập sai 3 lần sẽ bị khóa tài khoản"
                    });
                }
            }

            return Json(new { success = false, message = "Không được để trống tên đăng nhập" });
        }


        public ActionResult DangXuat()
        {
            FormsAuthentication.SignOut();
            Session.Abandon(); // it will clear the session at the end of request
            return RedirectToAction("Index", "Home");
        }
     
    }
}
