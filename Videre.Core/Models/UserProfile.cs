
using System.Collections.Generic;
namespace Videre.Core.Models
{
    public class UserProfile
    {
        public UserProfile()
        {
            Attributes = new Dictionary<string, object>();
        }

        public UserProfile(Models.User user)
        {
            this.Id = user.Id;
            this.Name = user.Name;
            this.Email = user.Email;
            this.Locale = user.Locale;
            this.TimeZone = user.TimeZone;
            foreach (var element in Services.Account.GetEditableProfileElements())
            {
                if (user.Attributes.ContainsKey(element.Name))
                    Attributes[element.Name] = user.Attributes[element.Name];
            }
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Locale { get; set; }
        public string TimeZone { get; set; }
        public Dictionary<string, object> Attributes {get;set;}

        public string Password1 { get; set; }
        public string Password2 { get; set; }

        public void ApplyProfileToUser(Models.User user)
        {
            //todo: mapping here or in object?
            user.Name = Name;
            user.Email = Email;
            user.Locale = Locale;
            user.TimeZone = TimeZone;
            user.Password = Password1;

            //ONLY update attributes user is able to edit
            if (Attributes != null)
            {
                foreach (var element in Services.Account.GetEditableProfileElements())
                {
                    if (Attributes.ContainsKey(element.Name))
                        user.Attributes[element.Name] = Attributes[element.Name];
                }
            }
        }

    }
}
