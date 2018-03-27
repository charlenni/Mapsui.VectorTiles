namespace Mapsui.VectorTiles.MapboxGLStyler.Filter
{
    public abstract class Filter : IFilter
    {
        public abstract bool Evaluate(EvaluationContext context);
    }
}
