using System.Collections.Generic;
using Mapsui.Styles;
using Newtonsoft.Json;

namespace Mapsui.VectorTiles.MapboxGLStyler.Json
{
    public class Paint
    {
        [JsonConverter(typeof(ColorConverter))]
        [JsonProperty("background-color")]
        public Color BackgroundColor { get; set; }

        [JsonConverter(typeof(StoppedColorConverter))]
        [JsonProperty("fill-color")]
        public StoppedColor FillColor { get; set; }

        [JsonProperty("fill-opacity")]
        public float? FillOpacity { get; set; }

        [JsonConverter(typeof(StoppedColorConverter))]
        [JsonProperty("line-color")]
        public StoppedColor LineColor { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("line-width")]
        public StoppedFloat LineWidth { get; set; }

        [JsonProperty("fill-translate")]
        public object FillTranslate { get; set; }

        [JsonConverter(typeof(StoppedStringConverter))]
        [JsonProperty("fill-pattern")]
        public StoppedString FillPattern { get; set; }

        [JsonConverter(typeof(StoppedColorConverter))]
        [JsonProperty("fill-outline-color")]
        public StoppedColor FillOutlineColor { get; set; }

        [JsonProperty("line-dasharray")]
        public object LineDasharray { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("line-opacity")]
        public StoppedFloat LineOpacity { get; set; }

        [JsonConverter(typeof(StoppedColorConverter))]
        [JsonProperty("text-color")]
        public StoppedColor TextColor { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("text-halo-width")]
        public StoppedFloat TextHaloWidth { get; set; }

        [JsonConverter(typeof(StoppedColorConverter))]
        [JsonProperty("text-halo-color")]
        public StoppedColor TextHaloColor { get; set; }

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

        [JsonConverter(typeof(StoppedColorConverter))]
        [JsonProperty("icon-halo-color")]
        public StoppedColor IconHaloColor { get; set; }

        [JsonProperty("icon-halo-width")]
        public int? IconHaloWidth { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("text-opacity")]
        public StoppedFloat TextOpacity { get; set; }

        [JsonConverter(typeof(StoppedColorConverter))]
        [JsonProperty("icon-color")]
        public StoppedColor IconColor { get; set; }

        [JsonProperty("text-translate")]
        public IList<int> TextTranslate { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("icon-opacity")]
        public StoppedFloat IconOpacity { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("raster-opacity")]
        public StoppedFloat RasterOpacity { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("raster-hue-rotate")]
        public StoppedFloat RasterHueRotate { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("raster-brightness-min")]
        public StoppedFloat RasterBrightnessMin { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("raster-brightness-max")]
        public StoppedFloat RasterBrightnessMax { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("raster-saturation")]
        public StoppedFloat RasterSaturation { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("raster-contrast")]
        public StoppedFloat RasterContrast { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("raster-fade-duration")]
        public StoppedFloat RasterFadeDuration { get; set; }
    }
}
