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
        /// <param name="contextZoom">Zoom factor for calculation </param>
        /// <param name="stoppsType">Type of calculation (interpolate, exponential, categorical)</param>
        /// <returns>Value for this stopp respecting zoom factor and type</returns>
        public Color Evaluate(float? contextZoom, StopsType stoppsType = StopsType.Exponential)
        {
            // Are there no stopps, but a single value?
            if (SingleVal != null)
                return SingleVal;

            // Are there no stopps in array
            if (Stops.Count == 0)
                return null;

            float zoom = contextZoom ?? 0f;

            var lastZoom = Stops[0].Key;
            var lastColor = Stops[0].Value;

            if (lastZoom > zoom)
                return lastColor;

            for (int i = 1; i < Stops.Count; i++)
            {
                var nextZoom = Stops[i].Key;
                var nextColor = Stops[i].Value;

                if (zoom == nextZoom)
                    return nextColor;

                if (lastZoom <= zoom && zoom < nextZoom)
                {
                    switch (stoppsType)
                    {
                        case StopsType.Interval:
                            return lastColor;
                        case StopsType.Exponential:
                            var progress = zoom - lastZoom;
                            var difference = nextZoom - lastZoom;
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
                            if (nextZoom - zoom < float.Epsilon)
                                return nextColor;
                            break;
                    }
                }

                lastZoom = nextZoom;
                lastColor = nextColor;
            }

            return lastColor;
        }
    }
}
