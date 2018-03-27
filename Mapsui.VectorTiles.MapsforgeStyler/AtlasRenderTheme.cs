using System.Collections.Generic;

/*
 * Copyright 2017 Longri
 * Copyright 2017 devemux86
 *
 * This program is free software: you can redistribute it and/or modify it under the
 * terms of the GNU Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY
 * WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
 * PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with
 * this program. If not, see <http://www.gnu.org/licenses/>.
 */
namespace org.oscim.theme
{

	using TextureAtlas = org.oscim.renderer.atlas.TextureAtlas;
	using TextureRegion = org.oscim.renderer.atlas.TextureRegion;
	using Rule = org.oscim.theme.rule.Rule;


	public class AtlasRenderTheme : RenderTheme
	{

		private readonly IDictionary<object, TextureRegion> textureRegionMap;
		private readonly IList<TextureAtlas> atlasList;

		public AtlasRenderTheme(int mapBackground, float baseTextSize, Rule[] rules, int levels, IDictionary<object, TextureRegion> textureRegionMap, IList<TextureAtlas> atlasList) : this(mapBackground, baseTextSize, rules, levels, false, textureRegionMap, atlasList)
		{
		}

		public AtlasRenderTheme(int mapBackground, float baseTextSize, Rule[] rules, int levels, bool mapsforgeTheme, IDictionary<object, TextureRegion> textureRegionMap, IList<TextureAtlas> atlasList) : base(mapBackground, baseTextSize, rules, levels, mapsforgeTheme)
		{
			this.textureRegionMap = textureRegionMap;
			this.atlasList = atlasList;
		}

		public override void dispose()
		{
			base.dispose();
			foreach (TextureAtlas atlas in atlasList)
			{
				atlas.clear();
				atlas.texture.dispose();
			}
			textureRegionMap.Clear();
		}
	}

}