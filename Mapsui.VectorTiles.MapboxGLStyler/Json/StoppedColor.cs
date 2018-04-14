using System;
using System.Collections.Generic;
using Mapsui.Styles;
using Mapsui.VectorTiles.MapboxGLStyler.Extensions;
using Newtonsoft.Json;

namespace Mapsui.VectorTiles.MapboxGLStyler.Json
{
    public class StoppedColor
    {
        [JsonProperty("base")]
        public float Base { get; set; } = 1f;

        [JsonProperty("stops")]
        public IList<KeyValuePair<float, Color>> Stops { get; set; }

        public Color SingleVal { get; set; }

        /// <summary>
        /// Calculate the correct color for a stopped function
        /// No Bezier type up to now
        /// </summary>
        /// <param name="contextResolution">Zoom factor for calculation </param>
        /// <param name="stoppsType">Type of calculation (interpolate, exponential, categorical)</param>
        /// <returns>Value for this stopp respecting zoom factor and type</returns>
        public Color Evaluate(float? contextResolution, StopsType stoppsType = StopsType.Exponential)
        {
            // Are there no stopps, but a single value?
            if (SingleVal != null)
                return SingleVal;

            // Are there no stopps in array
            if (Stops.Count == 0)
                return null;

            float resolution = contextResolution ?? (float)0f.ToResolution();

            var lastResolution = Stops[0].Key;
            var lastColor = Stops[0].Value;

            if (lastResolution < resolution)
                return lastColor;

            for (int i = 1; i < Stops.Count; i++)
            {
                var nextResolution = Stops[i].Key;
                var nextColor = Stops[i].Value;

                if (lastResolution >= resolution && resolution >= nextResolution)
                {
                    switch (stoppsType)
                    {
                        case StopsType.Interval:
                            return lastColor;
                        case StopsType.Exponential:
                            var progress = lastResolution - resolution;
                            var difference = lastResolution - nextResolution;
                            if (difference < float.Epsilon)
                                return null;
                            float factor;
                            if (Base - 1 < float.Epsilon)
                                factor = progress / difference;
                            else
                                factor = (float)((Math.Pow(Base, progress) - 1) / (Math.Pow(Base, difference) - 1));
                            var r = (int)Math.Round(lastColor.R + (nextColor.R - lastColor.R) * factor);
                            var g = (int)Math.Round(lastColor.G + (nextColor.G - lastColor.G) * factor);
                            var b = (int)Math.Round(lastColor.B + (nextColor.B - lastColor.B) * factor);
                            var a = (int)Math.Round(lastColor.A + (nextColor.A - lastColor.A) * factor);
                            return new Color(r, g, b, a);
                        case StopsType.Categorical:
                            // ==
                            if (resolution - nextResolution < float.Epsilon)
                                return nextColor;
                            break;
                    }
                }

                lastResolution = nextResolution;
                lastColor = nextColor;
            }

            return lastColor;
        }
    }
}
