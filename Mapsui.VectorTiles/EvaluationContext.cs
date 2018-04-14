namespace Mapsui.VectorTiles
{
    public class EvaluationContext
    {
        public float? Resolution { get; }
        public VectorTileFeature Feature { get; }

        public EvaluationContext(VectorTileFeature feature) : this(null, feature)
        { }

        public EvaluationContext(float? resolution, VectorTileFeature feature = null)
        {
            Resolution = resolution;
            Feature = feature;
        }
    }
}
