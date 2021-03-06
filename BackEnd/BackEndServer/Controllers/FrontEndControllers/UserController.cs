using System.Collections.Generic;
using BackEndServer.Models.DBModels;
using BackEndServer.Models.ViewModels;
using BackEndServer.Services;
using BackEndServer.Services.AbstractServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BackEndServer.Controllers.FrontEndControllers
{
    public class UserController : Controller
    {
        private AbstractUserService _userService;
        private AbstractUserService UserService => _userService ?? (_userService =
                                                           HttpContext.RequestServices.GetService(typeof(AbstractUserService)) as
                                                           AbstractUserService);
        
        public IActionResult BeginUserSettingsModification()
        {
            int? currentUserId = HttpContext.Session.GetInt32("currentUserId");
            if (currentUserId == null)
            {
                return RedirectToAction("SignIn", "Home");
            }

            UserSettings userSettings = UserService.GetUserSettings(currentUserId.Value);
            return View("UserSettings", userSettings);
        }
        public IActionResult PasswordChange()
        {
            int? currentUserId = HttpContext.Session.GetInt32("currentUserId");
            if (currentUserId == null)
            {
                return RedirectToAction("SignIn", "Home");
            }

            UserPassword userPassword = new UserPassword();
            userPassword.UserId = currentUserId;
            return View("PasswordChange", userPassword);
        }
        public IActionResult ForgotPassword()
        {
            return View("ForgotPassword");
        }
        public IActionResult PasswordReset([FromQuery(Name ="id")] string token)
        {
            PasswordReset passwordReset = new PasswordReset();
            passwordReset.Token = token;
            return View("PasswordReset", passwordReset);
        }

        [HttpPost]
        public JsonResult InitializePasswordReset(PasswordReset passwordReset)
        {
            return Json(UserService.ResetPassword(passwordReset));
        }

        [HttpPost]
        public JsonResult ModifyUserSettings(UserSettings userSettings)
        {
            int? currentUserId = HttpContext.Session.GetInt32("currentUserId");
            if (currentUserId != null)
            {
                return Json(UserService.ModifyUser(userSettings));
            }
            
            return Json(false);
        }
        [HttpPost]
        public JsonResult GeneratePasswordResetLink(PasswordResetLink passwordResetLink)
        {
            UserSettings userSettings = UserService.GetUserByEmailAddress(passwordResetLink.Email);
            if(userSettings == null)
            {
                PostRequestResult result = new PostRequestResult
                {
                    Success = false,
                    ErrorMessage = "A user with the specified email address could not be found"
                };

                return Json(result);
            }
            else
            {
                bool success = UserService.SendResetPasswordLink(passwordResetLink.Email);
                PostRequestResult result = new PostRequestResult
                {
                    Success = success,
                    ErrorMessage = success ? "" : "Couldn't send email because of an unexpected error"
                };

                return Json(result);
            }
        }
        
        [HttpPost]
        public JsonResult ChangePassword(UserPassword userPassword)
        {
            int? currentUserId = HttpContext.Session.GetInt32("currentUserId");
            if (currentUserId != null)
            {
                userPassword.UserId = currentUserId;
                return Json(UserService.ModifyPassword(userPassword));
            }
            
            return Json(false);
        }

        public IActionResult BeginUserCreation()
        {
            int? currentUserId = HttpContext.Session.GetInt32("currentUserId");
            if (currentUserId == null || UserService.IsUserAdministrator(currentUserId.Value) == false)
            {
                return RedirectToAction("SignIn", "Home");
            }

            UserSettings userSettings = UserService.GetUserSettings(currentUserId.Value);
            return View("UserCreation", userSettings);
        }
        
        [HttpPost]
        public IActionResult CreateUser(UserSettings userSettings)
        {
            int? currentUserId = HttpContext.Session.GetInt32("currentUserId");
            if (currentUserId == null || UserService.IsUserAdministrator(currentUserId.Value) == false)
            {
                return RedirectToAction("SignIn", "Home");
            }

            UserSettings createdUser = UserService.CreateAndReturnUser(userSettings);
            if (createdUser != null)
            {
                return View("SuccessfulCreation", createdUser);
            }
            return View("Error");
        }

        [HttpPost]
        public JsonResult ValidateUsername(string username)
        {
            int? currentUserId = HttpContext.Session.GetInt32("currentUserId");

            if (currentUserId != null)
            {
                return Json(UserService.ValidateUsername(username));
            }

            return Json(false);
        }

        [HttpPost]
        public JsonResult ValidateEmail(string emailAddress)
        {
            int? currentUserId = HttpContext.Session.GetInt32("currentUserId");

            if (currentUserId != null)
            {
                return Json(UserService.ValidateEmail(emailAddress));
            }

            return Json(false);
        }
    }
}