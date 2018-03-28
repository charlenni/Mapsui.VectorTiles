using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles.Filter
{
    public abstract class BinaryFilter : Filter
    {
        public string Key { get; }
        public JValue Value { get; }

        public BinaryFilter()
        {
        }

        public BinaryFilter(string key, JValue value)
        {
            Key = key;
            Value = value;
        }

        public abstract override bool Evaluate(EvaluationContext context);
    }
}
