using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Mapsui.Styles;
using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.MapboxGLStyler.Converter
{
    public class PaintConverter
    {
        /// <summary>
        /// Convert given context with Mapbox GL styling layer to a Mapsui Style list
        /// </summary>
        /// <param name="context">Context to use while evaluating style</param>
        /// <param name="layer">Mapbox GL style layer</param>
        /// <param name="spriteAtlas">Dictionary with availible sprites</param>
        /// <returns>A list of Mapsui Styles</returns>
        public List<IStyle> ConvertPaint(EvaluationContext context, Layer layer, Dictionary<string, Atlas> spriteAtlas)
        {
            List<IStyle> result = new List<IStyle>();

            var paint = layer.Paint;
            var layout = layer.Layout;

            var styleLabel = new LabelStyle
            {
                Halo = new Pen { Color = Color.Transparent, Width = 0 },
                CollisionDetection = true,
                BackColor = new Brush(Color.Transparent)
            };
            var styleVector = new VectorStyle();
            var styleSymbol = new SymbolStyle();
            
            var line = new Pen
            {
                Width = 1,
                PenStrokeCap = PenStrokeCap.Butt,
            };

            if (paint?.FillColor != null)
            {
                styleVector.Fill = new Styles.Brush(ConvertStoppedColor(paint.FillColor, context.Zoom));
                styleVector.Fill.FillStyle = FillStyle.Solid;

                if (paint?.FillOutlineColor == null)
                    line.Color = styleVector.Fill.Color;
            }

            if (paint?.FillOutlineColor != null) // && paint.FillOutlineColor is string)
            {
                line.Color = ConvertStoppedColor(paint.FillOutlineColor, context.Zoom);
            }

            if (paint?.FillOpacity != null)
            {
                var opacity = paint.FillOpacity;
                styleVector.Fill.Color = ColorOpacity(styleVector.Fill.Color, opacity);
                line.Color = ColorOpacity(line.Color, opacity);
            }

            if (layout?.LineCap != null)
            {
                switch (layout.LineCap)
                {
                    case "butt":
                        line.PenStrokeCap = PenStrokeCap.Butt;
                        break;
                    case "round":
                        line.PenStrokeCap = PenStrokeCap.Round;
                        break;
                    case "square":
                        line.PenStrokeCap = PenStrokeCap.Square;
                        break;
                }
            }

            if (layout?.LineJoin != null)
            {
                switch (layout.LineJoin)
                {
                    case "bevel":
                        line.StrokeJoin = StrokeJoin.Bevel;
                        break;
                    case "round":
                        line.StrokeJoin = StrokeJoin.Round;
                        break;
                    case "mitter":
                        line.StrokeJoin = StrokeJoin.Miter;
                        break;
                    default:
                        line.StrokeJoin = StrokeJoin.Miter;
                        break;
                }
            }
            else
            {
                // Default is mitter
            }

            if (paint?.LineColor != null)
            {
                line.Color = ConvertStoppedColor(paint.LineColor, context.Zoom);
            }

            if (paint?.LineWidth != null)
            {
                line.Width = ConvertStoppedDouble(paint.LineWidth, context.Zoom);
            }

            if (paint?.LineOpacity != null)
            {
                line.Color = new Color(line.Color.R, line.Color.G, line.Color.B, (int)Math.Round(ConvertStoppedDouble(paint.LineOpacity, context.Zoom) * 255.0));
            }

            if (paint?.LineDasharray != null)
            {
                if (paint.LineDasharray is JArray jsonDashArray)
                {
                    var dashArray = new float[jsonDashArray.Count];

                    for (int i = 0; i < jsonDashArray.Count; i++)
                        dashArray[i] = jsonDashArray[i].Value<float>();

                    line.PenStyle = PenStyle.UserDefined;
                    line.DashArray = dashArray;
                }
            }

            if (paint?.TextColor != null)
            {
                styleLabel.ForeColor = ConvertStoppedColor(paint.TextColor, context.Zoom);
            }

            if (paint?.TextHaloColor != null)
            {
                styleLabel.Halo.Color = ConvertStoppedColor(paint.TextHaloColor, context.Zoom);
            }

            if (paint?.TextHaloWidth != null)
            {
                styleLabel.Halo.Width = ConvertStoppedDouble(paint.TextHaloWidth, context.Zoom);
            }

            if (paint?.TextOpacity != null)
            {
            }

            if (layout?.TextFont != null)
            {
                var fontName = string.Empty;

                foreach (var font in layout.TextFont)
                {
                    // TODO: Check for fonts
                    //if (font.exists)
                    {
                        fontName = (string) font;
                        break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(fontName))
                    styleLabel.Font.FontFamily = fontName;
            }

            if (layout?.TextSize != null)
            {
                styleLabel.Font.Size = ConvertStoppedDouble(layout.TextSize, context.Zoom);
            }

            if (layout?.TextField != null)
            {
                var text = ReplaceFields(layout.TextField.Trim(), context.Feature.Tags);

                if (layout?.TextTransform != null)
                {
                    switch (layout.TextTransform)
                    {
                        case "uppercase":
                            text = text.ToUpper();
                            break;
                        case "lowercase":
                            text = text.ToLower();
                            break;
                    }
                }

                styleLabel.Text = text;
            }

            if (layout?.IconImage != null)
            {
                var name = ReplaceFields(ConvertStoppedString(layout.IconImage, context.Zoom), context.Feature.Tags);

                if (!string.IsNullOrEmpty(name) && spriteAtlas.ContainsKey(name) && spriteAtlas[name].BitmapId >= 0)
                {
                    styleSymbol.BitmapId = spriteAtlas[name].BitmapId;
                }
            }

            if (context.Feature.GeometryType == GeometryType.Polygon)
                styleVector.Outline = line;
            else if (context.Feature.GeometryType == GeometryType.LineString)
                styleVector.Line = line;

            if (context.Feature.GeometryType == GeometryType.Point)
            {
                result.Add(styleLabel);
                if (styleSymbol.BitmapId >= 0)
                    result.Add(styleSymbol);
            }
            else
                result.Add(styleVector);

            return result;
        }

        /// <summary>
        /// Calculate the correct string for a stopped function
        /// No StoppsType needed, because strings couldn't interpolated :)
        /// </summary>
        /// <param name="si">Parameters as StoppedString containing string</param>
        /// <param name="contextZoom">Zoom factor for calculation </param>
        /// <returns>Value for this stopp respecting zoom factor and type</returns>
        public string ConvertStoppedString(StoppedString si, float? contextZoom)
        {
            if (si == null)
                return string.Empty;

            // Are there no stopps, but a single value?
            if (si.SingleVal != string.Empty)
                return si.SingleVal;

            // Are there no stopps in array
            if (si.Stops.Count == 0)
                return string.Empty;

            float zoom = contextZoom ?? 0;

            var lastZoom = float.Parse(si.Stops[0][0].ToString());
            var lastValue = si.Stops[0][1].ToString();

            if (lastZoom > zoom)
                return lastValue;

            for (int i = 1; i < si.Stops.Count; i++)
            {
                var nextZoom = float.Parse(si.Stops[i][0].ToString());
                var nextValue = si.Stops[i][1].ToString();

                if (lastZoom <= zoom && zoom <= nextZoom)
                {
                    return lastValue;
                }

                lastZoom = nextZoom;
                lastValue = nextValue;
            }

            return lastValue;
        }

        /// <summary>
        /// Calculate the correct color for a stopped function
        /// No Bezier type up to now
        /// </summary>
        /// <param name="sc">Parameters as StoppedString containing colors</param>
        /// <param name="contextZoom">Zoom factor for calculation </param>
        /// <param name="stoppsType">Type of calculation (interpolate, exponential, categorical)</param>
        /// <returns>Value for this stopp respecting zoom factor and type</returns>
        public Color ConvertStoppedColor(StoppedString sc, float? contextZoom, StoppsType stoppsType = StoppsType.Exponential)
        {
            if (sc == null)
                return Color.Black;

            // Are there no stopps, but a single value?
            if (sc.SingleVal != string.Empty)
                return ConvertColor(sc.SingleVal);

            // Are there no stopps in array
            if (sc.Stops.Count == 0)
                return Color.Black;

            float zoom = contextZoom ?? 0;

            var lastZoom = float.Parse(sc.Stops[0][0].ToString());
            var lastValue = ConvertColor(sc.Stops[0][1].ToString());

            if (lastZoom > zoom)
                return lastValue;

            for (int i = 1; i < sc.Stops.Count; i++)
            {
                var nextZoom = float.Parse(sc.Stops[i][0].ToString());
                var nextValue = ConvertColor(sc.Stops[i][1].ToString());

                if (lastZoom <= zoom && zoom <= nextZoom)
                {
                    switch (stoppsType)
                    {
                        case StoppsType.Interval:
                            return lastValue;
                        case StoppsType.Exponential:
                            var progress = zoom - lastZoom;
                            var difference = nextZoom - lastZoom;
                            if (difference < float.Epsilon)
                                return Color.Black;
                            float factor;
                            if (sc.Base - 1 < float.Epsilon)
                                factor = progress / difference;
                            else
                                factor =  (float)((Math.Pow(sc.Base, progress) - 1) / (Math.Pow(sc.Base, difference) - 1));
                            var r = (int)Math.Round((nextValue.R - lastValue.R) * factor);
                            var g = (int)Math.Round((nextValue.G - lastValue.G) * factor);
                            var b = (int)Math.Round((nextValue.B - lastValue.B) * factor);
                            var a = (int)Math.Round((nextValue.A - lastValue.A) * factor);
                            return new Color(r, g, b, a);
                        case StoppsType.Categorical:
                            // ==
                            if (nextZoom - zoom < float.Epsilon)
                                return nextValue;
                            break;
                    }
                }

                lastZoom = nextZoom;
                lastValue = nextValue;
            }

            return lastValue;
        }

        /// <summary>
        /// Calculate the correct value for a stopped function
        /// No Bezier type up to now
        /// </summary>
        /// <param name="sd">Parameters as StoppedDouble</param>
        /// <param name="contextZoom">Zoom factor for calculation </param>
        /// <param name="stoppsType">Type of calculation (interpolate, exponential, categorical)</param>
        /// <returns>Value for this stopp respecting zoom factor and type</returns>
        public float ConvertStoppedDouble(StoppedDouble sd, float? contextZoom, StoppsType stoppsType = StoppsType.Exponential)
        {
            if (sd == null)
                return 0;

            // Are there no stopps, but a single value?
            // !=
            if (sd.SingleVal - float.MinValue > float.Epsilon)
                return sd.SingleVal;

            // Are there no stopps in array
            if (sd.Stops.Count == 0)
                return 0;

            float zoom = contextZoom ?? 0;

            var lastZoom = float.Parse(sd.Stops[0][0].ToString());
            var lastValue = float.Parse(sd.Stops[0][1].ToString());

            if (lastZoom > zoom)
                return lastValue;

            for (int i = 1; i < sd.Stops.Count; i++)
            {
                var nextZoom = float.Parse(sd.Stops[i][0].ToString());
                var nextValue = float.Parse(sd.Stops[i][1].ToString());

                if (lastZoom <= zoom && zoom <= nextZoom)
                {
                    switch (stoppsType)
                    {
                        case StoppsType.Interval:
                            return lastValue;
                        case StoppsType.Exponential:
                            var progress = zoom - lastZoom;
                            var difference = nextZoom - lastZoom;
                            if (difference < float.Epsilon)
                                return 0;
                            if (sd.Base - 1.0f < float.Epsilon)
                                return (nextValue - lastValue) * progress / difference;
                            else
                                return (float)((nextValue - lastValue) * (Math.Pow(sd.Base, progress) - 1) / (Math.Pow(sd.Base, difference) - 1));
                        case StoppsType.Categorical:
                            if (nextZoom - zoom < float.Epsilon)
                                return nextValue;
                            break;
                    }
                }

                lastZoom = nextZoom;
                lastValue = nextValue;
            }

            return lastValue;
        }

        /// <summary>
        /// Converts a string in Mapbox GL format to a Mapsui Color
        /// </summary>
        /// <param name="from">String with HTML color representation or function like rgb() or hsl()</param>
        /// <returns>Converted Mapsui Color</returns>
        public Color ConvertColor(string from)
        {
            Color result = default(Color);

            from = from.Trim();

            // Check, if it is a known color
            if (knownColors.ContainsKey(from))
                from = knownColors[from];

            if (from.StartsWith("#"))
            {
                if (from.Length == 7)
                {
                    var color = int.Parse(from.Substring(1), NumberStyles.AllowHexSpecifier,
                        CultureInfo.InvariantCulture);
                    result = new Color(color >> 16 & 0xFF, color >> 8 & 0xFF, color & 0xFF);
                }
                else if (from.Length == 4)
                {
                    var color = int.Parse(from.Substring(1), NumberStyles.AllowHexSpecifier,
                        CultureInfo.InvariantCulture);
                    var r = (color >> 8 & 0xF) * 16 + (color >> 8 & 0xF);
                    var g = (color >> 4 & 0xF) * 16 + (color >> 4 & 0xF);
                    var b = (color & 0xF) * 16 + (color & 0xF);
                    result = new Color(r, g, b);
                }
            }
            else if (from.StartsWith("rgb("))
            {
                var split = from.Substring(4).TrimEnd(')').Split(',');

                if (split.Length != 3)
                    throw new ArgumentException($"color {from} isn't a valid color");

                var r = int.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
                var g = int.Parse(split[1].Trim(), CultureInfo.InvariantCulture);
                var b = int.Parse(split[2].Trim(), CultureInfo.InvariantCulture);

                result = new Color(r, g, b);
            }
            else if (from.StartsWith("rgba("))
            {
                var split = from.Substring(5).TrimEnd(')').Split(',');

                if (split.Length != 4)
                    throw new ArgumentException($"color {from} isn't a valid color");

                var r = int.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
                var g = int.Parse(split[1].Trim(), CultureInfo.InvariantCulture);
                var b = int.Parse(split[2].Trim(), CultureInfo.InvariantCulture);
                var a = float.Parse(split[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

                result = new Color(r, g, b, (int)(a * 255));
            }
            else if (from.StartsWith("hsl("))
            {
                var split = from.Substring(4).TrimEnd(')').Split(',');

                if (split.Length != 3)
                    throw new ArgumentException($"color {from} isn't a valid color");

                var h = int.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
                var s = int.Parse(split[1].Trim().Replace("%", ""), CultureInfo.InvariantCulture);
                var l = int.Parse(split[2].Trim().Replace("%", ""), CultureInfo.InvariantCulture);

                result = ColorFromHsl(h / 360.0f, s / 100.0f, l / 100.0f);
            }
            else if (from.StartsWith("hsla("))
            {
                var split = from.Substring(5).TrimEnd(')').Split(',');

                if (split.Length != 4)
                    throw new ArgumentException($"color {from} isn't a valid color");

                var h = float.Parse(split[0].Trim(), CultureInfo.InvariantCulture);
                var s = float.Parse(split[1].Trim().Replace("%", ""), CultureInfo.InvariantCulture);
                var l = float.Parse(split[2].Trim().Replace("%", ""), CultureInfo.InvariantCulture);
                var a = float.Parse(split[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

                result = ColorFromHsl(h / 360.0f, s / 100.0f, l / 100.0f, a);
            }

            return result;
        }

        /// <summary>
        /// Found at http://james-ramsden.com/convert-from-hsl-to-rgb-colour-codes-in-c/
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="l"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Color ColorFromHsl(float h, float s, float l, float a = 1)
        {
            double r = 0, g = 0, b = 0;
            // != 0
            if (l > float.Epsilon)
            {
                // == 0
                if (s < float.Epsilon)
                    r = g = b = l;
                else
                {
                    float temp2;

                    if (l < 0.5)
                        temp2 = l * (1.0f + s);
                    else
                        temp2 = l + s - (l * s);

                    float temp1 = 2.0f * l - temp2;

                    r = GetColorComponent(temp1, temp2, h + 1.0f / 3.0f);
                    g = GetColorComponent(temp1, temp2, h);
                    b = GetColorComponent(temp1, temp2, h - 1.0f / 3.0f);
                }
            }
            return Color.FromArgb((int)Math.Round(a * 255.0f), 
                (int)Math.Round(r * 255.0f), 
                (int)Math.Round(g * 255.0f), 
                (int)Math.Round(b * 255.0f));

        }

        /// <summary>
        /// Helper function for ColorFromHsl function
        /// </summary>
        /// <param name="temp1"></param>
        /// <param name="temp2"></param>
        /// <param name="temp3"></param>
        /// <returns></returns>
        private static double GetColorComponent(float temp1, float temp2, float temp3)
        {
            if (temp3 < 0.0f)
                temp3 += 1.0f;
            else if (temp3 > 1.0f)
                temp3 -= 1.0f;

            if (temp3 < 1.0f / 6.0f)
                return temp1 + (temp2 - temp1) * 6.0f * temp3;
            else if (temp3 < 0.5f)
                return temp2;
            else if (temp3 < 2.0f / 3.0f)
                return temp1 + ((temp2 - temp1) * ((2.0f / 3.0f) - temp3) * 6.0f);
            else
                return temp1;
        }

        /// <summary>
        /// Change alpha channel from given color to respect opacity
        /// </summary>
        /// <param name="color">Mapsui Color to change</param>
        /// <param name="opacity">Opacity of the new color</param>
        /// <returns>New color respecting old alpha and new opacity</returns>
        private Color ColorOpacity(Color color, float? opacity)
        {
            if (opacity == null)
                return color;

            return new Color(color.R, color.G, color.B, (int)Math.Round(color.A * (float)opacity));
        }

        Regex regExFields = new Regex(@"\{(.*?)\}", (RegexOptions)8);

        /// <summary>
        /// Replace all fields in string with values
        /// </summary>
        /// <param name="text">String with fields to replace</param>
        /// <param name="tags">Tags to replace fields with</param>
        /// <returns></returns>
        public string ReplaceFields(string text, TagsCollection tags)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var result = text;

            var match = regExFields.Match(text);

            while (match.Success)
            {
                var field = match.Groups[1].Captures[0].Value;

                // Search field
                var replacement = string.Empty;

                if (tags.ContainsKey(field))
                    replacement = tags[field].ToString();

                // Replace field with new value
                result = result.Replace(match.Groups[0].Captures[0].Value, replacement);

                // Check for next field
                match = match.NextMatch();
            };

            return result;
        }

        /// <summary>
        /// Known HTML color names and hex code for RGB color
        /// </summary>
        private readonly Dictionary<string, string> knownColors = new Dictionary<string, string>
        {
            {"AliceBlue", "#F0F8FF"},
            {"AntiqueWhite", "#FAEBD7"},
            {"Aqua", "#00FFFF"},
            {"Aquamarine", "#7FFFD4"},
            {"Azure", "#F0FFFF"},
            {"Beige", "#F5F5DC"},
            {"Bisque", "#FFE4C4"},
            {"Black", "#000000"},
            {"BlanchedAlmond", "#FFEBCD"},
            {"Blue", "#0000FF"},
            {"BlueViolet", "#8A2BE2"},
            {"Brown", "#A52A2A"},
            {"BurlyWood", "#DEB887"},
            {"CadetBlue", "#5F9EA0"},
            {"Chartreuse", "#7FFF00"},
            {"Chocolate", "#D2691E"},
            {"Coral", "#FF7F50"},
            {"CornflowerBlue", "#6495ED"},
            {"Cornsilk", "#FFF8DC"},
            {"Crimson", "#DC143C"},
            {"Cyan", "#00FFFF"},
            {"DarkBlue", "#00008B"},
            {"DarkCyan", "#008B8B"},
            {"DarkGoldenRod", "#B8860B"},
            {"DarkGray", "#A9A9A9"},
            {"DarkGrey", "#A9A9A9"},
            {"DarkGreen", "#006400"},
            {"DarkKhaki", "#BDB76B"},
            {"DarkMagenta", "#8B008B"},
            {"DarkOliveGreen", "#556B2F"},
            {"DarkOrange", "#FF8C00"},
            {"DarkOrchid", "#9932CC"},
            {"DarkRed", "#8B0000"},
            {"DarkSalmon", "#E9967A"},
            {"DarkSeaGreen", "#8FBC8F"},
            {"DarkSlateBlue", "#483D8B"},
            {"DarkSlateGray", "#2F4F4F"},
            {"DarkSlateGrey", "#2F4F4F"},
            {"DarkTurquoise", "#00CED1"},
            {"DarkViolet", "#9400D3"},
            {"DeepPink", "#FF1493"},
            {"DeepSkyBlue", "#00BFFF"},
            {"DimGray", "#696969"},
            {"DimGrey", "#696969"},
            {"DodgerBlue", "#1E90FF"},
            {"FireBrick", "#B22222"},
            {"FloralWhite", "#FFFAF0"},
            {"ForestGreen", "#228B22"},
            {"Fuchsia", "#FF00FF"},
            {"Gainsboro", "#DCDCDC"},
            {"GhostWhite", "#F8F8FF"},
            {"Gold", "#FFD700"},
            {"GoldenRod", "#DAA520"},
            {"Gray", "#808080"},
            {"Grey", "#808080"},
            {"Green", "#008000"},
            {"GreenYellow", "#ADFF2F"},
            {"HoneyDew", "#F0FFF0"},
            {"HotPink", "#FF69B4"},
            {"IndianRed ", "#CD5C5C"},
            {"Indigo ", "#4B0082"},
            {"Ivory", "#FFFFF0"},
            {"Khaki", "#F0E68C"},
            {"Lavender", "#E6E6FA"},
            {"LavenderBlush", "#FFF0F5"},
            {"LawnGreen", "#7CFC00"},
            {"LemonChiffon", "#FFFACD"},
            {"LightBlue", "#ADD8E6"},
            {"LightCoral", "#F08080"},
            {"LightCyan", "#E0FFFF"},
            {"LightGoldenRodYellow", "#FAFAD2"},
            {"LightGray", "#D3D3D3"},
            {"LightGrey", "#D3D3D3"},
            {"LightGreen", "#90EE90"},
            {"LightPink", "#FFB6C1"},
            {"LightSalmon", "#FFA07A"},
            {"LightSeaGreen", "#20B2AA"},
            {"LightSkyBlue", "#87CEFA"},
            {"LightSlateGray", "#778899"},
            {"LightSlateGrey", "#778899"},
            {"LightSteelBlue", "#B0C4DE"},
            {"LightYellow", "#FFFFE0"},
            {"Lime", "#00FF00"},
            {"LimeGreen", "#32CD32"},
            {"Linen", "#FAF0E6"},
            {"Magenta", "#FF00FF"},
            {"Maroon", "#800000"},
            {"MediumAquaMarine", "#66CDAA"},
            {"MediumBlue", "#0000CD"},
            {"MediumOrchid", "#BA55D3"},
            {"MediumPurple", "#9370DB"},
            {"MediumSeaGreen", "#3CB371"},
            {"MediumSlateBlue", "#7B68EE"},
            {"MediumSpringGreen", "#00FA9A"},
            {"MediumTurquoise", "#48D1CC"},
            {"MediumVioletRed", "#C71585"},
            {"MidnightBlue", "#191970"},
            {"MintCream", "#F5FFFA"},
            {"MistyRose", "#FFE4E1"},
            {"Moccasin", "#FFE4B5"},
            {"NavajoWhite", "#FFDEAD"},
            {"Navy", "#000080"},
            {"OldLace", "#FDF5E6"},
            {"Olive", "#808000"},
            {"OliveDrab", "#6B8E23"},
            {"Orange", "#FFA500"},
            {"OrangeRed", "#FF4500"},
            {"Orchid", "#DA70D6"},
            {"PaleGoldenRod", "#EEE8AA"},
            {"PaleGreen", "#98FB98"},
            {"PaleTurquoise", "#AFEEEE"},
            {"PaleVioletRed", "#DB7093"},
            {"PapayaWhip", "#FFEFD5"},
            {"PeachPuff", "#FFDAB9"},
            {"Peru", "#CD853F"},
            {"Pink", "#FFC0CB"},
            {"Plum", "#DDA0DD"},
            {"PowderBlue", "#B0E0E6"},
            {"Purple", "#800080"},
            {"RebeccaPurple", "#663399"},
            {"Red", "#FF0000"},
            {"RosyBrown", "#BC8F8F"},
            {"RoyalBlue", "#4169E1"},
            {"SaddleBrown", "#8B4513"},
            {"Salmon", "#FA8072"},
            {"SandyBrown", "#F4A460"},
            {"SeaGreen", "#2E8B57"},
            {"SeaShell", "#FFF5EE"},
            {"Sienna", "#A0522D"},
            {"Silver", "#C0C0C0"},
            {"SkyBlue", "#87CEEB"},
            {"SlateBlue", "#6A5ACD"},
            {"SlateGray", "#708090"},
            {"SlateGrey", "#708090"},
            {"Snow", "#FFFAFA"},
            {"SpringGreen", "#00FF7F"},
            {"SteelBlue", "#4682B4"},
            {"Tan", "#D2B48C"},
            {"Teal", "#008080"},
            {"Thistle", "#D8BFD8"},
            {"Tomato", "#FF6347"},
            {"Turquoise", "#40E0D0"},
            {"Violet", "#EE82EE"},
            {"Wheat", "#F5DEB3"},
            {"White", "#FFFFFF"},
            {"WhiteSmoke", "#F5F5F5"},
            {"Yellow", "#FFFF00"},
            {"YellowGreen", "#9ACD32"}
        };
    }
}
