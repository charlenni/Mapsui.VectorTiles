using Mapsui.Styles.Thematics;
using Mapsui.VectorTiles.MapboxGLStyler.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.MapboxGLStyler.Json
{
    public class StyleLayer
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

        private float? _zoomMax;

        [JsonProperty("maxzoom")]
        public float? ZoomMax
        {
            get => _zoomMax;
            set
            {
                _zoomMax = value;
                if (_zoomMax != null)
                    MinVisible = (_zoomMax ?? 30).ToResolution();
            }
        }

        private float? _zoomMin;

        [JsonProperty("minzoom")]
        public float? ZoomMin
        {
            get => _zoomMin;
            set
            {
                _zoomMin = value;
                if (_zoomMin != null)
                    MaxVisible = (_zoomMin ?? 0f).ToResolution();
            }
        }

        public double MinVisible { get; set; } = 0;

        public double MaxVisible { get; set; } = double.PositiveInfinity;

        public IThemeStyle Style { get; set; }

        public override string ToString()
        {
            return Id + " " + Type;
        }
    }
}
