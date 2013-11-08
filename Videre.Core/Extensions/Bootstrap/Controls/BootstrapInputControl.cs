using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Videre.Core.Extensions.Bootstrap.Controls
{
    public abstract class BootstrapInputControl<TControl, TModel> : BootstrapControlBase<TControl, TModel>, IFluentBootstrapInputControl<TControl, TModel>
        where TModel : IBootstrapInputControlModel, new()
        where TControl : class, IFluentBootstrapInputControl<TControl, TModel>
    {
        public BootstrapInputControl(HtmlHelper html) : base(html)
        { }
        public BootstrapInputControl(HtmlHelper html, string id) : this(html)
        {
            if (!string.IsNullOrEmpty(id))
                this._model.id = GetId(id);
        }

        public TControl Val(string val)
        {
            _model.val = val;
            return _control;
        }

        public TControl MustMatch(string controlId)
        {
            _model.mustMatch = GetId(controlId);
            return _control;
        }

        public TControl DataType(string type)
        {
            _model.dataType = type;
            return _control;
        }

        public TControl Required()
        {
            return Required(true);
        }
        public TControl Required(bool required)
        {
            _model.required = required;
            return _control;
        }

        public TControl DisableAutoComplete()
        {
            _model.disableAutoComplete = true;
            return _control;
        }

        protected void AddBaseMarkup(TagBuilder ctl)
        {
            base.AddBaseMarkup(ctl);

            //if (!string.IsNullOrEmpty(_model.text)) //cannot be in base as textarea is different!
            //    ctl.Attributes.Add("val", _model.text);

            if (!string.IsNullOrEmpty(_model.dataType))
                ctl.Attributes.Add("data-datatype", _model.dataType);
            if (!string.IsNullOrEmpty(_model.mustMatch))
                ctl.Attributes.Add("data-match", _model.mustMatch);

            if (_model.required)
                ctl.Attributes.Add("required", "required");
            if (_model.disableAutoComplete)
                ctl.Attributes.Add("autocomplete", "off");
        }


    }

}
