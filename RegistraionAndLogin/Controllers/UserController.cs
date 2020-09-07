using RegistraionAndLogin.Common;
using RegistraionAndLogin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace RegistraionAndLogin.Controllers
{
    public class UserController : Controller
    {
        // GET: Registration Action
        public ActionResult Registration()
        {
            return View();
        }

        //registration post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] User user)
        {
            bool Status = false;
            string message = "";
            //Model Validation
            if(ModelState.IsValid)
            {
                #region email is exist
                var isExist = IsEmailExist(user.Email);
                if(isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email is already exist");
                    return View(user);
                }
                #endregion

                #region Generate Activation Code
                user.ActivationCode = Guid.NewGuid();
                #endregion

                #region Password Hashing
                user.Password = Crypto.Hash(user.Password);
                user.ConfirmPass = Crypto.Hash(user.ConfirmPass); //this line for DbContext validation again .. prevent issue
                #endregion
                user.IsEmailVerified = false;

                #region Save Data to Database
                using (LoginContext context = new LoginContext())
                {
                    context.Users.Add(user);
                    context.SaveChanges();

                    //Send Email to User
                    SendVerificationlinkEmail(user.Email, user.ActivationCode.ToString());
                    message = "Registration successfully done , account activation link " +
                       "has been sent to your email" + user.Email;
                    Status = true;
                }
                #endregion

            }
            else
            {
                message = "Invalid Request";
            }


            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }

        //verify Account

        public ActionResult VrifyAccount(string ActivationCode)
        {
            bool Status = false;
            using (LoginContext context = new LoginContext())
            {
                context.Configuration.ValidateOnSaveEnabled = false;  //this line added to avoid confirm pass doesn't match issue on save changes

                var user = context.Users.Where(a => a.ActivationCode == new Guid(ActivationCode)).FirstOrDefault();
                if (user != null)
                {
                    user.IsEmailVerified = true;
                    context.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
                ViewBag.status = Status;
                return View();
            }
        }

        //login 

        public ActionResult Login()
        {
            return View();
        }

        //login post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogIn userLogin, string returnUrl)
        {
            string message = "";
            using(LoginContext context = new LoginContext())
            {
                var userDb = context.Users.Where(u => u.Email == userLogin.Email).FirstOrDefault();
                if (userDb != null && userDb.IsEmailVerified == true)
                {
                    if (string.Compare(Crypto.Hash(userLogin.Password), userDb.Password) == 0) // that means provided pass is valid
                    {
                        //if user select remeber me , save it to cookie for long time
                        int timeOut = userLogin.RememberMe ? 525600 : 20; //525600 = one year
                        var ticket = new FormsAuthenticationTicket(userLogin.Email, userLogin.RememberMe, timeOut);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeOut);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                        if (Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                else
                {
                    message = "Invalid Login Credential";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        //logout

        [Authorize]
        [HttpPost]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }



        [NonAction]
        public bool IsEmailExist(string emailId)
        {
            using (LoginContext  context = new LoginContext())
            {
                var emailExist = context.Users.Where(e => e.Email == emailId).FirstOrDefault();
                return emailExist != null;
            }
        }

        [NonAction]
        public void SendVerificationlinkEmail(string email, string code, string codeType = "activationCode", string emailFor = "VerifyAccount")
        {
            var verifyUrl = "";
            if (codeType == "activationCode")
                verifyUrl = "/User/" + emailFor + "?" + codeType + "=" + code;
            else
                verifyUrl = "/User/" + emailFor + "?" + codeType + "=" + code;

            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            // ****************** change mail and pass to yours before any debug ***************************
            var fromEmail = new MailAddress("search2adie@gmail.com", "Thamin");
            var toEmail = new MailAddress(email);
            var fromPass = "Google@1745";
            string subject = "";
            string body = "";


            if (emailFor == "VerifyAccount")
            {

                subject = "Your account is successfully created";

                body = "<br/><br/> We 're excited to tell you that your account is successfully created." +
                    " please check the link below to verify your account" +
                    "<br/><br/> <a href='" + link + "'>" + link + "</a>";
            }

            if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                body = "Hi, <br/> <br/> we've got a request for reset your account password , please click on the link below to reset your password" +
                       " <br/> <br/> <a href=" + link + "> Reset Password Link </a>";
            }


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromPass)
            };

            using (var messaage = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true

            })

                smtp.Send(messaage);
        }

        //part 3 : forget pass

        public ActionResult ForgetPassword()
        {
            return View();
        }

        //check email is valid then send resetPassCode to this email 
        [HttpPost]
        public ActionResult ForgetPassword(string EmailId)
        {

            string Message = "";
            using (LoginContext context = new LoginContext())
            {
                var account = context.Users.Where(a => a.Email == EmailId).FirstOrDefault();
                if (account != null)
                {
                    //generate reset password link
                    string resetCode = Guid.NewGuid().ToString();
                    SendVerificationlinkEmail(account.Email, resetCode, "resetPassCode", "ResetPassword");
                    account.ResetPasswordCode = resetCode;

                    context.Configuration.ValidateOnSaveEnabled = false; //this line for avoiding confirm pass not match issue , becus confirm not in db and validate 
                                                                         //will throw error
                    context.SaveChanges();
                    Message = "Reset Password Link has been sent to your Email : " + EmailId;
                }
                else
                {
                    Message = "There is no aacount with this email";
                }
            }

            ViewBag.Message = Message;
            return View();
        }
        public ActionResult ResetPassword(string resetPassCode)
        {
            //verify the reset pass link 

            //find account associated with that link
            //redirect to reset password page
            using (LoginContext context = new LoginContext())
            {
                var user = context.Users.Where(u => u.ResetPasswordCode == resetPassCode).FirstOrDefault();
                if (user != null)
                {
                    ResetPasswordModel resetModel = new ResetPasswordModel();
                    resetModel.ResetCode = resetPassCode;
                    return View(resetModel);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel resetModel)
        {
            string Message = "";
            bool status = false;
            if (ModelState.IsValid)
            {
                using (LoginContext context = new LoginContext())
                {
                    var user = context.Users.Where(u => u.ResetPasswordCode == resetModel.ResetCode).FirstOrDefault();
                    if (user != null)
                    {
                        user.Password = Crypto.Hash(resetModel.NewPassword);
                        user.ResetPasswordCode = ""; //this line for no one can use the same email for multipe times to reset pass

                        context.Configuration.ValidateOnSaveEnabled = false;
                        context.SaveChanges();
                        status = true;
                        Message = "New password is updated successfully";
                    }
                }
            }
            else
            {
                Message = "Something Invalid";
            }

            ViewBag.Status = status;
            ViewBag.Message = Message;
            return View(resetModel);
        }
    }
}