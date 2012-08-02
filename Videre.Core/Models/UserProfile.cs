
namespace Videre.Core.Models
{
    public class UserProfile
    {
        public UserProfile()
        {

        }

        public UserProfile(Models.User user)    //todo: this or extension or something else?
        {
            Id = user.Id;
            Name = user.Name;
            Email = user.Email;
            Locale = user.Locale;
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Locale { get; set; }

        public string Password1 { get; set; }
        public string Password2 { get; set; }
    }
}
