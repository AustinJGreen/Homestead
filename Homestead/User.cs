using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Homestead
{
    public class User
    {
        public User(string email, string name, string phone)
        {
            Email = email;
            Name = name;
            Phone = phone;
        }

        public bool IsEmailValid()
        {
            return !string.IsNullOrWhiteSpace(Email) && Email.Length <= 255 && Regex.IsMatch(Email, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }

        public bool IsNameValid()
        {
            return !string.IsNullOrWhiteSpace(Name) && Name.Length <= 255;
        }

        public bool IsPhoneValid()
        {
            return !string.IsNullOrWhiteSpace(Phone) && Phone.Length <= 15 && Regex.IsMatch(Phone, @"^\D?(\d{3})\D?\D?(\d{3})\D?(\d{4})$");
        }

        public bool IsValid()
        {
            return IsEmailValid() && IsNameValid() && IsPhoneValid();
        }

        public string Email { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string IP { get; set; }
    }
}
