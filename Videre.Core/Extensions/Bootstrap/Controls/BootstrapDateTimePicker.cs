using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Videre.Core.Extensions;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public class BootstrapDateTimePickerModel : BootstrapBaseInputControlModel
    {
        public enum Plugin
        {
            [Description("bootstrap-datetimepicker")]
            BootstrapDateTimePicker,
            [Description("jqueryui-datepicker")]
            JqueryUIDatePicker,
            [Description("jqueryui-timepicker")]
            JqueryUITimePicker
        }

        public BootstrapDateTimePickerModel() : base()
        {
        }

        public string plugin { get; set; }
        public string timeZone { get; set; }
        public bool pickTime { get; set; }
        public bool pickDate { get; set; }
    }

    public interface IBootstrapDateTimePicker : IFluentBootstrapInputControl<IBootstrapDateTimePicker, BootstrapDateTimePickerModel>
    {
        IBootstrapDateTimePicker Plugin(BootstrapDateTimePickerModel.Plugin plugin);
        IBootstrapDateTimePicker Plugin(string plugin);
        IBootstrapDateTimePicker TimeZone(string timeZone);
        IBootstrapDateTimePicker UserTimeZone();
        IBootstrapDateTimePicker PickDate(bool pickDate);
        IBootstrapDateTimePicker PickTime(bool pickTime);
    }

    public class BootstrapDateTimePicker : BootstrapBaseInputControl<IBootstrapDateTimePicker, BootstrapDateTimePickerModel>, IBootstrapDateTimePicker
    {
        public BootstrapDateTimePicker(HtmlHelper html) : base(html) { }
        public BootstrapDateTimePicker(HtmlHelper html, string id) : base(html, id)
        {
            _model.pickDate = true;
            _model.pickTime = true;
        }

        public IBootstrapDateTimePicker Plugin(BootstrapDateTimePickerModel.Plugin plugin)
        {
            return Plugin(plugin.GetDescription());
        }
        public IBootstrapDateTimePicker Plugin(string plugin)
        {
            this._model.plugin = plugin;
            return this;
        }
        public IBootstrapDateTimePicker TimeZone(string timeZone)
        {
            this._model.timeZone = timeZone;
            return this;
        }
        public IBootstrapDateTimePicker UserTimeZone()
        {
            return TimeZone(Services.Account.GetUserTimeZoneString());
        }
        public IBootstrapDateTimePicker PickDate(bool pickDate)
        {
            this._model.pickDate = pickDate;
            return this;
        }
        public IBootstrapDateTimePicker PickTime(bool pickTime)
        {
            this._model.pickTime = pickTime;
            return this;
        }

        public override string ToHtmlString()
        {
            var ctl = new TagBuilder("input"); ;
            base.AddBaseMarkup(ctl);

            ctl.Attributes.AddSafe("type", "text");

            if (!string.IsNullOrEmpty(_model.val))
                ctl.Attributes.AddSafe("val", _model.val);  //encode?

            if (_model.pickDate)
                ctl.Attributes.AddSafe("data-pick-date", "true");
            if (_model.pickTime)
                ctl.Attributes.AddSafe("data-pick-time", "true");

            if (!string.IsNullOrEmpty(_model.timeZone))
                ctl.Attributes.AddSafe("data-timezone", _model.timeZone);

            if (!string.IsNullOrEmpty(_model.plugin))
            {
                _html.RegisterWebReferenceGroup(_model.plugin); //todo:  use plugin name as web reference group?
                ctl.Attributes.AddSafe("data-controltype", _model.plugin);
            }

            return base.Render(ctl);
        }

    }

}
