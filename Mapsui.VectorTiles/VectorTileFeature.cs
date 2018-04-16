﻿using Mapsui.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapsui.Providers;
using Mapsui.Styles;
using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles
{
    public sealed class VectorTileFeature : IFeature
    {
        public string Id { get; set; }

        /// <summary>
        /// Hash code of vector tile layer, to which this vector tile feature belongs
        /// </summary>
        public int VectorTileLayer { get; }

        public TagsCollection Tags { get; } = new TagsCollection();

        public int Rank { get; set; } = int.MaxValue;

        public GeometryType GeometryType { get; set; } = GeometryType.Unknown;

        public IGeometry Geometry { get; set; }

        public BoundingBox Bounds { get; set; }

        public IDictionary<IStyle, object> RenderedGeometry { get; } = null;

        public ICollection<IStyle> Styles { get; } = new List<IStyle>();

        public object this[string key]
        {
            get => Tags[key];
            set => Tags[key] = value as JValue;
        }

        public IEnumerable<string> Fields
        {
            get => Tags.Keys;
        }

        public uint Extent
        {
            get { return 1; }
        }

        public VectorTileFeature(string layer, string id = "")
        {
            VectorTileLayer = layer.GetHashCode();
            Id = id;
        }
    }
}