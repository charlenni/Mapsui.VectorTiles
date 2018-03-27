namespace Mapsui.VectorTiles.MapsforgeStyler.Rules
{
    internal class PositiveRuleV : Rule
    {
        internal readonly string mValue;

        internal PositiveRuleV(int element, int zoom, int selector, string value, Rule[] subRules, RenderStyle[] styles) 
            : base(element, zoom, selector, subRules, styles)
        {
            mValue = value;
        }

        public override bool MatchesTags(Tag[] tags)
        {
            foreach (Tag tag in tags)
            {
                if (mValue.Equals(tag.Value))
                {
                    return true;
                }
            }

            return false;
        }
    }

}
