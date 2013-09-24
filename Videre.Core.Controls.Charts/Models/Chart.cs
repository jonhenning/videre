using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using Videre.Core.Models;

namespace Videre.Core.Models
{
    public class Chart : IClientControl
    {
        public enum ChartTheme
        {
            none,
            gray,
            grid,
            darkblue,
            darkgreen
        }
        public enum ChartType
        {
            line,
            pie,
            column
        }

        private string _path = "Core/Highcharts";

        public Chart()
        {
            xAxis = new List<string>();
            Data = new List<SeriesData>();
        }

        public Chart(string path)
            : this()
        {
            _path = path;
        }

        public string Style { get; set; }
        public string Css { get; set; }
        public string Path { get { return _path; } }
        public ChartTheme Theme { get; set; }
        public ChartType Type { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        //public string yAxisTitle { get; set; }
        public ChartOptions.yAxis yAxis { get; set; }
        public List<string> xAxis { get; set; }
        public List<SeriesData> Data { get; set; }
        public ChartOptions.Legend Legend { get; set; }
        public ChartOptions.ToolTip ToolTip { get; set; }

        public string ClientId { get; set; }    //must be assigned every time the widget is rendered

        public string ScriptPath
        {
            get
            {
                return string.Format("~/scripts/Controls/{0}/", Path);
            }
        }

        public string GetId(string id)
        {
            return string.Format("{0}_{1}", this.ClientId, id);
        }

        public string GetText(string key, string defaultValue)
        {
            return Services.Localization.GetLocalization(LocalizationType.ClientControl, key, defaultValue, this.Path);
        }

        public bool Register(HtmlHelper helper, string clientType, string instanceName, Dictionary<string, object> properties = null, bool preserveObjectReferences = false)
        {
            return false;
        }

        public string GetPortalText(string key, string efaultValue)
        {
            return Services.Localization.GetPortalText(key, efaultValue);
        }

        public class SeriesData
        {
            public SeriesData()
            {
                data = new List<decimal>();
                events = new Dictionary<string, object>();
            }
            public SeriesData(string name, dynamic data)
                : this()
            {
                //this.type = type.ToString();
                this.name = name;
                this.data = data;
            }

            public SeriesData(string name, Dictionary<string, dynamic> pieData)
                : this()
            {
                //create data like this!
                //[ ['Firefox',   45.0], ['IE',       65.0] ]
                //OR
                //{color: 'orange', name: 'Chrome', y: 12.8, sliced: true, selected: true }

                this.name = name;
                var series = new List<dynamic>();
                foreach (var key in pieData.Keys)
                    series.Add(new SeriesDataItem() { name = key, y = pieData[key] });
                //series.Add(new List<dynamic>() { key, pieData[key] });

                this.data = series;
            }

            public SeriesData(string name, List<SeriesDataItem> pieData)
                : this()
            {
                this.name = name;
                this.data = pieData;
            }

            //public string type {get;set;}
            public string name { get; set; }
            public dynamic data { get; set; }
            public Dictionary<string, object> events { get; set; }
        }

        public class SeriesDataItem
        {
            public string name { get; set; }
            public string color { get; set; }
            public dynamic y { get; set; }
            public bool? sliced { get; set; }
            public bool? selected { get; set; }
            public Dictionary<string, object> events { get; set; }
        }

    }

    public class ChartOptions
    {
        public class Legend
        {
            public Legend()
            {
                enabled = true;
            }

            public bool enabled { get; set; }
        }

        public class yAxis
        {
            public yAxis()
            {
            }
            public yAxis(string titleText)
            {
                title = new Title() { text = titleText };
            }

            public int? max { get; set; }
            public Title title { get; set; }
            public Labels labels { get; set; }
        }

        public class Title
        {
            public string text { get; set; }
        }

        public class Labels
        {
            public string format { get; set; }
        }

        public class ToolTip
        {
            public bool enabled { get; set; }
            public string format { get; set; }
        }


    }

}


