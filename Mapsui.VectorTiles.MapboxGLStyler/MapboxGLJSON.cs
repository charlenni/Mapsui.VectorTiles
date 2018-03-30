using System;
using System.Collections.Generic;
using Mapsui.Styles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.MapboxGLStyler
{
    public enum StoppsType
    {
        Exponential,
        Interval,
        Categorical
    }

    public class StoppedDouble
    {
        [JsonProperty("base")]
        public float Base { get; set; } = 1f;

        [JsonProperty("stops")]
        public IList<IList<object>> Stops { get; set; }

        public float SingleVal { get; set; } = float.MinValue;
    }

    public class StoppedString
    {
        [JsonProperty("base")]
        public float Base { get; set; } = 1f;

        [JsonProperty("stops")]
        public IList<IList<object>> Stops { get; set; }

        public string SingleVal { get; set; } = string.Empty;
    }

    public class Atlas
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("pixelRatio")]
        public float PixelRatio { get; set; }

        public int BitmapId { get; internal set; }

        //public Mapsui.Styles.Atlas ToMapsui()
        //{
        //    return new Styles.Atlas
        //    {
        //        X = this.X,
        //        Y = this.Y,
        //        Width = this.Width,
        //        Height = this.Height,
        //        PixelRatio = this.PixelRatio
        //    };
        //}
    }

    public class Paint
    {
        [JsonProperty("background-color")]
        public string BackgroundColor { get; set; }

        [JsonConverter(typeof(StoppedStringConverter))]
        [JsonProperty("fill-color")]
        public StoppedString FillColor { get; set; }

        [JsonProperty("fill-opacity")]
        public float? FillOpacity { get; set; }

        [JsonConverter(typeof(StoppedStringConverter))]
        [JsonProperty("line-color")]
        public StoppedString LineColor { get; set; }

        [JsonConverter(typeof(StoppedDoubleConverter))]
        [JsonProperty("line-width")]
        public StoppedDouble LineWidth { get; set; }

        [JsonProperty("fill-translate")]
        public object FillTranslate { get; set; }

        [JsonProperty("fill-pattern")]
        public string FillPattern { get; set; }

        [JsonConverter(typeof(StoppedStringConverter))]
        [JsonProperty("fill-outline-color")]
        public StoppedString FillOutlineColor { get; set; }

        [JsonProperty("line-dasharray")]
        public object LineDasharray { get; set; }

        [JsonConverter(typeof(StoppedDoubleConverter))]
        [JsonProperty("line-opacity")]
        public StoppedDouble LineOpacity { get; set; }

        [JsonConverter(typeof(StoppedStringConverter))]
        [JsonProperty("text-color")]
        public StoppedString TextColor { get; set; }

        [JsonConverter(typeof(StoppedDoubleConverter))]
        [JsonProperty("text-halo-width")]
        public StoppedDouble TextHaloWidth { get; set; }

        [JsonConverter(typeof(StoppedStringConverter))]
        [JsonProperty("text-halo-color")]
        public StoppedString TextHaloColor { get; set; }

        [JsonProperty("text-halo-blur")]
        public float? TextHaloBlur { get; set; }

        [JsonProperty("fill-antialias")]
        public bool? FillAntialias { get; set; }

        [JsonProperty("fill-translate-anchor")]
        public string FillTranslateAnchor { get; set; }

        [JsonProperty("line-gap-width")]
        public object LineGapWidth { get; set; }

        [JsonProperty("line-blur")]
        public object LineBlur { get; set; }

        [JsonProperty("line-translate")]
        public IList<int> LineTranslate { get; set; }

        [JsonConverter(typeof(StoppedStringConverter))]
        [JsonProperty("icon-halo-color")]
        public StoppedString IconHaloColor { get; set; }

        [JsonProperty("icon-halo-width")]
        public int? IconHaloWidth { get; set; }

        [JsonConverter(typeof(StoppedDoubleConverter))]
        [JsonProperty("text-opacity")]
        public StoppedDouble TextOpacity { get; set; }

        [JsonConverter(typeof(StoppedStringConverter))]
        [JsonProperty("icon-color")]
        public StoppedString IconColor { get; set; }

        [JsonProperty("text-translate")]
        public IList<int> TextTranslate { get; set; }

        [JsonConverter(typeof(StoppedDoubleConverter))]
        [JsonProperty("icon-opacity")]
        public StoppedDouble IconOpacity { get; set; }
    }

    public class Layout
    {
        [JsonProperty("line-cap")]
        public string LineCap { get; set; }

        [JsonProperty("line-join")]
        public string LineJoin { get; set; }

        [JsonProperty("visibility")]
        public string Visibility { get; set; }

        [JsonProperty("text-font")]
        public JArray TextFont { get; set; }

        [JsonProperty("text-field")]
        public string TextField { get; set; }

        [JsonProperty("text-max-width")]
        public object TextMaxWidth { get; set; }

        [JsonConverter(typeof(StoppedDoubleConverter))]
        [JsonProperty("text-size")]
        public StoppedDouble TextSize { get; set; }

        [JsonConverter(typeof(StoppedDoubleConverter))]
        [JsonProperty("text-padding")]
        public StoppedDouble TextPadding { get; set; }

        [JsonProperty("text-offset")]
        public object TextOffset { get; set; }

        [JsonProperty("text-anchor")]
        public object TextAnchor { get; set; }

        [JsonProperty("symbol-placement")]
        public object SymbolPlacement { get; set; }

        [JsonProperty("text-rotation-alignment")]
        public string TextRotationAlignment { get; set; }

        [JsonProperty("icon-rotation-alignment")]
        public string IconRotationAlignment { get; set; }

        [JsonProperty("text-transform")]
        public string TextTransform { get; set; }

        [JsonProperty("text-letter-spacing")]
        public float? TextLetterSpacing { get; set; }

        [JsonProperty("text-line-height")]
        public float? TextLineHeight { get; set; }

        [JsonConverter(typeof(StoppedStringConverter))]
        [JsonProperty("icon-image")]
        public StoppedString IconImage { get; set; }

        [JsonProperty("symbol-spacing")]
        public object SymbolSpacing { get; set; }

        [JsonProperty("icon-padding")]
        public int? IconPadding { get; set; }

        [JsonConverter(typeof(StoppedDoubleConverter))]
        [JsonProperty("icon-size")]
        public StoppedDouble IconSize { get; set; }

        [JsonProperty("icon-allow-overlap")]
        public bool? IconAllowOverlap { get; set; }

        [JsonProperty("icon-ignore-placement")]
        public bool? IconIgnorePlacement { get; set; }

        [JsonProperty("text-max-angle")]
        public int? TextMaxAngle { get; set; }
    }

    public class Layer
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("paint")]
        public Paint Paint { get; set; }

        [JsonProperty("interactive")]
        public bool Interactive { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("source-layer")]
        public string SourceLayer { get; set; }

        [JsonProperty("filter")]
        public JArray NativeFilter { get; set; }

        public Filter.IFilter Filter { get; set; }

        //[JsonProperty("metadata")]
        //public Metadata { get; set; }

        [JsonProperty("layout")]
        public Layout Layout { get; set; }

        [JsonProperty("ref")]
        public string Ref { get; set; }

        [JsonProperty("maxzoom")]
        public int? ZoomMax { get; set; }

        [JsonProperty("minzoom")]
        public int? ZoomMin { get; set; }

        public List<IStyle> Style { get; set; }

        public override string ToString()
        {
            return Id + " " + Type;
        }
    }

    public class MapboxJson
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        //[JsonProperty("metadata")]
        //public Metadata Metadata { get; set; }

        [JsonProperty("sources")]
        public object Sources { get; set; }

        [JsonProperty("sprite")]
        public string Sprite { get; set; }

        [JsonProperty("glyphs")]
        public string Glyphs { get; set; }

        [JsonProperty("layers")]
        public IList<Layer> Layers { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("modified")]
        public DateTime Modified { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("draft")]
        public bool Draft { get; set; }

        [JsonProperty("center")]
        public IList<float> Center { get; set; }

        [JsonProperty("zoom")]
        public float? Zoom { get; set; }
    }
}
