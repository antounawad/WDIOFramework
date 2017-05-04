using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Eulg.Client.SupportTool
{
    #region IndicatorColor

    public struct IndicatorColor
    {
        private static readonly ColorBrightnessConverter BrightnessConverter = new ColorBrightnessConverter();

        public static readonly IndicatorColor None;
        public static readonly IndicatorColor Gray;
        public static readonly IndicatorColor Blue;
        public static readonly IndicatorColor Green;
        public static readonly IndicatorColor Purple;
        public static readonly IndicatorColor Red;
        public static readonly IndicatorColor Yellow;

        static IndicatorColor()
        {
            None = new IndicatorColor(Colors.Transparent, Colors.Transparent);
            Gray = new IndicatorColor(Colors.Gray, Colors.LightGray);
            Blue = new IndicatorColor(Colors.Blue, Colors.LightBlue);
            Green = new IndicatorColor(Colors.Green, Colors.LimeGreen);
            Purple = new IndicatorColor(Colors.Purple, Color.FromRgb(190, 0, 190));
            Red = new IndicatorColor(Colors.Red, Colors.LightCoral);
            Yellow = new IndicatorColor(Colors.Yellow, Colors.LightYellow);
        }

        private readonly Color _color;
        private readonly Color _highlight;

        private IndicatorColor(Color c1, Color c2)
        {
            _color = c1;
            _highlight = c2;
        }

        public static IndicatorColor FromValues(byte a, byte r, byte g, byte b)
        {
            return FromColor(Color.FromArgb(a, r, g, b));
        }

        public static IndicatorColor FromColor(Color color)
        {
            return new IndicatorColor(color, (Color)BrightnessConverter.Convert(color, null, 1.6, CultureInfo.InvariantCulture));
        }

        public Color Base { get { return _color; } }
        public Color Highlight { get { return _highlight; } }

        public static implicit operator Color(IndicatorColor c)
        {
            return c.Base;
        }

        public static implicit operator IndicatorColor(Color c)
        {
            return FromColor(c);
        }

        public static bool operator ==(IndicatorColor a, IndicatorColor b)
        {
            return a.Base == b.Base;
        }

        public static bool operator !=(IndicatorColor a, IndicatorColor b)
        {
            return a.Base == b.Base;
        }

        public override bool Equals(object obj)
        {
            if (obj is IndicatorColor)
            {
                return this == (IndicatorColor)obj;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Base.GetHashCode();
        }
    }

    #endregion

    public class Indicator : Control
    {
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(IndicatorColor), typeof(Indicator), new FrameworkPropertyMetadata(IndicatorColor.Gray));
        public static readonly DependencyProperty OtherColorProperty = DependencyProperty.Register("OtherColor", typeof(IndicatorColor), typeof(Indicator), new FrameworkPropertyMetadata(IndicatorColor.None));

        static Indicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Indicator), new FrameworkPropertyMetadata(typeof(Indicator)));
        }

        public IndicatorColor Color { get { return (IndicatorColor)GetValue(ColorProperty); } set { SetValue(ColorProperty, value); } }

        public IndicatorColor OtherColor { get { return (IndicatorColor)GetValue(OtherColorProperty); } set { SetValue(OtherColorProperty, value); } }
    }

    public class ColorBrightnessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return AdjustBrightnessInHsl((Color)value, System.Convert.ToDouble(parameter));
            }
            catch
            {
                return Colors.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static Color AdjustBrightnessInHsl(Color color, double factor)
        {
            double hue = 0, saturation = 0;

            var red = color.R / 255.0;
            var green = color.G / 255.0;
            var blue = color.B / 255.0;

            var max = Math.Max(color.R, Math.Max(color.G, color.B));
            var min = Math.Min(color.R, Math.Min(color.G, color.B));
            var delta = (max - min) / 255.0;

            if (max == min)
            {
                hue = 0;
            }
            else if (max == color.R && green >= blue)
            {
                hue = 60.0 * (green - blue) / delta;
                factor *= 1.04 - 0.04 * (green - blue) / delta;
            }
            else if (max == color.R && green < blue)
            {
                hue = 60.0 * (green - blue) / delta + 360.0;
            }
            else if (max == color.G)
            {
                hue = 60.0 * (blue - red) / delta + 120.0;
                factor *= 1.1 - 0.1 * (blue - red) / delta;
            }
            else if (max == color.B)
            {
                hue = 60.0 * (red - green) / delta + 240.0;
                factor *= 1.1 - 0.1 * (red - green) / delta;
            }

            var luminance = (max + min) / 512.0;

            if (min == 0 && max == 0)
            {
                saturation = 0;
            }
            else if (0 < luminance && luminance <= 0.5)
            {
                saturation = (max - min) / (double)(max + min);
            }
            else if (luminance > 0.5)
            {
                saturation = (max - min) / (double)(512 - (max + min));
            }

            luminance = Math.Min(2.0, luminance * factor);

            if (saturation <= double.Epsilon)
            {
                var b = (byte)(luminance * 255.0);
                return new Color { A = color.A, R = b, G = b, B = b };
            }

            double q = (luminance < 0.5) ? (luminance * (1.0 + saturation)) : (luminance + saturation - (luminance * saturation));
            double p = (2.0 * luminance) - q;

            var hk = hue / 360.0;
            var result = new[] { hk + (1.0 / 3.0), hk, hk - (1.0 / 3.0) };

            for (int i = 0; i < 3; i++)
            {
                if (result[i] < 0) result[i] += 1.0;
                if (result[i] > 1) result[i] -= 1.0;

                if ((result[i] * 6) < 1)
                {
                    result[i] = p + ((q - p) * 6.0 * result[i]);
                }
                else if ((result[i] * 2.0) < 1)
                {
                    result[i] = q;
                }
                else if ((result[i] * 3.0) < 2)
                {
                    result[i] = p + (q - p) * ((2.0 / 3.0) - result[i]) * 6.0;
                }
                else result[i] = p;
            }

            return new Color { A = color.A, R = (byte)(result[0] * 255), G = (byte)(result[1] * 255), B = (byte)(result[2] * 255) };
        }
    }

}
