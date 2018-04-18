using System.Collections.Generic;
using Mapsui.VectorTiles.MapboxGLStyler.Extensions;
using Newtonsoft.Json;

namespace Mapsui.VectorTiles.MapboxGLStyler.Json
{
    /// <summary>
    /// Class holding StoppedString data in Json format
    /// </summary>
    public class StoppedString
    {
        [JsonProperty("base")]
        public float Base { get; set; } = 1f;

        [JsonProperty("stops")]
        public IList<KeyValuePair<float, string>> Stops { get; set; }

        public string SingleVal { get; set; } = string.Empty;

        /// <summary>
        /// Calculate the correct string for a stopped function
        /// No StoppsType needed, because strings couldn't interpolated :)
        /// </summary>
        /// <param name="contextZoom">Zoom factor for calculation </param>
        /// <returns>Value for this stopp respecting resolution factor and type</returns>
        public string Evaluate(float? contextZoom)
        {
            // Are there no stopps, but a single value?
            if (SingleVal != string.Empty)
                return SingleVal;

            // Are there no stopps in array
            if (Stops.Count == 0)
                return string.Empty;

            float zoom = contextZoom ?? 0f;

            var lastZoom = Stops[0].Key;
            var lastValue = Stops[0].Value;

            if (lastZoom > zoom)
                return lastValue;

            for (int i = 1; i < Stops.Count; i++)
            {
                var nextZoom = Stops[i].Key;
                var nextValue = Stops[i].Value;

                if (zoom == nextZoom)
                    return nextValue;

                if (lastZoom <= zoom && zoom < nextZoom)
                {
                    return lastValue;
                }

                lastZoom = nextZoom;
                lastValue = nextValue;
            }

            return lastValue;
        }
    }
}
