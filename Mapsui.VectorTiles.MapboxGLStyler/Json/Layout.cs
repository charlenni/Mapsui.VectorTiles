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

        [JsonProperty("text-max-width")]
        public object TextMaxWidth { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("text-size")]
        public StoppedFloat TextSize { get; set; }

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("text-padding")]
        public StoppedFloat TextPadding { get; set; }

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

        [JsonConverter(typeof(StoppedFloatConverter))]
        [JsonProperty("icon-size")]
        public StoppedFloat IconSize { get; set; }

        [JsonProperty("icon-allow-overlap")]
        public bool? IconAllowOverlap { get; set; }

        [JsonProperty("icon-ignore-placement")]
        public bool? IconIgnorePlacement { get; set; }

        [JsonProperty("text-max-angle")]
        public int? TextMaxAngle { get; set; }
    }
}
