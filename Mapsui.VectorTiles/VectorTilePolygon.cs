namespace Mapsui.VectorTiles
{
    public class VectorTilePolygon
    {
        private VectorTileGeometry geometry;

        public VectorTilePolygon(VectorTileGeometry geometry)
        {
            this.geometry = geometry;
        }

        // method assuming polygon is closed (first point is the same as last point)
        public double SignedArea()
        {
            var sum = 0.0;
            for (var i = 0; i < geometry.Points.Count-1; i++)
            {
                sum = sum + (geometry.Points[i].X * geometry.Points[i + 1].Y - (geometry.Points[i].Y * geometry.Points[i + 1].X));
            }
            return 0.5 * sum;
        }

        public bool IsOuterRing()
        {
            return IsCCW();
        }

        public bool IsCW()
        {
            return SignedArea() < 0;
        }

        public bool IsCCW()
        {
            return SignedArea() > 0;
        }
    }
}