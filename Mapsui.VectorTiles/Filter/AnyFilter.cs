using System.Collections.Generic;

namespace Mapsui.VectorTiles.Filter
{
    public class AnyFilter : CompoundFilter
    {
        public AnyFilter(List<IFilter> filters) : base(filters)
        {
        }

        public override bool Evaluate(EvaluationContext context)
        {
            foreach (var filter in Filters)
            {
                if (filter.Evaluate(context))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
