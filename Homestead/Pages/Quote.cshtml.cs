using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Homestead.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Homestead.Pages
{
    public class QuoteModel : PageModel
    {
        private Guid userId;
        private UsersDatabase usersDatabase;
        private AttomData aData;

        public string AddressError { get; set; }
        public string CityStateError { get; set; }
        public string ResultError { get; set; }

        [BindProperty] public string HomeAddress { get; set; }
        [BindProperty] public string CityState { get; set; }

        public QuoteModel(UsersDatabase usersDatabase, AttomData aData)
        {
            this.usersDatabase = usersDatabase;
            this.aData = aData;
        }

        /// <summary>
        /// Default get request handler
        /// </summary>
        /// <returns></returns>
        public IActionResult OnGet()
        {
            // No user id, redirect back to index
            return new RedirectToPageResult("Index");
        }

        /// <summary>
        /// User get request handler
        /// </summary>
        /// <param name="id">User id</param>
        public IActionResult OnGetUser(Guid id)
        {
            // If there is no id, redirect back to index
            if (id.Equals(Guid.Empty))
            {
                return new RedirectToPageResult("Index");
            }

            userId = id;
            return Page();
        }

        private Tuple<int, int> CalculateRent(int estimate)
        {
            // Calculate annual and monthly rate
            double annualRate = estimate * 0.05;
            double monthlyRate = annualRate / 12;

            // Give estimate as -+10%
            int low = (int)Math.Round(monthlyRate - (monthlyRate * 0.1));
            int high = (int)Math.Round(monthlyRate + (monthlyRate * 0.1));
            return new Tuple<int, int>(low, high);
        }

        public IActionResult OnPostUser(Guid id)
        {
            // Check for empty home address
            if (string.IsNullOrWhiteSpace(HomeAddress))
            {
                AddressError = "Please enter an address.";
                return Page();
            }

            // Check for empty city, state string
            if (string.IsNullOrWhiteSpace(CityState))
            {
                CityStateError = "Please enter a city and state.";
                return Page();
            }

            // Check for too long of strings
            string addr = string.Concat(HomeAddress, ",", CityState);
            if (addr.Length + 1 > 255)
            {
                AddressError = "Address is too long.";
                CityStateError = "City and state is too long.";
                return Page();
            }

            // Log lookup
            Guid reqid = usersDatabase.LogLookup(id, addr);

            // Get estimate
            int estimate = aData.GetHouseEstimate(HomeAddress, CityState);
            if (estimate <= 0)
            {
                // No estimate found
                ResultError = "No estimate available for this address.";
                return Page();
            }

            // Calculate monthly rent
            var rentRange = CalculateRent(estimate);

            // Log estimate in DB
            usersDatabase.LogEstimate(reqid, rentRange.Item1, rentRange.Item2);

            // Redirect to final result page with user id and estimate data
            return new RedirectToPageResult("Result", "Quote", new { uid = id, reqid, low = rentRange.Item1, high = rentRange.Item2 });
        }
    }
}