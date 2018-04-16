using System;
using System.Collections.Generic;
using Mapsui.VectorTiles.MapboxGLStyler.Extensions;
using Newtonsoft.Json;

namespace Mapsui.VectorTiles.MapboxGLStyler.Json
{
    /// <summary>
    /// Class holding StoppedFloat data in Json format
    /// </summary>
    public class StoppedFloat
    {
        [JsonProperty("base")]
        public float Base { get; set; } = 1f;

        [JsonProperty("stops")]
        public IList<KeyValuePair<float, float>> Stops { get; set; }

        public float SingleVal { get; set; } = float.MinValue;

        /// <summary>
        /// Calculate the correct value for a stopped function
        /// No Bezier type up to now
        /// </summary>
        /// <param name="contextResolution">Zoom factor for calculation </param>
        /// <param name="stoppsType">Type of calculation (interpolate, exponential, categorical)</param>
        /// <returns>Value for this stopp respecting zoom factor and type</returns>
        public float Evaluate(float? contextResolution, StopsType stoppsType = StopsType.Exponential)
        {
            // Are there no stopps, but a single value?
            // !=
            if (SingleVal - float.MinValue > float.Epsilon)
                return SingleVal;

            // Are there no stopps in array
            if (Stops.Count == 0)
                return 0;

            float resolution = contextResolution ?? (float)0f.ToResolution();

            var lastResolution = Stops[0].Key;
            var lastValue = Stops[0].Value;

            if (lastResolution < resolution)
                return lastValue;

            for (int i = 1; i < Stops.Count; i++)
            {
                var nextResolution = Stops[i].Key;
                var nextValue = Stops[i].Value;

                if (resolution == nextResolution)
                    return nextValue;

                if (lastResolution >= resolution && resolution > nextResolution)
                {
                    switch (stoppsType)
                    {
                        case StopsType.Interval:
                            return lastValue;
                        case StopsType.Exponential:
                            var progress = lastResolution - resolution;
                            var difference = lastResolution - nextResolution;
                            if (difference < float.Epsilon)
                                return 0;
                            if (Base - 1.0f < float.Epsilon)
                                return lastValue + (nextValue - lastValue) * progress / difference;
                            else
                            {
                                //var r = FromResolution(resolution);
                                //var lr = FromResolution(lastResolution);
                                //var nr = FromResolution(nextResolution);
                                var logBase = Math.Log(Base);
                                //return lastValue + (float)((nextValue - lastValue) * (Math.Pow(Base, lr-r) - 1) / (Math.Pow(Base, lr-nr) - 1));
                                return lastValue + (float)((nextValue - lastValue) * (Math.Exp(progress * logBase) - 1) / (Math.Exp(difference * logBase) - 1)); // (Math.Pow(Base, progress) - 1) / (Math.Pow(Base, difference) - 1));
                            }
                        case StopsType.Categorical:
                            if (resolution - nextResolution < float.Epsilon)
                                return nextValue;
                            break;
                    }
                }

                lastResolution = nextResolution;
                lastValue = nextValue;
            }

            return lastValue;
        }

        private double FromResolution(double resolution)
        {
            var zoom = Math.Log(78271.51696401953125 / resolution, 2);

            return zoom < 0 ? 0 : zoom;
        }
    }
}
