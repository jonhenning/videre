using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Videre.Core.Services
{
    public class Validation
    {
        public static bool ValidateEmail(string value, bool throwException = true)
        {
            //try
            //{
            //    var mail = new System.Net.Mail.MailAddress(value);
            //    return true;
            //}
            //catch (FormatException ex)
            //{
            //    return false;
            //}

            //http://www.regular-expressions.info/email.html
            var regex = new Regex(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
            var valid = regex.IsMatch(value);
            if (!valid && throwException)
                throw new Exception(Localization.GetExceptionText("Invalid.Error", "{0} is invalid.", "Email"));
            return valid;
        }

    }
}
