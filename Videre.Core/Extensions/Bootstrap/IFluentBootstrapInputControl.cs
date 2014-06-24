using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Videre.Core.Extensions.Bootstrap
{
    public interface IInputControl  //need way to determine if control is input of any type... todo: can we check for IFluentBootstrapControlBase of any type?
    {
    }

    public interface IFluentBootstrapInputControl<TControl, TModel> : IFluentBootstrapControlBase<TControl, TModel>, IInputControl
    {
        TControl Val(string text);

        TControl DataType(string type);
        TControl ControlType(string type);
        TControl MustMatch(string controlId);
        TControl Required();
        TControl Required(bool required);
        TControl ReadOnly();
        TControl ReadOnly(bool readOnly);
        TControl MaxLength(int maxLength);
        TControl InputSize(Bootstrap.BootstrapUnits.InputSize size);
        TControl Append(Bootstrap.IBootstrapBaseControl ctl);
        TControl Prepend(Bootstrap.IBootstrapBaseControl ctl);

        TControl DisableAutoComplete();
        TControl PlaceHolder(string token, string defaultText);
        TControl PlaceHolder(string token, string defaultText, bool portalText);
    }
}
