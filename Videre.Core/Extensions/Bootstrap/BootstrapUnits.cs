using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Videre.Core.Extensions.Bootstrap
{
    public class BootstrapUnits
    {
        public enum ButtonSize
        {
            Large,
            Small,
            ExtraSmall
        }

        public static string GetButtonSizeCss(ButtonSize size)
        {
            return ButtonSizeCss[size];
        }

        private static Dictionary<ButtonSize, string> ButtonSizeCss = new Dictionary<ButtonSize, string>()
        {
            {ButtonSize.Large, "btn-lg" },
            {ButtonSize.Small, "btn-sm" },
            {ButtonSize.ExtraSmall, "btn-xs" }
        };

        public enum ButtonStyle
        {
            Default,
            Primary,
            Success,
            Info,
            Warning,
            Danger,
            Link
        }

        public static string GetButtonStyleCss(ButtonStyle style)
        {
            return ButtonStyleCss[style];
        }

        private static Dictionary<ButtonStyle, string> ButtonStyleCss = new Dictionary<ButtonStyle, string>()
        {
            {ButtonStyle.Default, "btn-default" },
            {ButtonStyle.Primary, "btn-primary" },
            {ButtonStyle.Success, "btn-success" },
            {ButtonStyle.Info, "btn-info" },
            {ButtonStyle.Warning, "btn-warning" },
            {ButtonStyle.Danger, "btn-danger" },
            {ButtonStyle.Link, "btn-link" }
        };

        public enum InputSize
        {
            Default,
            Large,
            Small
        }

        public static string GetInputSizeCss(InputSize size)
        {
            return InputSizeCss[size];
        }

        private static Dictionary<InputSize, string> InputSizeCss = new Dictionary<InputSize, string>()
        {
            {InputSize.Default, "" },
            {InputSize.Large, "input-lg" },
            {InputSize.Small, "input-sm" },
        };


        public enum GridSize
        {
            LargeDevice1,
            LargeDevice2,
            LargeDevice3,
            LargeDevice4,
            LargeDevice5,
            LargeDevice6,
            LargeDevice7,
            LargeDevice8,
            LargeDevice9,
            LargeDevice10,
            LargeDevice11,
            LargeDevice12,
            MediumDevice1,
            MediumDevice2,
            MediumDevice3,
            MediumDevice4,
            MediumDevice5,
            MediumDevice6,
            MediumDevice7,
            MediumDevice8,
            MediumDevice9,
            MediumDevice10,
            MediumDevice11,
            MediumDevice12,
            SmallDevice1,
            SmallDevice2,
            SmallDevice3,
            SmallDevice4,
            SmallDevice5,
            SmallDevice6,
            SmallDevice7,
            SmallDevice8,
            SmallDevice9,
            SmallDevice10,
            SmallDevice11,
            SmallDevice12,
            ExtraSmallDevice1,
            ExtraSmallDevice2,
            ExtraSmallDevice3,
            ExtraSmallDevice4,
            ExtraSmallDevice5,
            ExtraSmallDevice6,
            ExtraSmallDevice7,
            ExtraSmallDevice8,
            ExtraSmallDevice9,
            ExtraSmallDevice10,
            ExtraSmallDevice11,
            ExtraSmallDevice12
        }

        public static string GetGridSizeCss(GridSize? size)
        {
            if (size.HasValue)
                return GridSizeCss[size.Value];
            return null;
        }

        private static Dictionary<GridSize, string> GridSizeCss = new Dictionary<GridSize, string>()
        {
            {GridSize.LargeDevice1, "col-lg-1" },
            {GridSize.LargeDevice2, "col-lg-2" },
            {GridSize.LargeDevice3, "col-lg-3" },
            {GridSize.LargeDevice4, "col-lg-4" },
            {GridSize.LargeDevice5, "col-lg-5" },
            {GridSize.LargeDevice6, "col-lg-6" },
            {GridSize.LargeDevice7, "col-lg-7" },
            {GridSize.LargeDevice8, "col-lg-8" },
            {GridSize.LargeDevice9, "col-lg-9" },
            {GridSize.LargeDevice10, "col-lg-10" },
            {GridSize.LargeDevice11, "col-lg-11" },
            {GridSize.LargeDevice12, "col-lg-12" },
            {GridSize.MediumDevice1, "col-md-1" },
            {GridSize.MediumDevice2, "col-md-2" },
            {GridSize.MediumDevice3, "col-md-3" },
            {GridSize.MediumDevice4, "col-md-4" },
            {GridSize.MediumDevice5, "col-md-5" },
            {GridSize.MediumDevice6, "col-md-6" },
            {GridSize.MediumDevice7, "col-md-7" },
            {GridSize.MediumDevice8, "col-md-8" },
            {GridSize.MediumDevice9, "col-md-9" },
            {GridSize.MediumDevice10, "col-md-10" },
            {GridSize.MediumDevice11, "col-md-11" },
            {GridSize.MediumDevice12, "col-md-12" },
            {GridSize.SmallDevice1, "col-sm-1" },
            {GridSize.SmallDevice2, "col-sm-2" },
            {GridSize.SmallDevice3, "col-sm-3" },
            {GridSize.SmallDevice4, "col-sm-4" },
            {GridSize.SmallDevice5, "col-sm-5" },
            {GridSize.SmallDevice6, "col-sm-6" },
            {GridSize.SmallDevice7, "col-sm-7" },
            {GridSize.SmallDevice8, "col-sm-8" },
            {GridSize.SmallDevice9, "col-sm-9" },
            {GridSize.SmallDevice10, "col-sm-10" },
            {GridSize.SmallDevice11, "col-sm-11" },
            {GridSize.SmallDevice12, "col-sm-12" },
            {GridSize.ExtraSmallDevice1, "col-xs-1" },
            {GridSize.ExtraSmallDevice2, "col-xs-2" },
            {GridSize.ExtraSmallDevice3, "col-xs-3" },
            {GridSize.ExtraSmallDevice4, "col-xs-4" },
            {GridSize.ExtraSmallDevice5, "col-xs-5" },
            {GridSize.ExtraSmallDevice6, "col-xs-6" },
            {GridSize.ExtraSmallDevice7, "col-xs-7" },
            {GridSize.ExtraSmallDevice8, "col-xs-8" },
            {GridSize.ExtraSmallDevice9, "col-xs-9" },
            {GridSize.ExtraSmallDevice10, "col-xs-10" },
            {GridSize.ExtraSmallDevice11, "col-xs-11" },
            {GridSize.ExtraSmallDevice12, "col-xs-12" }
        };


    }
}
