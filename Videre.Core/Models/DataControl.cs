
namespace Videre.Core.Models
{
    public class DataControl : Control, IClientControl
    {
        private string _path = null;

        public DataControl(string dataColumn, string path) : base(path)
        {
            DataColumn = dataColumn;
        }

        public string DataColumn { get; set; }
        public bool Required { get; set; }
        public string LabelText { get; set; }
    }
}
