﻿using Mapsui.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapsui.VectorTiles
{
    public sealed class VectorTileFeature
    {
        string id;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        readonly List<Tag> tags = new List<Tag>();

        public List<Tag> Tags
        {
            get { return tags; }
        }

        GeometryType geometryType = GeometryType.Unknown;

        public GeometryType GeometryType
        {
            get { return geometryType; }
            set { geometryType = value; }
        }

        readonly List<VectorTileGeometry> geometry = new System.Collections.Generic.List<VectorTileGeometry>();

        public List<VectorTileGeometry> Geometry
        {
            get { return geometry; }
        }

        public uint Extent
        {
            get { return 1; }
        }

        public VectorTileFeature(string id = "")
        {
            this.id = id;
        }
    }
}