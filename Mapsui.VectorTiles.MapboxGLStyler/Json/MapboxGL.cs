using System;
using System.Collections.Generic;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.VectorTiles.MapboxGLStyler.Json;
using Newtonsoft.Json;

namespace Mapsui.VectorTiles.MapboxGLStyler.Json
{
    /// <summary>
    /// Class for holding MapboxGL file data in Json format
    /// </summary>
    public class MapboxGL
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        //[JsonProperty("metadata")]
        //public Metadata Metadata { get; set; }

        [JsonProperty("sources")]
        public Dictionary<string, Source> Sources { get; set; }

        [JsonProperty("sprite")]
        public string Sprite { get; set; }

        [JsonProperty("glyphs")]
        public string Glyphs { get; set; }

        [JsonProperty("layers")]
        public IList<StyleLayer> StyleLayers { get; set; }

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
