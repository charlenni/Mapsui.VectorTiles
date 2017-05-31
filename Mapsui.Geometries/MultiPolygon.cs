// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mapsui.Geometries
{
    /// <summary>
    ///     A MultiPolygon is a MultiSurface whose elements are Polygons.
    /// </summary>
    public class MultiPolygon : GeometryCollection
    {
        /// <summary>
        ///     Instantiates a MultiPolygon
        /// </summary>
        public MultiPolygon()
        {
            Polygons = new Collection<Polygon>();
        }

        /// <summary>
        ///     Collection of polygons in the multipolygon
        /// </summary>
        public IList<Polygon> Polygons { get; set; }

        /// <summary>
        ///     Returns an indexed geometry in the collection
        /// </summary>
        /// <param name="index">Geometry index</param>
        /// <returns>Geometry at index</returns>
        public new Polygon this[int index] => Polygons[index];

        /// <summary>
        ///     Returns summed area of the Polygons in the MultiPolygon collection
        /// </summary>
        public double Area
        {
            get { return Polygons.Sum(polygon => polygon.Area); }
        }

        /// <summary>
        ///     Returns the number of geometries in the collection.
        /// </summary>
        public override int NumGeometries
        {
            get { return Polygons.Count; }
        }

        /// <summary>
        ///     If true, then this Geometry represents the empty point set, Ø, for the coordinate space.
        /// </summary>
        /// <returns>Returns 'true' if this Geometry is the empty geometry</returns>
        public override bool IsEmpty()
        {
            if ((Polygons == null) || (Polygons.Count == 0)) return true;
            return Polygons.All(polygon => polygon.IsEmpty());
        }

        /// <summary>
        ///     Returns the shortest distance between any two points in the two geometries
        ///     as calculated in the spatial reference system of this Geometry.
        /// </summary>
        /// <param name="point">Geometry to calculate distance to</param>
        /// <returns>Shortest distance between any two points in the two geometries</returns>
        public override double Distance(Point point)
        {
            var minDistance = double.MaxValue;
            foreach (var geometry in Collection)
            {
                minDistance = Math.Min(minDistance, geometry.Distance(point));
            }
            return minDistance;
        }

        /// <summary>
        ///     Returns an indexed geometry in the collection
        /// </summary>
        /// <param name="n">Geometry index</param>
        /// <returns>Geometry at index N</returns>
        public override Geometry Geometry(int n)
        {
            return Polygons[n];
        }

        /// <summary>
        ///     Returns the bounding box of the object
        /// </summary>
        /// <returns>bounding box</returns>
        public override BoundingBox GetBoundingBox()
        {
            if ((Polygons == null) || (Polygons.Count == 0))
                return null;
            var bbox = Polygons[0].GetBoundingBox();
            for (var i = 1; i < Polygons.Count; i++)
            {
                bbox = bbox.Join(Polygons[i].GetBoundingBox());
            }
            return bbox;
        }

        /// <summary>
        ///     Return a copy of this geometry
        /// </summary>
        /// <returns>Copy of Geometry</returns>
        public new MultiPolygon Clone()
        {
            var geoms = new MultiPolygon();
            foreach (var polygon in Polygons)
            {
                geoms.Polygons.Add(polygon.Clone());
            }
            return geoms;
        }

        /// <summary>
        ///     Gets an enumerator for enumerating the geometries in the GeometryCollection
        /// </summary>
        /// <returns></returns>
        public override IEnumerator<Geometry> GetEnumerator()
        {
            foreach (var polygon in Polygons)
            {
                yield return polygon;
            }
        }
    }
}