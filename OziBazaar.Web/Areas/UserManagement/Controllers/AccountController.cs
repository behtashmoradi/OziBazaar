﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using OziBazaar.Web.Models;
using System.Web.Security;
using System.Transactions;
using DotNetOpenAuth.AspNet;
using OziBazaar.DAL;
using OziBazaar.Web.Infrastructure.Repository;
using System.Data.SqlClient;
using OziBazaar.DAL.Repository;
using OziBazaar.Common.Cryptography;
using OziBazaar.Notification;
using OziBazaar.Notification.Email;
using OziBazaar.Common.Transformation;
using OziBazaar.Common.Serialization;
using OziBazaar.Notification.Entities;
using System.Text;
using OziBazaar.Common.Helper;
using OziBazaar.Web.Areas.UserManagement.Models;
using System.Configuration;
using OziBazaar.Notification.Controller;
using OziBazaar.Web.Areas.UserManagement.Converter;

namespace OziBazaar.Web.Areas.UserManagement.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        const char separator = ';';
        private readonly IEncryptionEngine _encryptionEngine;
        private readonly INotificationController _notificationController;
        private readonly IProductRepository _productRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ICacheRepository<Country> _countryRepository;
        private readonly IConverter<UserProfile, UserProfileViewModel> _userProfileViewModelConverter;

        public AccountController(
            IEncryptionEngine encryptionEngine, 
            INotificationController notificationController,
            IProductRepository productRepository,
            IAccountRepository accountRepository,
            ICacheRepository<Country> countryRepository,
            IConverter<UserProfile, UserProfileViewModel> userProfileViewModelConverter)
        {
            this._encryptionEngine = encryptionEngine;
            this._notificationController = notificationController;
            this._productRepository = productRepository;
            this._accountRepository = accountRepository;
            this._countryRepository = countryRepository;
            this._userProfileViewModelConverter = userProfileViewModelConverter;
        }
        
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(@"~\Areas\UserManagement\Views\Account\Login.cshtml");
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            UserProfile userProfile = _accountRepository.GetUser(model.UserName);
            if (userProfile != null)
            {
                if (ModelState.IsValid && WebSecurity.Login(userProfile.UserName, model.Password, persistCookie: model.RememberMe))
                {
                    if (userProfile.Activated == true)
                    {
                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Please activate your account.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The password is incorrect.");
                }
            }
            else
            {
                ModelState.AddModelError("", "The user name is incorrect.");
            }
            // If we got this far, something failed, redisplay form            
            return View(@"~\Areas\UserManagement\Views\Account\Login.cshtml",model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            RegisterModel model = new RegisterModel();
            model.CountryList = _countryRepository.GetAll().ToList<Country>();
            return View(model);
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Captcha.ToLower() != Session["captchaText"].ToString().ToLower())
                {
                    ModelState.AddModelError("CaptchText", new Exception("Invalid captcha"));
                    return View(model); 
                }
                if ( _accountRepository.GetUserByEmail(model.EmailAddress) != null)
                {
                    ModelState.AddModelError("Email address already exits", new MembershipCreateUserException(MembershipCreateStatus.DuplicateEmail));
                    return View(model);
                }
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(
                        model.UserName, 
                        model.Password,
                        new 
                        {
                            FullName = model.FullName,
                            EmailAddress = model.EmailAddress,
                            Phone = model.Phone,
                            Activated = false,
                            CountryID = model.CountryID,
                            Address1 = model.Address1,
                            Address2 = model.Address2,
                            PostCode = model.PostCode
                        },
                        false);
                    string activationCode = _encryptionEngine.DESEncrypt(
                        model.UserName + separator.ToString() + model.EmailAddress);
                    StringBuilder callBackUrl = new StringBuilder();
                    callBackUrl.Append(URLHelper.GetActivationEmailUrl());
                    callBackUrl.Append(activationCode);
                    bool result = _notificationController.SendActivationEmail(
                        new ActivationEmail()
                        {
                            Fullname = model.UserName,
                            ActivationUrl = callBackUrl.ToString()
                        },
                        model.EmailAddress
                    );
                    ViewBag.Message = "An activation email has been sent to your email address";
                    return View("Message");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
                catch(SqlException se)
                {
                     ModelState.AddModelError("", "Failed to Register new user, Try with new username/email");
                }
            }
            // If we got this far, something failed, redisplay form
            model.CountryList = _countryRepository.GetAll().ToList<Country>();
            return View(model);
        }

        //
        // Get: /Account/Activate
        
        [AllowAnonymous]
        public ActionResult Activation(string activationCode)
        {
            try
            {
                string decActivatioCode = _encryptionEngine.DESDecrypt(activationCode);
                string[] values = decActivatioCode.Split(separator);
                if (_accountRepository.ActivateUser(values[0], values[1]) == true)
                {
                    ViewBag.Message = "The account is activated sucessfuly";
                    return View("Message");
                }
                else
                    return View("Error");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // GET: /Account/UpdateUserProfile

        [Authorize]
        public ActionResult UpdateUserProfile()
        {
            UserProfileViewModel model = _userProfileViewModelConverter.ConvertTo(_accountRepository.GetUser(User.Identity.Name));
            model.CountryList = _countryRepository.GetAll().ToList<Country>();
            return View(model);
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateUserProfile(UserProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to update user profile information
                try
                {
                    return View("Message");
                }
                catch (SqlException se)
                {
                    ModelState.AddModelError("", "Failed to update user information, Please try again");
                }
            }
            // If we got this far, something failed, redisplay form
            model.CountryList = _countryRepository.GetAll().ToList<Country>();
            return View(model);
        }

        // GET: /Account/ResetPassword

        [AllowAnonymous]
        public ActionResult ResetPassword()
        {
            return View();
        }

        //
        // POST: /Account/ResetPassword

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            UserProfile userProfile = _accountRepository.GetUser(model.UserName);
            if (userProfile != null && userProfile.Activated == true)
            {
                if (ModelState.IsValid && userProfile.EmailAddress == model.EmailAddress)
                {
                    var token = WebSecurity.GeneratePasswordResetToken(model.UserName);
                    var result = WebSecurity.ResetPassword(token, model.UserName+"1234");

                    bool result1 = _notificationController.SendResetPassword(
                        new ResetPassword()
                        {
                            Fullname = userProfile.FullName,
                            NewPassword = model.UserName + "1234"
                        },
                        model.EmailAddress
                    );
                    ViewBag.Message = "A new password has been sent to your email address";
                    return View("Message");                    
                }
                else
                {
                    ModelState.AddModelError("", "The user name or email address provided is incorrect.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Please activate your account.");
            }
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (OziBazaarEntities db = new OziBazaarEntities())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}