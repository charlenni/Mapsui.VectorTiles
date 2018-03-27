namespace Mapsui.VectorTiles.MapsforgeStyler.Rules
{
    internal class PositiveRuleKV : Rule
    {
        internal readonly string mKey;
        internal readonly string mValue;

        internal PositiveRuleKV(int element, int zoom, int selector, string key, string value, Rule[] subRules, RenderStyle[] styles)
            : base(element, zoom, selector, subRules, styles)
        {
            mKey = key;
            mValue = value;
        }

        public override bool MatchesTags(Tag[] tags)
        {
            foreach (Tag tag in tags)
            {
                if (mKey.Equals(tag.Key))
                {
                    return (mValue.Equals(tag.Value));
                }
            }

            return false;
        }
    }
}
