using System;
using System.Text.RegularExpressions;
using Homestead.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Homestead.Pages
{
    public class ResultModel : PageModel
    {
        private Guid uid;
        private Guid reqid;

        private UsersDatabase userDb;
        private EmailService eService;

        public string RentError { get; set; }

        public int EstimateLow { get; set; }
        public int EstimateHigh { get; set; }

        public bool SentEmail { get; set; }

        [BindProperty] public string ExpectedRent { get; set; }

        public ResultModel(UsersDatabase userDb, EmailService eService)
        {
            this.userDb = userDb;
            this.eService = eService;
        }

        private void SetRent(Guid uid, Guid reqid, int low, int high)
        {
            this.uid = uid;
            this.reqid = reqid;
            EstimateLow = low;
            EstimateHigh = high;
        }

        public IActionResult OnGet()
        {
            // No quote info, redirect to index
            return new RedirectToPageResult("Index");
        }

        public void OnGetQuote(Guid uid, Guid reqid, int low, int high)
        {
            // Set details for page display
            SetRent(uid, reqid, low, high);
        }

        public IActionResult OnPostQuote(Guid uid, Guid reqid, int low, int high)
        {
            // Set details for page display
            SetRent(uid, reqid, low, high);

            // Validate rent length
            if (string.IsNullOrWhiteSpace(ExpectedRent))
            {
                RentError = "Please enter in a dollar amount.";
                return Page();
            }

            // Validate rent as money
            decimal expectedRentValue = 0;
            if (!decimal.TryParse(ExpectedRent, System.Globalization.NumberStyles.Any, null, out expectedRentValue))
            {
                RentError = "Invalid dollar amount.";
                return Page();
            }

            // Update expected rent
            int roundedAmount = (int)Math.Round(expectedRentValue);
            userDb.LogExpectedRent(reqid, roundedAmount);

            // Send user an email
            User user = userDb.GetUser(uid);
            eService.SendMessageAsync(user, (int)Math.Round((low * 1.1 * 12) / 0.05), low, high);

            // Return to final state for now, set sent email flag to true.
            SentEmail = true;
            return Page();
        }
    }
}