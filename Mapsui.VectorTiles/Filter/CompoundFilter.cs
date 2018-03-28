using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.Filter
{
    public abstract class CompoundFilter : Filter
    {
        public List<IFilter> Filters { get; }

        public CompoundFilter()
        {
        }

        public CompoundFilter(List<IFilter> filters)
        {
            Filters = new List<IFilter>();

            foreach (var filter in filters)
            {
                Filters.Add(filter);
            }
        }

        public abstract override bool Evaluate(EvaluationContext context);
    }
}
