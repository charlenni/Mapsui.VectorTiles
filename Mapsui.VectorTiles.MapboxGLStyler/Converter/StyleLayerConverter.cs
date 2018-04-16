using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Mapsui.Styles;
using Mapsui.VectorTiles.MapboxGLStyler.Json;
using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.MapboxGLStyler.Converter
{
    public class StyleLayerConverter
    {
        /// <summary>
        /// Convert given context with Mapbox GL styling layer to a Mapsui Style list
        /// </summary>
        /// <param name="context">Context to use while evaluating style</param>
        /// <param name="styleLayer">Mapbox GL style layer</param>
        /// <param name="spriteAtlas">Dictionary with availible sprites</param>
        /// <returns>A list of Mapsui Styles</returns>
        public IList<IStyle> Convert(EvaluationContext context, StyleLayer styleLayer, Dictionary<string, Styles.Sprite> spriteAtlas)
        {
            switch (styleLayer.Type)
            {
                case "fill":
                    return ConvertFillLayer(context, styleLayer, spriteAtlas);
                case "line":
                    return ConvertLineLayer(context, styleLayer, spriteAtlas);
                case "symbol":
                    return ConvertSymbolLayer(context, styleLayer, spriteAtlas);
                case "circle":
                    return new List<IStyle>();
                case "raster":
                    // Shouldn't get here, because raster are directly handled by ConvertRasterLayer
                    break;
                case "background":
                    return new List<IStyle>();
            }

            return new List<IStyle>();
        }

        public IStyle ConvertRasterLayer(float contextResolution, StyleLayer styleLayer)
        {
            // visibility
            //   Optional enum. One of visible, none. Defaults to visible.
            //   The display of this layer. none hides this layer.
            if (styleLayer.Layout?.Visibility != null && styleLayer.Layout.Visibility.Equals("none"))
                return null;

            var paint = styleLayer.Paint;

            var styleRaster = new RasterStyle();

            // raster-opacity
            //   Optional number. Defaults to 1.
            //   The opacity at which the image will be drawn.
            if (paint?.RasterOpacity != null)
            {
                styleRaster.Opacity = paint.RasterOpacity.Evaluate(contextResolution);
            }

            // raster-hue-rotate
            //   Optional number. Units in degrees. Defaults to 0.
            //   Rotates hues around the color wheel.

            // raster-brightness-min
            //   Optional number.Defaults to 0.
            //   Increase or reduce the brightness of the image. The value is the minimum brightness.

            // raster-brightness-max
            //   Optional number. Defaults to 1.
            //   Increase or reduce the brightness of the image. The value is the maximum brightness.

            // raster-saturation
            //   Optional number.Defaults to 0.
            //   Increase or reduce the saturation of the image.

            // raster-contrast
            //   Optional number. Defaults to 0.
            //   Increase or reduce the contrast of the image.

            // raster-fade-duration
            //   Optional number.Units in milliseconds.Defaults to 300.
            //   Fade duration when a new tile is added.

            return styleRaster;
        }

        public IList<IStyle> ConvertFillLayer(EvaluationContext context, StyleLayer styleLayer, Dictionary<string, Styles.Sprite> spriteAtlas)
        {
            // Height of building isn't used (that's what Point contains in tags here)
            if (context.Feature.GeometryType == GeometryType.Point)
                return null;

            List<IStyle> result = new List<IStyle>();

            // visibility
            //   Optional enum. One of visible, none. Defaults to visible.
            //   The display of this layer. none hides this layer.
            if (styleLayer.Layout?.Visibility != null && styleLayer.Layout.Visibility.Equals("none"))
                return result;

            var paint = styleLayer.Paint;

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
                styleVector.Fill = new Styles.Brush(paint.FillColor.Evaluate(context.Resolution))
                {
                    FillStyle = FillStyle.Solid
                };

                if (paint?.FillOutlineColor == null)
                    line.Color = styleVector.Fill.Color;
            }

            // fill-outline-color
            //   Optional color. Disabled by fill-pattern. Requires fill-antialias = true. Exponential. 
            //   The outline color of the fill. Matches the value of fill-color if unspecified.
            if (paint?.FillOutlineColor != null) // && paint.FillOutlineColor is string)
            {
                line.Color = paint.FillOutlineColor.Evaluate(context.Resolution);
            }

            // fill-opacity
            //   Optional number. Defaults to 1. Exponential.
            //   The opacity of the entire fill layer. In contrast to the fill-color, this 
            //   value will also affect the 1px stroke around the fill, if the stroke is used.
            if (paint?.FillOpacity != null)
            {
                var opacity = paint.FillOpacity;
                styleVector.Fill.Color = Color.Opacity(styleVector.Fill.Color, opacity);
                line.Color = Color.Opacity(line.Color, opacity);
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

            styleVector.Enabled = true;

            result.Add(styleVector);

            return result;
        }

        public IList<IStyle> ConvertLineLayer(EvaluationContext context, StyleLayer styleLayer, Dictionary<string, Styles.Sprite> spriteAtlas)
        {
            List<IStyle> result = new List<IStyle>();

            // visibility
            //   Optional enum. One of visible, none. Defaults to visible.
            //   The display of this layer. none hides this layer.
            if (styleLayer.Layout?.Visibility != null && styleLayer.Layout.Visibility.Equals("none"))
                return result;

            var paint = styleLayer.Paint;
            var layout = styleLayer.Layout;

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
                line.Color = paint.LineColor.Evaluate(context.Resolution);
            }

            // line-width
            //   Optional number.Units in pixels.Defaults to 1. Exponential.
            //   Stroke thickness.
            if (paint?.LineWidth != null)
            {
                line.Width = paint.LineWidth.Evaluate(context.Resolution);
            }

            // line-opacity
            //   Optional number. Defaults to 1. Exponential.
            //   The opacity at which the line will be drawn.
            if (paint?.LineOpacity != null)
            {
                line.Color = new Color(line.Color.R, line.Color.G, line.Color.B, (int)Math.Round(paint.LineOpacity.Evaluate(context.Resolution) * 255.0));
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

            styleVector.Enabled = true;

            result.Add(styleVector);

            return result;
        }

        public IList<IStyle> ConvertSymbolLayer(EvaluationContext context, StyleLayer styleLayer, Dictionary<string, Styles.Sprite> spriteAtlas)
        {
            string styleLabelText = string.Empty;
            List<IStyle> result = new List<IStyle>();



            //return result;


            // visibility
            //   Optional enum. One of visible, none. Defaults to visible.
            //   The display of this layer. none hides this layer.
            if (styleLayer.Layout?.Visibility != null && styleLayer.Layout.Visibility.Equals("none"))
                return result;

            var paint = styleLayer.Paint;
            var layout = styleLayer.Layout;

            var styleLabel = new LabelStyle
            {
                Enabled = false,
                Halo = new Pen { Color = Color.Transparent, Width = 0 },
                CollisionDetection = true,
                BackColor = null,
            };

            styleLabel.Font.Size = 16;

            var styleVector = new VectorStyle();

            var styleSymbol = new SymbolStyle
            {
                Enabled = false,
            };

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
                styleLabelText = ReplaceFields(layout.TextField.Trim(), context.Feature.Tags);

                // text-transform
                //   Optional enum. One of none, uppercase, lowercase. Defaults to none. Requires text-field. Interval.
                //   Specifies how to capitalize text, similar to the CSS text-transform property.
                if (layout?.TextTransform != null)
                {
                    switch (layout.TextTransform)
                    {
                        case "uppercase":
                            styleLabelText = styleLabelText.ToUpper();
                            break;
                        case "lowercase":
                            styleLabelText = styleLabelText.ToLower();
                            break;
                    }
                }

                styleLabel.Text = styleLabelText;

                // text-color
                //   Optional color. Defaults to #000000. Requires text-field. Exponential.
                //   The color with which the text will be drawn.
                if (paint?.TextColor != null)
                {
                    styleLabel.ForeColor = paint.TextColor.Evaluate(context.Resolution);
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
                    styleLabel.Halo.Color = paint.TextHaloColor.Evaluate(context.Resolution);
                }

                //text-halo-width
                //   Optional number. Units in pixels. Defaults to 0. Requires text-field. Exponential.
                //   Distance of halo to the font outline. Max text halo width is 1/4 of the font-size.
                if (paint?.TextHaloWidth != null)
                {
                    styleLabel.Halo.Width = paint.TextHaloWidth.Evaluate(context.Resolution);
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
                    styleLabel.Font.Size = layout.TextSize.Evaluate(context.Resolution);
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
                if (layout?.TextMaxWidth != null)
                {
                    styleLabel.MaxWidth = layout.TextMaxWidth.Evaluate(context.Resolution);
                }

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
                    switch (layout.TextAnchor)
                    {
                        case "left":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left;
                            break;
                        case "right":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right;
                            break;
                        case "top":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Top;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
                            break;
                        case "bottom":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
                            break;
                        case "top-left":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Top;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left;
                            break;
                        case "top-right":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Top;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right;
                            break;
                        case "bottom-left":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left;
                            break;
                        case "bottom-right":
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right;
                            break;
                        default:
                            styleLabel.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center;
                            styleLabel.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
                            break;
                    }
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
                    var x = layout.TextOffset[0] * styleLabel.Font.Size;
                    var y = layout.TextOffset[1] * styleLabel.Font.Size;
                    styleLabel.Offset = new Offset(x, y, false);
                }

                // text-allow-overlap
                //   Optional boolean. Defaults to false. Requires text-field. Interval.
                //   If true, the text will be visible even if it collides with other previously drawn symbols.
                if (layout?.TextAllowOverlap != null)
                {
                    // TODO
                    layout.TextAllowOverlap.Evaluate(context.Resolution);
                }

                // text-ignore-placement
                //   Optional boolean. Defaults to false. Requires text-field. Interval.
                //   If true, other symbols can be visible even if they collide with the text.
                if (layout?.TextIgnorePlacement != null)
                {
                    // TODO
                    layout.TextIgnorePlacement.Evaluate(context.Resolution);
                }

                // text-optional
                //   Optional boolean. Defaults to false. Requires text-field. Requires icon-image. Interval.
                //   If true, icons will display without their corresponding text when the text collides with other symbols and the icon does not.
                if (layout?.TextOptional != null)
                {
                    // TODO
                    layout.TextOptional.Evaluate(context.Resolution);
                }

                // text-halo-blur
                //   Optional number. Units in pixels. Defaults to 0. Requires text-field. Exponential.
                //   The halo's fadeout distance towards the outside.
            }

            // icon-image
            //   Optional string.
            //   A string with { tokens } replaced, referencing the data property to pull from. Interval.
            if (layout?.IconImage != null)
            {
                var name = ReplaceFields(layout.IconImage.Evaluate(context.Resolution), context.Feature.Tags);

                if (!string.IsNullOrEmpty(name) && spriteAtlas.ContainsKey(name) && spriteAtlas[name].Atlas >= 0)
                {
                    styleSymbol.BitmapId = spriteAtlas[name].Atlas;
                }
                else
                {
                    // No sprite found
                    styleSymbol.BitmapId = -1;
                    // Log information
                    Logging.Logger.Log(Logging.LogLevel.Information, $"Sprite {name} not found");
                }

                // icon-allow-overlap
                //   Optional boolean. Defaults to false. Requires icon-image. Interval.
                //   If true, the icon will be visible even if it collides with other previously drawn symbols.
                if (layout?.IconAllowOverlap != null)
                {
                    // TODO
                    layout.IconAllowOverlap.Evaluate(context.Resolution);
                }

                // icon-ignore-placement
                //   Optional boolean. Defaults to false. Requires icon-image. Interval.
                //   If true, other symbols can be visible even if they collide with the icon.
                if (layout?.IconIgnorePlacement != null)
                {
                    // TODO
                    layout.IconIgnorePlacement.Evaluate(context.Resolution);
                }

                // icon-optional
                //   Optional boolean. Defaults to false. Requires icon-image. Requires text-field. Interval.
                //   If true, text will display without their corresponding icons when the icon collides 
                //   with other symbols and the text does not.
                if (layout?.IconOptional != null)
                {
                    // TODO
                    layout.IconOptional.Evaluate(context.Resolution);
                }

                // icon-rotation-alignment
                //   Optional enum. One of map, viewport. Defaults to viewport. Requires icon-image. Interval.
                //   Orientation of icon when map is rotated.

                // icon-size
                //   Optional number. Defaults to 1. Requires icon-image. Exponential.
                //   Scale factor for icon. 1 is original size, 3 triples the size.
                if (layout?.IconSize != null)
                {
                    styleSymbol.SymbolScale = layout.IconSize.Evaluate(context.Resolution);
                }

                // icon-rotate
                //   Optional number. Units in degrees. Defaults to 0. Requires icon-image. Exponential.
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
                if (layout?.IconOffset != null)
                {
                    var x = layout.IconOffset[0];
                    var y = layout.IconOffset[1];
                    styleSymbol.SymbolOffset = new Offset(x, y, false);
                }

                // icon-opacity
                //   Optional number. Defaults to 1. Requires icon-image. Exponential.
                //   The opacity at which the icon will be drawn.
                if (layout?.IconOpacity != null)
                {
                    styleSymbol.Opacity = layout.IconOpacity.Evaluate(context.Resolution);
                }

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
                if (!string.IsNullOrEmpty(styleLabelText))
                {
                    styleLabel.Enabled = true;

                    result.Add(styleLabel);
                }

                if (styleSymbol.BitmapId >= 0)
                {
                    styleSymbol.Enabled = true;

                    result.Add(styleSymbol);
                }
            }
            else
            {
                styleVector.Enabled = true;

                result.Add(styleVector);
            }

            return result;
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
    }
}
