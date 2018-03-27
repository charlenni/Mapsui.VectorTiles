using System;
using System.Linq;

namespace Mapsui.VectorTiles.MapCSSStyler.v0_2
{
    /// <summary>
    /// Enumeration of MapCSS types.
    /// </summary>
    public enum MapCSSTypes
    {
        /// <summary>
        /// A node.
        /// </summary>
        Node,

        /// <summary>
        /// A way.
        /// </summary>
        Way,

        /// <summary>
        /// A relation.
        /// </summary>
        Relation,

        /// <summary>
        /// A line (A way where the start and finish nodes are not the same node, or area=no has been set).
        /// </summary>
        Line,

        /// <summary>
        /// An area (A way where the start and finish nodes are the same node, or area=yes has been set).
        /// </summary>
        Area
    }

    /// <summary>
    /// Contains MapCSS extensions.
    /// </summary>
    public static class MapCSSTypesExtensions
    {
        /// <summary>
        /// Returns true if the given object is of the given type.
        /// </summary>
        /// <param name="vtf"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static bool IsOfType(this Feature vtf, MapCSSTypes types)
        {
            string area = string.Empty;
            switch (types)
            {
                case MapCSSTypes.Node:
                    return vtf.GeometryType == GeometryType.Point;
                case MapCSSTypes.Way:
                    return vtf.GeometryType == GeometryType.LineString;
                case MapCSSTypes.Relation:
                    // TODO: We don't have such elements in databases
                    // return vtf.GeometryType == CompleteOsmType.Relation;
                case MapCSSTypes.Line:
                    if (vtf.GeometryType == GeometryType.LineString)
                    { // the type is way way. now check for a line.
                        var way = vtf.Geometry.First();
                        if (way != null &&
                            way.Points[0] == way.Points[way.Points.Count - 1])
                        { // first node is the same as the last one.
                            if (vtf.Tags != null &&
                                vtf.Tags.TryGetValue("area", out area) &&
                                area == "yes")
                            { // oeps, an area.
                                return false;
                            }
                            return true;
                        }
                        else
                        { // first node is different from the last one.
                            return true; // even if there is an area=yes tag this cannot be an area.
                        }
                    }
					break;
                case MapCSSTypes.Area:
                    return vtf.GeometryType == GeometryType.Polygon;
                default:
                    throw new ArgumentOutOfRangeException("types");
            }
            return false;
        }
    }
}