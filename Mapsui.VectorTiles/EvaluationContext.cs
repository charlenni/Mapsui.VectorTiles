namespace Mapsui.VectorTiles
{
    public class EvaluationContext
    {
        public float? Zoom { get; }
        public VectorTileFeature Feature { get; }

        public EvaluationContext(VectorTileFeature feature) : this(null, feature)
        { }

        public EvaluationContext(float? zoom, VectorTileFeature feature = null)
        {
            Zoom = zoom;
            Feature = feature;
        }
    }
}
