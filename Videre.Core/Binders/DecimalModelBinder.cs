using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Videre.Core.Binders
{
    //http://stackoverflow.com/questions/5500150/mvc3-model-binding-causes-the-parameter-conversion-from-type-system-int32-to
    public class DecimalModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            decimal value;
            return valueProviderResult == null || !Decimal.TryParse(valueProviderResult.AttemptedValue, out value) ? base.BindModel(controllerContext, bindingContext) : value;
        }
    }
}
