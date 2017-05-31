namespace Mapsui.VectorTiles
{
    using Mapsui.Geometries;
    using System.Collections.Generic;

    public class VectorTileGeometry
    {
        //GeometryType type = GeometryType.Unknown;

        //public GeometryType Type
        //{
        //    get { return type; }
        //}

        List<Point> points;

        public List<Point> Points
        {
            get
            {
                return points;
            }
        }

        public VectorTileGeometry()
        {
            this.points = new List<Point>();
        }

        public VectorTileGeometry(List<Point> points)
        {
            this.points = new List<Point>();
            this.points.AddRange(points);
        }

        public VectorTileGeometry(Point point)
        {
            this.points = new List<Point>();
            this.points.Add(point);
        }

        public void Reverse()
        {
            points.Reverse();
        }
    }
}
