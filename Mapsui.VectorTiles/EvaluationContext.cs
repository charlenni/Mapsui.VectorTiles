namespace Mapsui.VectorTiles
{
    public class EvaluationContext
    {
        public float? Zoom { get; }
        public Feature Feature { get; }

        public EvaluationContext(Feature feature) : this(null, feature)
        { }

        public EvaluationContext(float? zoom, Feature feature = null)
        {
            Zoom = zoom;
            Feature = feature;
        }
    }
}
