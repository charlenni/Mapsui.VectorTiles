namespace Mapsui.VectorTiles.MapboxGLStyler.Filter
{
    public class NullFilter : Filter
    {
        public override bool Evaluate(EvaluationContext context)
        {
            return true;
        }
    }
}
