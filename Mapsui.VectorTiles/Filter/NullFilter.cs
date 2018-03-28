namespace Mapsui.VectorTiles.Filter
{
    public class NullFilter : Filter
    {
        public override bool Evaluate(EvaluationContext context)
        {
            return true;
        }
    }
}
