using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.ComponentModel;

namespace Videre.Core.Binders
{
    //http://stackoverflow.com/questions/4582233/asp-net-mvc3-why-does-the-default-support-for-json-model-binding-fail-to-decod
    public class EnumConverterModelBinder : DefaultModelBinder
    {
        protected override object GetPropertyValue(ControllerContext controllerContext, ModelBindingContext bindingContext, PropertyDescriptor propertyDescriptor, IModelBinder propertyBinder)
        {
            var propertyType = propertyDescriptor.PropertyType;
            if (propertyType.IsEnum)
            {
                var providerValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                if (null != providerValue)
                {
                    var value = providerValue.RawValue;
                    if (null != value)
                    {
                        var valueType = value.GetType();
                        if (!valueType.IsEnum)
                        {
                            return Enum.Parse(propertyType, value.ToString());
                        }
                    }
                }
            }
            return base.GetPropertyValue(controllerContext, bindingContext, propertyDescriptor, propertyBinder);
        }
    }
}
