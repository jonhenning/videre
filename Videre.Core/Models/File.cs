
namespace Videre.Core.Models 
{
    public class File 
    {
        public string Id { get; set; }
        public string MimeType { get; set; }
        public long? Size { get; set; }
        public string PortalId { get; set; }  
        public string Url { get; set; }
    }
}
