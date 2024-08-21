using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using NorbSoftDev.SOW;
using System.Globalization;
using System.Windows.Media.Imaging;

namespace ScenarioEditor
{
    class RankToBrushConverter : IValueConverter
    {
        public static SolidColorBrush s1Brush, a1Brush, c1Brush, d1Brush, b1Brush, r1Brush, co1Brush;
        public static SolidColorBrush s2Brush, a2Brush, c2Brush, d2Brush, b2Brush, r2Brush, co2Brush;
        public static SolidColorBrush errorBrush;
        static RankToBrushConverter()
        {
            byte i = 64;
            byte inc = 190 / 7;
            s1Brush = new SolidColorBrush(Color.FromArgb(255, i, i, 255));
            i += inc;
            a1Brush = new SolidColorBrush(Color.FromArgb(255, i, i, 255));
            i += inc;
            c1Brush = new SolidColorBrush(Color.FromArgb(255, i, i, 255));
            i += inc;
            d1Brush = new SolidColorBrush(Color.FromArgb(255, i, i, 255));
            i += inc;
            b1Brush = new SolidColorBrush(Color.FromArgb(255, i, i, 255));
            i += inc;
            r1Brush = new SolidColorBrush(Color.FromArgb(255, i, i, 255));
            i += inc;
            co1Brush = new SolidColorBrush(Color.FromArgb(255, i, i, 255));

            i = 0;
            inc = 255 / 7;
            s2Brush = new SolidColorBrush(Color.FromArgb(255, 255, i, i));
            i += inc;
            a2Brush = new SolidColorBrush(Color.FromArgb(255, 255, i, i));
            i += inc;
            c2Brush = new SolidColorBrush(Color.FromArgb(255, 255, i, i));
            i += inc;
            d2Brush = new SolidColorBrush(Color.FromArgb(255, 255, i, i));
            i += inc;
            b2Brush = new SolidColorBrush(Color.FromArgb(255, 255, i, i));
            i += inc;
            r2Brush = new SolidColorBrush(Color.FromArgb(255, 255, i, i));
            i += inc;
            co2Brush = new SolidColorBrush(Color.FromArgb(255, 255, i, i));

            errorBrush = new SolidColorBrush(Colors.Gray);
        }
        /// <summary>
        /// Converts an incoming echelon to a brush color, currently color changes on rank and 2 sides, could be modified to support more sides
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            IEchelon input = value as IEchelon;
            if (input == null)
            {
                IUnit unit = value as IUnit;
                if (unit != null)
                {
                    input = unit.echelon;
                }
            }
            if (input == null)
            {
                return errorBrush;
            }

            Brush brush = RankToBrush(input);
            if (brush == null) return DependencyProperty.UnsetValue;
            return brush;
        }

        public static Brush RankToBrush(IEchelon input) {
            //could switch based on side too
            if (input.sideIndex < 2)
            {
                switch (input.rank)
                {
                    case ERank.Side:
                        return s1Brush;
                    case ERank.Army:
                        return a1Brush;
                    case ERank.Corps:
                        return c1Brush;
                    case ERank.Division:
                        return d1Brush;
                    case ERank.Brigade:
                        return b1Brush;
                    case ERank.Regiment:
                        return r1Brush;
                    case ERank.Battalion:
                        return co1Brush;
                    default:
                        return null;
                }
            }
            switch (input.rank)
            {
                case ERank.Side:
                    return s2Brush;
                case ERank.Army:
                    return a2Brush;
                case ERank.Corps:
                    return c2Brush;
                case ERank.Division:
                    return d2Brush;
                case ERank.Brigade:
                    return b2Brush;
                case ERank.Regiment:
                    return r2Brush;
                case ERank.Battalion:
                    return co1Brush;
                default:
                    return null;
            }
        }



        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

    }

    class CommandArgToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converts an incoming echelon to a brush color, currently color changes on rank and 2 sides, could be modified to support more sides
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    class IdToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converts an incoming echelon to a brush color, currently color changes on rank and 2 sides, could be modified to support more sides
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new SolidColorBrush(Colors.AliceBlue);

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


    class EventToImageConverter : IValueConverter
    {
        /// <summary>
        /// Converts an incoming echelon to a brush color, currently color changes on rank and 2 sides, could be modified to support more sides
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// 

        static BitmapImage timeIcon, defaultIcon, continueIcon, randomIcon, unitPositionIcon;
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Type type = value.GetType();
            if (type == typeof(TimeEvent))
            {
                if (timeIcon == null) timeIcon = new BitmapImage(SOWScenarioEditorWindow.GetResourceUri("SilkIcons/icons/clock.png"));

                return timeIcon;
            }

            if (type == typeof(ContinueEvent))
            {
                if (continueIcon == null) continueIcon = new BitmapImage(SOWScenarioEditorWindow.GetResourceUri("SilkIcons/icons/link.png"));

                return continueIcon;
            }

            if (type == typeof(UnitPositionEvent))
            {
                if (unitPositionIcon == null) unitPositionIcon = new BitmapImage(SOWScenarioEditorWindow.GetResourceUri("SilkIcons/icons/arrow_turn_right.png"));

                return unitPositionIcon;
            }

            if (type == typeof(RandomEvent))
            {
                if (randomIcon == null) randomIcon = new BitmapImage(SOWScenarioEditorWindow.GetResourceUri("SilkIcons/icons/bell.png"));

                return randomIcon;
            }

            if (defaultIcon == null) defaultIcon = new BitmapImage(SOWScenarioEditorWindow.GetResourceUri("SilkIcons/icons/bullet_green.png"));

            return defaultIcon;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


    [ValueConversion(typeof(object), typeof(string))]
    public class ObjectToTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value == null ? null : value.GetType().Name;// or FullName, or whatever
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException();
        }
    }

    [ValueConversion(typeof(object), typeof(bool))]
    public class IHasCommandToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return value is IHasCommand;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

}
