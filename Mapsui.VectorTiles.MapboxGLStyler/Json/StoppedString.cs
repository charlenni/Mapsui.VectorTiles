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
        /// <param name="contextResolution">Resolution factor for calculation </param>
        /// <returns>Value for this stopp respecting resolution factor and type</returns>
        public string Evaluate(float? contextResolution)
        {
            // Are there no stopps, but a single value?
            if (SingleVal != string.Empty)
                return SingleVal;

            // Are there no stopps in array
            if (Stops.Count == 0)
                return string.Empty;

            float resolution = contextResolution ?? (float)0f.ToResolution();

            var lastResolution = Stops[0].Key;
            var lastValue = Stops[0].Value;

            if (lastResolution < resolution)
                return lastValue;

            for (int i = 1; i < Stops.Count; i++)
            {
                var nextResolution = Stops[i].Key;
                var nextValue = Stops[i].Value;

                if (lastResolution >= resolution && resolution >= nextResolution)
                {
                    return lastValue;
                }

                lastResolution = nextResolution;
                lastValue = nextValue;
            }

            return lastValue;
        }
    }
}
