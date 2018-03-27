using System.Collections.Generic;

namespace Mapsui.VectorTiles.MapboxGLStyler.Filter
{
    public class NoneFilter : CompoundFilter
    {
        public NoneFilter(List<IFilter> filters) : base(filters)
        {
        }

        public override bool Evaluate(EvaluationContext context)
        {
            foreach (var filter in Filters)
            {
                if (filter.Evaluate(context))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
