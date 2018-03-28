namespace Mapsui.VectorTiles.Filter
{
    public interface IFilter
    {
        bool Evaluate(EvaluationContext context);
    }
}
