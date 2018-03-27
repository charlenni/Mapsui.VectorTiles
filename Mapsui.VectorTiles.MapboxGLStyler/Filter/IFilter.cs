namespace Mapsui.VectorTiles.MapboxGLStyler.Filter
{
    public interface IFilter
    {
        bool Evaluate(EvaluationContext context);
    }
}
