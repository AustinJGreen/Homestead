using System;
using Homestead.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Homestead.Pages
{
    public class IndexModel : PageModel
    {
        private UsersDatabase usersDatabase;
        private IHttpContextAccessor context;

        public string EmailError { get; set; }
        public string NameError { get; set; }
        public string PhoneError { get; set; }

        [BindProperty] public string Email { get; set; }
        [BindProperty] public string Name { get; set; }
        [BindProperty] public string Phone { get; set; }

        public IndexModel(UsersDatabase usersDatabase, IHttpContextAccessor context)
        {
            this.usersDatabase = usersDatabase;
            this.context = context;
        }

        public IActionResult OnPost()
        {
            User newUser = new User(Email, Name, Phone);
            if (!newUser.IsEmailValid())
            {
                EmailError = "Must be a valid email under 256 characters.";
                return Page();
            }
            else if (!newUser.IsNameValid())
            {
                NameError = "Name must be between 1 and 255 characters.";
                return Page();
            }
            else if (!newUser.IsPhoneValid())
            {
                PhoneError = "Invalid phone number.";
                return Page();
            }
            else if (usersDatabase.UserExists(newUser))
            {
                EmailError = "An account with this email already exists.";
                return Page();
            }

            // Create user
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress;
            Guid uid = usersDatabase.CreateUser(newUser, ipAddress);
            return new RedirectToPageResult("Quote", "User", new { id = uid });
        }
    }
}