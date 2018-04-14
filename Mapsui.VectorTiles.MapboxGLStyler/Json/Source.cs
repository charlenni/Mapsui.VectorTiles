using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.MapboxGLStyler.Json
{
    /// <summary>
    /// Source in TileJSON format
    /// See https://github.com/mapbox/tilejson-spec/tree/master/2.2.0
    /// </summary>
    public class Source
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("tileSize")]
        public int TileSize { get; set; }

        [JsonProperty("pixel_size")]
        public int PixelSize { get; set; }

        [JsonProperty("vector_layers")]
        public IList<StyleLayer> VectorLayers { get; set; }

        [JsonProperty("tilejson")]
        public string TileJson { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("attribution")]
        public string Attribution { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("legend")]
        public string Legend { get; set; }

        [JsonProperty("scheme")]
        public string Scheme { get; set; }

        [JsonProperty("tiles")]
        public IList<string> Tiles { get; set; }

        [JsonProperty("grids")]
        public IList<string> Grids { get; set; }

        [JsonProperty("data")]
        public IList<string> Data { get; set; }

        [JsonProperty("minzoom")]
        public int? ZoomMin { get; set; }

        [JsonProperty("maxzoom")]
        public int? ZoomMax { get; set; }

        [JsonProperty("bounds")]
        public JValue[] Bounds { get; set; }

        [JsonProperty("center")]
        public JValue[] Center { get; set; }

        public override string ToString()
        {
            return Id + " " + Type;
        }
    }
}
