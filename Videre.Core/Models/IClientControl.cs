
namespace Videre.Core.Models
{
    public interface IClientControl
    {
        string ClientId { get; }
        string GetId(string Id);
        string GetText(string Key, string DefaultValue);
        string GetPortalText(string Key, string DefaultValue);
        string Path { get; }
        string ScriptPath { get; }
        //IManifest Manifest { get; set; }
    }
}
