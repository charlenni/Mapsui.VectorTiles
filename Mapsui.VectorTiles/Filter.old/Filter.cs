namespace Mapsui.VectorTiles.Filter
{
    public abstract class Filter : IFilter
    {
        public abstract bool Evaluate(EvaluationContext context);
    }
}
