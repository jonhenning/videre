using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Videre.Core.Extensions.Bootstrap
{
    public interface IFluentBootstrapInputControl<TControl, TModel> : IFluentBootstrapControlBase<TControl, TModel>
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

    }
}
