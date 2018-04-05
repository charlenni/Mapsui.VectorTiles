using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Mapsui.Styles;
using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.MapboxGLStyler.Converter
{
    public class StyleLayerConverter
    {
        /// <summary>
        /// Convert given context with Mapbox GL styling layer to a Mapsui Style list
        /// </summary>
        /// <param name="context">Context to use while evaluating style</param>
        /// <param name="layer">Mapbox GL style layer</param>
        /// <param name="spriteAtlas">Dictionary with availible sprites</param>
        /// <returns>A list of Mapsui Styles</returns>
        public IList<IStyle> Convert(EvaluationContext context, Layer layer, Dictionary<string, Atlas> spriteAtlas)
        {
            switch (layer.Type)
            {
                case "fill":
                    return ConvertFillLayer(context, layer, spriteAtlas);
                case "line":
                    return ConvertLineLayer(context, layer, spriteAtlas);
                case "symbol":
                    return ConvertSymbolLayer(context, layer, spriteAtlas);
                case "circle":
                    return new List<IStyle>();
                case "raster":
                    return new List<IStyle>();
                case "background":
                    return new List<IStyle>();
            }

            return new List<IStyle>();
        }

        public IList<IStyle> ConvertFillLayer(EvaluationContext context, Layer layer, Dictionary<string, Atlas> spriteAtlas)
        {
            List<IStyle> result = new List<IStyle>();

            // visibility
            //   Optional enum. One of visible, none. Defaults to visible.
            //   The display of this layer. none hides this layer.
            if (layer.Layout?.Visibility != null && layer.Layout.Visibility.Equals("none"))
                return result;

            var paint = layer.Paint;

            var styleVector = new VectorStyle();
            
            var line = new Pen
            {
                Width = 1,
                PenStrokeCap = PenStrokeCap.Butt,
            };

            // fill-color
            //   Optional color. Defaults to #000000. Disabled by fill-pattern. Exponential.
            //   The color of the filled part of this layer.This color can be specified as 
            //   rgba with an alpha component and the color's opacity will not affect the 
            //   opacity of the 1px stroke, if it is used.
            if (paint?.FillColor != null)
            {
                styleVector.Fill = new Styles.Brush(ConvertStoppedColor(paint.FillColor, context.Zoom));
                styleVector.Fill.FillStyle = FillStyle.Solid;

                if (paint?.FillOutlineColor == null)
                    line.Color = styleVector.Fill.Color;
            }

            // fill-outline-color
            //   Optional color. Disabled by fill-pattern. Requires fill-antialias = true. Exponential. 
            //   The outline color of the fill. Matches the value of fill-color if unspecified.
            if (paint?.FillOutlineColor != null) // && paint.FillOutlineColor is string)
            {
                line.Color = ConvertStoppedColor(paint.FillOutlineColor, context.Zoom);
            }

            // fill-opacity
            //   Optional number. Defaults to 1. Exponential.
            //   The opacity of the entire fill layer. In contrast to the fill-color, this 
            //   value will also affect the 1px stroke around the fill, if the stroke is used.
            if (paint?.FillOpacity != null)
            {
                var opacity = paint.FillOpacity;
                styleVector.Fill.Color = ColorOpacity(styleVector.Fill.Color, opacity);
                line.Color = ColorOpacity(line.Color, opacity);
            }

            // fill-antialias
            //   Optional boolean. Defaults to true. Interval.
            //   Whether or not the fill should be antialiased.

            // fill-translate
            //   Optional array. Units in pixels. Defaults to 0,0. Exponential.
            //   The geometry's offset. Values are [x, y] where negatives indicate left and up, 
            //   respectively.

            // fill-translate-anchor
            //   Optional enum. One of map, viewport. Defaults to map. Requires fill-translate. Interval.
            //   Control whether the translation is relative to the map (north) or viewport (screen)

            // fill-pattern
            //   Optional string. Interval.
            //   Name of image in sprite to use for drawing image fills. For seamless patterns, 
            //   image width and height must be a factor of two(2, 4, 8, …, 512).

            if (context.Feature.GeometryType == GeometryType.Polygon)
                styleVector.Outline = line;
            else if (context.Feature.GeometryType == GeometryType.LineString)
                styleVector.Line = line;

            result.Add(styleVector);

            return result;
        }

        public IList<IStyle> ConvertLineLayer(EvaluationContext context, Layer layer, Dictionary<string, Atlas> spriteAtlas)
        {
            List<IStyle> result = new List<IStyle>();

            // visibility
            //   Optional enum. One of visible, none. Defaults to visible.
            //   The display of this layer. none hides this layer.
            if (layer.Layout?.Visibility != null && layer.Layout.Visibility.Equals("none"))
                return result;

            var paint = layer.Paint;
            var layout = layer.Layout;

            var styleVector = new VectorStyle();

            var line = new Pen
            {
                Width = 1,
                PenStrokeCap = PenStrokeCap.Butt,
            };

            // line-cap
            //   Optional enum. One of butt, round, square. Defaults to butt. Interval.
            //   The display of line endings.
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
                    default:
                        line.PenStrokeCap = PenStrokeCap.Butt;
                        break;
                }
            }

            // line-join
            //   Optional enum. One of bevel, round, miter. Defaults to miter. Interval.
            //   The display of lines when joining.
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

            // line-color
            //   Optional color. Defaults to #000000. Disabled by line-pattern. Exponential.
            //   The color with which the line will be drawn.
            if (paint?.LineColor != null)
            {
                line.Color = ConvertStoppedColor(paint.LineColor, context.Zoom);
            }

            // line-width
            //   Optional number.Units in pixels.Defaults to 1. Exponential.
            //   Stroke thickness.
            if (paint?.LineWidth != null)
            {
                line.Width = ConvertStoppedDouble(paint.LineWidth, context.Zoom);
            }

            // line-opacity
            //   Optional number. Defaults to 1. Exponential.
            //   The opacity at which the line will be drawn.
            if (paint?.LineOpacity != null)
            {
                line.Color = new Color(line.Color.R, line.Color.G, line.Color.B, (int)Math.Round(ConvertStoppedDouble(paint.LineOpacity, context.Zoom) * 255.0));
            }

            // line-dasharray
            //   Optional array. Units in line widths.Disabled by line-pattern. Interval.
            //   Specifies the lengths of the alternating dashes and gaps that form the dash pattern. 
            //   The lengths are later scaled by the line width.To convert a dash length to pixels, 
            //   multiply the length by the current line width.
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


            // line-miter-limit
            //   Optional number. Defaults to 2. Requires line-join = miter. Exponential.
            //   Used to automatically convert miter joins to bevel joins for sharp angles.

            // line-round-limit
            //   Optional number. Defaults to 1.05. Requires line-join = round. Exponential.
            //   Used to automatically convert round joins to miter joins for shallow angles.

            // line-translate
            //   Optional array. Units in pixels.Defaults to 0,0. Exponential.
            //   The geometry's offset. Values are [x, y] where negatives indicate left and up, 
            //   respectively.

            // line-translate-anchor
            //   Optional enum. One of map, viewport.Defaults to map. Requires line-translate. Interval.
            //   Control whether the translation is relative to the map (north) or viewport (screen)

            // line-gap-width
            //   Optional number.Units in pixels.Defaults to 0. Exponential.
            //   Draws a line casing outside of a line's actual path.Value indicates the width of 
            //   the inner gap.

            // line-offset
            //   Optional number. Units in pixels. Defaults to 0. Exponential.
            //   The line's offset perpendicular to its direction. Values may be positive or negative, 
            //   where positive indicates "rightwards" (if you were moving in the direction of the line) 
            //   and negative indicates "leftwards".

            // line-blur
            //   Optional number. Units in pixels.Defaults to 0. Exponential.
            //   Blur applied to the line, in pixels.

            // line-pattern
            //   Optional string. Interval.
            //   Name of image in sprite to use for drawing image lines. For seamless patterns, image 
            //   width must be a factor of two (2, 4, 8, …, 512).


            if (context.Feature.GeometryType == GeometryType.Polygon)
                styleVector.Outline = line;
            else if (context.Feature.GeometryType == GeometryType.LineString)
                styleVector.Line = line;

            result.Add(styleVector);

            return result;
        }

        public IList<IStyle> ConvertSymbolLayer(EvaluationContext context, Layer layer, Dictionary<string, Atlas> spriteAtlas)
        {
            List<IStyle> result = new List<IStyle>();

            // visibility
            //   Optional enum. One of visible, none. Defaults to visible.
            //   The display of this layer. none hides this layer.
            if (layer.Layout?.Visibility != null && layer.Layout.Visibility.Equals("none"))
                return result;

            var paint = layer.Paint;
            var layout = layer.Layout;

            var styleLabel = new LabelStyle
            {
                Halo = new Pen { Color = Color.Transparent, Width = 0 },
                CollisionDetection = true,
                BackColor = null
            };
            var styleVector = new VectorStyle();
            var styleSymbol = new SymbolStyle();

            var line = new Pen
            {
                Width = 1,
                PenStrokeCap = PenStrokeCap.Butt,
            };



            // symbol-placement
            //   Optional enum. One of point, line. Defaults to point. Interval.
            //   Label placement relative to its geometry. line can only be used on 
            //   LineStrings and Polygons.

            // symbol-spacing
            //   Optional number. Units in pixels. Defaults to 250. Requires symbol-placement = line. Exponential.
            //   Distance between two symbol anchors.

            // symbol-avoid-edges
            //   Optional boolean. Defaults to false. Interval.
            //   If true, the symbols will not cross tile edges to avoid mutual collisions.
            //   Recommended in layers that don't have enough padding in the vector tile to prevent 
            //   collisions, or if it is a point symbol layer placed after a line symbol layer.

            // text-field
            //   Optional string. Interval.
            //   Value to use for a text label. Feature properties are specified using tokens like {field_name}.
            if (layout?.TextField != null)
            {
                var text = ReplaceFields(layout.TextField.Trim(), context.Feature.Tags);

                // text-transform
                //   Optional enum. One of none, uppercase, lowercase. Defaults to none. Requires text-field. Interval.
                //   Specifies how to capitalize text, similar to the CSS text-transform property.
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

                // text-color
                //   Optional color. Defaults to #000000. Requires text-field. Exponential.
                //   The color with which the text will be drawn.
                if (paint?.TextColor != null)
                {
                    styleLabel.ForeColor = ConvertStoppedColor(paint.TextColor, context.Zoom);
                }

                // text-opacity
                //   Optional number. Defaults to 1. Requires text-field. Exponential.
                //   The opacity at which the text will be drawn.
                if (paint?.TextOpacity != null)
                {
                }

                // text-halo-color
                //   Optional color. Defaults to rgba(0, 0, 0, 0). Requires text-field. Exponential.
                //   The color of the text's halo, which helps it stand out from backgrounds.
                if (paint?.TextHaloColor != null)
                {
                    styleLabel.Halo.Color = ConvertStoppedColor(paint.TextHaloColor, context.Zoom);
                }

                //text-halo-width
                //   Optional number. Units in pixels. Defaults to 0. Requires text-field. Exponential.
                //   Distance of halo to the font outline. Max text halo width is 1/4 of the font-size.
                if (paint?.TextHaloWidth != null)
                {
                    styleLabel.Halo.Width = ConvertStoppedDouble(paint.TextHaloWidth, context.Zoom);
                }

                // text-font
                //   Optional array. Defaults to Open Sans Regular, Arial Unicode MS Regular. Requires text-field. Interval.
                //   Font stack to use for displaying text.
                if (layout?.TextFont != null)
                {
                    var fontName = string.Empty;

                    foreach (var font in layout.TextFont)
                    {
                        // TODO: Check for fonts
                        //if (font.exists)
                        {
                            fontName = (string)font;
                            break;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(fontName))
                        styleLabel.Font.FontFamily = fontName;
                }

                // text-size
                //   Optional number. Units in pixels. Defaults to 16. Requires text-field. Exponential.
                //   Font size.
                if (layout?.TextSize != null)
                {
                    styleLabel.Font.Size = ConvertStoppedDouble(layout.TextSize, context.Zoom);
                }

                // text-rotation-alignment
                //   Optional enum. One of map, viewport. Defaults to viewport. Requires text-field. Interval.
                //   Orientation of text when map is rotated.

                // text-translate
                //   Optional array. Units in pixels. Defaults to 0, 0. Requires text-field. Exponential.
                //   Distance that the text's anchor is moved from its original placement.Positive values 
                //   indicate right and down, while negative values indicate left and up.
                if (paint?.TextTranslate != null)
                {
                    // text-translate-anchor
                    //   Optional enum. One of map, viewport. Defaults to map. Requires text-field. Requires text-translate. Interval.
                    //   Control whether the translation is relative to the map(north) or viewport(screen).
                }

                // text-max-width
                //   Optional number. Units in em. Defaults to 10. Requires text-field. Exponential.
                //   The maximum line width for text wrapping.

                // text-line-height
                //   Optional number. Units in em. Defaults to 1.2. Requires text-field. Exponential.
                //   Text leading value for multi-line text.

                // text-letter-spacing
                //   Optional number. Units in em. Defaults to 0. Requires text-field. Exponential.
                //   Text tracking amount.

                // text-justify
                //   Optional enum. One of left, center, right. Defaults to center. Requires text-field. Interval.
                //   Text justification options.

                // text-anchor
                //   Optional enum. One of center, left, right, top, bottom, top-left, top-right, bottom-left, 
                //   bottom-right. Defaults to center. Requires text-field. Interval.
                //   Part of the text placed closest to the anchor.
                if (layout?.TextAnchor != null)
                {
                }

                // text-max-angle
                //   Optional number. Units in degrees. Defaults to 45. Requires text-field. 
                //   Requires symbol-placement = line. Exponential.
                //   Maximum angle change between adjacent characters.

                // text-rotate
                //   Optional number. Units in degrees. Defaults to 0. Requires text-field. Exponential.
                //   Rotates the text clockwise.

                // text-padding
                //   Optional number. Units in pixels. Defaults to 2. Requires text-field. Exponential.
                //   Size of the additional area around the text bounding box used for detecting symbol collisions.

                // text-keep-upright
                //   Optional boolean. Defaults to true. Requires text-field. Requires text-rotation-alignment = map.
                //   Requires symbol-placement = line. Interval.
                //   If true, the text may be flipped vertically to prevent it from being rendered upside-down.

                // text-offset
                //   Optional array. Units in ems. Defaults to 0,0. Requires text-field. Exponential.
                //   Offset distance of text from its anchor. Positive values indicate right and down, 
                //   while negative values indicate left and up.
                if (layout?.TextOffset != null)
                {
                }

                // text-allow-overlap
                //   Optional boolean. Defaults to false. Requires text-field. Interval.
                //   If true, the text will be visible even if it collides with other previously drawn symbols.

                // text-ignore-placement
                //   Optional boolean. Defaults to false. Requires text-field. Interval.
                //   If true, other symbols can be visible even if they collide with the text.

                // text-optional
                //   Optional boolean. Defaults to false. Requires text-field. Requires icon-image. Interval.
                //   If true, icons will display without their corresponding text when the text collides with other symbols and the icon does not.

                // text-halo-blur
                //   Optional number. Units in pixels. Defaults to 0. Requires text-field. Exponential.
                //   The halo's fadeout distance towards the outside.
            }

            // icon-image
            //   Optional string.
            //   A string with { tokens } replaced, referencing the data property to pull from. Interval.
            if (layout?.IconImage != null)
            {
                var name = ReplaceFields(ConvertStoppedString(layout.IconImage, context.Zoom), context.Feature.Tags);

                if (!string.IsNullOrEmpty(name) && spriteAtlas.ContainsKey(name) && spriteAtlas[name].BitmapId >= 0)
                {
                    styleSymbol.BitmapId = spriteAtlas[name].BitmapId;
                }

                // icon-allow-overlap
                //   Optional boolean. Defaults to false. Requires icon-image. Interval.
                //   If true, the icon will be visible even if it collides with other previously drawn symbols.

                // icon-ignore-placement
                //   Optional boolean. Defaults to false. Requires icon-image. Interval.
                //   If true, other symbols can be visible even if they collide with the icon.

                // icon-optional
                //   Optional boolean. Defaults to false. Requires icon-image. Requires text-field. Interval.
                //   If true, text will display without their corresponding icons when the icon collides 
                //   with other symbols and the text does not.

                // icon-rotation-alignment
                //   Optional enum. One of map, viewport. Defaults to viewport. Requires icon-image. Interval.
                //   Orientation of icon when map is rotated.

                // icon-size
                //   Optional number. Defaults to 1. Requires icon-image. Exponential.
                //   Scale factor for icon. 1 is original size, 3 triples the size.

                // icon-rotate
                //   Optional number.Units in degrees.Defaults to 0. Requires icon-image. Exponential.
                //   Rotates the icon clockwise.

                // icon-padding
                //   Optional number. Units in pixels. Defaults to 2. Requires icon-image. Exponential.
                //   Size of the additional area around the icon bounding box used for detecting symbol collisions.

                // icon-keep-upright
                //   Optional boolean. Defaults to false. Requires icon-image. Requires icon-rotation-alignment = map. Interval.
                //   Requires symbol-placement = line.
                //   If true, the icon may be flipped to prevent it from being rendered upside-down.

                // icon-offset
                //   Optional array. Defaults to 0,0. Requires icon-image. Exponential.
                //   Offset distance of icon from its anchor. Positive values indicate right and down, 
                //   while negative values indicate left and up.

                // icon-opacity
                //   Optional number. Defaults to 1. Requires icon-image. Exponential.
                //   The opacity at which the icon will be drawn.

                // icon-color
                //   Optional color. Defaults to #000000. Requires icon-image. Exponential.
                //   The color of the icon. This can only be used with sdf icons.

                // icon-halo-color
                //   Optional color. Defaults to rgba(0, 0, 0, 0). Requires icon-image. Exponential.
                //   The color of the icon's halo. Icon halos can only be used with sdf icons.

                // icon-halo-width
                //   Optional number. Units in pixels. Defaults to 0. Requires icon-image. Exponential.
                //   Distance of halo to the icon outline.

                // icon-halo-blur
                //   Optional number. Units in pixels. Defaults to 0. Requires icon-image. Exponential.
                //   Fade out the halo towards the outside.

                // icon-translate
                //   Optional array. Units in pixels. Defaults to 0, 0. Requires icon-image. Exponential.
                //   Distance that the icon's anchor is moved from its original placement.
                //   Positive values indicate right and down, while negative values indicate left and up.

                // icon-translate-anchor
                //   Optional enum. One of map, viewport. Defaults to map. Requires icon-image. Requires icon-translate. Interval.
                //   Control whether the translation is relative to the map(north) or viewport(screen).


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
            var lastValue = sc.Stops[0][1].ToString();

            if (lastZoom > zoom)
                return ConvertColor(lastValue);

            for (int i = 1; i < sc.Stops.Count; i++)
            {
                var nextZoom = float.Parse(sc.Stops[i][0].ToString());
                var nextValue = sc.Stops[i][1].ToString();

                if (lastZoom <= zoom && zoom <= nextZoom)
                {
                    switch (stoppsType)
                    {
                        case StoppsType.Interval:
                            return ConvertColor(lastValue);
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
                            var nextColor = ConvertColor(nextValue);
                            var lastColor = ConvertColor(lastValue);
                            var r = (int)Math.Round((nextColor.R - lastColor.R) * factor);
                            var g = (int)Math.Round((nextColor.G - lastColor.G) * factor);
                            var b = (int)Math.Round((nextColor.B - lastColor.B) * factor);
                            var a = (int)Math.Round((nextColor.A - lastColor.A) * factor);
                            return new Color(r, g, b, a);
                        case StoppsType.Categorical:
                            // ==
                            if (nextZoom - zoom < float.Epsilon)
                                return ConvertColor(nextValue);
                            break;
                    }
                }

                lastZoom = nextZoom;
                lastValue = nextValue;
            }

            return ConvertColor(lastValue);
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

        private readonly Dictionary<string, Color> colorCache = new Dictionary<string, Color>();

        /// <summary>
        /// Converts a string in Mapbox GL format to a Mapsui Color
        /// </summary>
        /// <param name="from">String with HTML color representation or function like rgb() or hsl()</param>
        /// <returns>Converted Mapsui Color</returns>
        public Color ConvertColor(string from)
        {
            if (colorCache.TryGetValue(from, out var result))
                return result;

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

            // Save for potential later use
            colorCache[from] = result;

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
