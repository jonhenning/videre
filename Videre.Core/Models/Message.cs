
namespace Videre.Core.Models
{
    public class Message
    {
        public Message() { }

        public Message(string Id, string Text, bool IsError)
        {
            this.id = Id;
            this.text = Text;
            this.isError = IsError;
        }

        public string id { get; set; }
        public string text { get; set; }
        public bool isError { get; set; }
    }
}
