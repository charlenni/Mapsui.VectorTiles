using System.Collections.Generic;

namespace Mapsui.VectorTiles.Filter
{
    public class AllFilter : CompoundFilter
    {
        public AllFilter(List<IFilter> filters) : base(filters)
        {
        }

        public override bool Evaluate(EvaluationContext context)
        {
            foreach (var filter in Filters)
            {
                if (!filter.Evaluate(context))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
