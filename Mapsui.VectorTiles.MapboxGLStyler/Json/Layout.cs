using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.MapboxGLStyler.Json
{
    /// <summary>
    /// Class holding Layout data in Json format
    /// </summary>
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

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("text-max-width")]
        public StoppedFloat TextMaxWidth { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("text-size")]
        public StoppedFloat TextSize { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("text-padding")]
        public StoppedFloat TextPadding { get; set; }

        [JsonProperty("text-offset")]
        public float[] TextOffset { get; set; }

        [JsonConverter(typeof(StoppedBooleanConverter))]
        [JsonProperty("text-optional")]
        public StoppedBoolean TextOptional { get; set; }

        [JsonConverter(typeof(StoppedBooleanConverter))]
        [JsonProperty("text-allow-overlap")]
        public StoppedBoolean TextAllowOverlap { get; set; }

        [JsonConverter(typeof(StoppedBooleanConverter))]
        [JsonProperty("text-ignore-placement")]
        public StoppedBoolean TextIgnorePlacement { get; set; }

        [JsonProperty("text-anchor")]
        public string TextAnchor { get; set; }

        [JsonProperty("text-justify")]
        public string TextJustify { get; set; }

        [JsonProperty("text-rotation-alignment")]
        public string TextRotationAlignment { get; set; }

        [JsonProperty("icon-rotation-alignment")]
        public string IconRotationAlignment { get; set; }

        [JsonProperty("text-max-angle")]
        public int? TextMaxAngle { get; set; }

        [JsonProperty("text-transform")]
        public string TextTransform { get; set; }

        [JsonProperty("text-letter-spacing")]
        public float? TextLetterSpacing { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("text-line-height")]
        public StoppedFloat TextLineHeight { get; set; }

        [JsonConverter(typeof(StoppedStringConverter))]
        [JsonProperty("icon-image")]
        public StoppedString IconImage { get; set; }

        [JsonConverter(typeof(StoppedStringConverter))]
        [JsonProperty("symbol-placement")]
        public StoppedString SymbolPlacement { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("symbol-spacing")]
        public StoppedFloat SymbolSpacing { get; set; }

        [JsonProperty("icon-padding")]
        public int? IconPadding { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("icon-size")]
        public StoppedFloat IconSize { get; set; }

        [JsonProperty("icon-offset")]
        public float[] IconOffset { get; set; }

        [JsonConverter(typeof(StoppedBooleanConverter))]
        [JsonProperty("icon-optional")]
        public StoppedBoolean IconOptional { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("icon-opacity")]
        public StoppedFloat IconOpacity { get; set; }

        [JsonConverter(typeof(StoppedBooleanConverter))]
        [JsonProperty("icon-allow-overlap")]
        public StoppedBoolean IconAllowOverlap { get; set; }

        [JsonConverter(typeof(StoppedBooleanConverter))]
        [JsonProperty("icon-ignore-placement")]
        public StoppedBoolean IconIgnorePlacement { get; set; }
    }
}
