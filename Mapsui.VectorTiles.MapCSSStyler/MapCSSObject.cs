using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mapsui.Geometries;

namespace Mapsui.VectorTiles.MapCSSStyler
{
    /// <summary>
    /// Represents a MapCSS object that can be interpreted by the MapCSS interpreter.
    /// </summary>
    public class MapCSSObject : ITagsSource
    {
        /// <summary>
        /// Creates a new MapCSS object with a geometry object.
        /// </summary>
        /// <param name="feature"></param>
        public MapCSSObject(Feature feature)
        {
            if (feature == null) throw new ArgumentNullException();

            this.Feature = feature;

            if (!(this.Feature.GeometryType is GeometryType.Polygon ||
                this.Feature.GeometryType is GeometryType.Point ||
                this.Feature.GeometryType is GeometryType.LineString))
            {
                throw new Exception("Invalid MapCSS type.");
            }
        }

        /// <summary>
        /// Gets the feature.
        /// </summary>
        public Feature Feature { get; set; }

        /// <summary>
        /// Returns true if this object contains a geometry object.
        /// </summary>
        public bool IsGeo
        {
            get
            {
                return this.Feature != null;
            }
        }

        /// <summary>
        /// Returns the type of MapCSS object.
        /// </summary>
        public MapCSSType MapCSSType
        {
            get
            {
                if (this.Feature.GeometryType is GeometryType.Point)
                {
                    return MapCSSType.Node;
                }
                else if (this.Feature.GeometryType is GeometryType.Polygon)
                {
                    return MapCSSType.Area;
                }
                else if (this.Feature.GeometryType is GeometryType.LineString)
                {
                    return MapCSSType.Way;
                }

                throw new Exception("Invalid MapCSS type.");
            }
        }

        /// <summary>
        /// Returns true if the tags- or attributecollection contains the given key.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            if (this.IsGeo)
            {
                return this.Feature.Tags != null &&
                    this.Feature.Tags.ContainsKey(key);
            }

            return false;
        }

        /// <summary>
        /// Returns true if the tags- or attributecollection contains the given key.
        /// Returns the value associated with the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="tagValue"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out string tagValue)
        {
            if (this.IsGeo)
            {
                string value;
                if (this.Feature.Tags != null &&
                    this.Feature.Tags.TryGetValue(key, out value))
                {
                    if (value != null)
                    {
                        tagValue = value.ToString();
                        return true;
                    }
                }
                tagValue = string.Empty;
                return false;
            }
            tagValue = string.Empty;
            return false;
        }
    }
}
