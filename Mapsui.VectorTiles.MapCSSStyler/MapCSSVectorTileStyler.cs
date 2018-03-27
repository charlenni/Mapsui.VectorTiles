using Mapsui.VectorTiles.MapCSSStyler.v0_2;

namespace Mapsui.VectorTiles.MapCSSStyler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Mapsui.VectorTiles.MapCSSStyler.v0_2.Domain;
    using Styles;

    public class MapCSSVectorTileStyler : IVectorTileStyler
    {
        private HashSet<string> _keysForNodes;
        private HashSet<string> _keysForWays;
        private HashSet<string> _keysForRelations;
        private HashSet<string> _keysForAreas;
        private HashSet<string> _keysForLines;

        private HashSet<TagsCollectionBase> _unsuccesfullWays;
        private Dictionary<TagsCollectionBase, List<MapCSSRuleProperties>> _succesfullWays;

        public MapCSSVectorTileStyler(Stream stream)
        {
            var _mapCSSFile = MapCSSFile.FromStream(stream);

            _keysForNodes = new HashSet<string>();
            _keysForWays = new HashSet<string>();
            _keysForRelations = new HashSet<string>();
            _keysForLines = new HashSet<string>();
            _keysForAreas = new HashSet<string>();

            if (_mapCSSFile != null && _mapCSSFile.Rules != null)
            {
                foreach (var rule in _mapCSSFile.Rules)
                {
                    foreach (var selector in rule.Selectors)
                    {
                        if(selector.SelectorRule == null)
                        { // there is no selector rule, not irrelevant tags, no short-list of relevant tags possible.
                            switch (selector.Type)
                            {
                                case SelectorTypeEnum.Node:
                                    _keysForNodes = null;
                                    break;
                                case SelectorTypeEnum.Way:
                                    _keysForWays = null;
                                    break;
                                case SelectorTypeEnum.Relation:
                                    _keysForRelations = null;
                                    break;
                                case SelectorTypeEnum.Line:
                                    _keysForLines = null;
                                    break;
                                case SelectorTypeEnum.Area:
                                    _keysForAreas = null;
                                    break;
                            }
}
                        else
                        { // there might be relevant tags in this selector rule.
                            switch (selector.Type)
                            {
                                case SelectorTypeEnum.Node:
                                    selector.SelectorRule.AddRelevantKeysTo(_keysForNodes);
                                    break;
                                case SelectorTypeEnum.Way:
                                    selector.SelectorRule.AddRelevantKeysTo(_keysForWays);
                                    break;
                                case SelectorTypeEnum.Relation:
                                    selector.SelectorRule.AddRelevantKeysTo(_keysForRelations);
                                    break;
                                case SelectorTypeEnum.Line:
                                    selector.SelectorRule.AddRelevantKeysTo(_keysForLines);
                                    break;
                                case SelectorTypeEnum.Area:
                                    selector.SelectorRule.AddRelevantKeysTo(_keysForAreas);
                                    break;
                            }
                        }
                    }
                }
            }

            _unsuccesfullWays = new HashSet<TagsCollectionBase>();
            _succesfullWays = new Dictionary<TagsCollectionBase, List<MapCSSRuleProperties>>();
         }
        
        public IStyle GetStyle(TagsCollection tags)
        {
            return null;
        }
    }
}