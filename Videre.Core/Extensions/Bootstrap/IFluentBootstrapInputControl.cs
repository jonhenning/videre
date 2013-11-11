﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Videre.Core.Extensions.Bootstrap
{
    public interface IFluentBootstrapInputControl<TControl, TModel> : IFluentBootstrapControlBase<TControl, TModel>
    {
        TControl Val(string text);

        TControl DataType(string type);
        TControl MustMatch(string controlId);
        TControl Required();
        TControl InputSize(Bootstrap.BootstrapUnits.InputSize size);
        TControl Append(Bootstrap.IBootstrapBaseControl ctl);
        TControl Prepend(Bootstrap.IBootstrapBaseControl ctl);

        TControl Required(bool required);

        TControl DisableAutoComplete();

    }
}