using System;
using System.Collections.Generic;
using System.Text;

/*
 * Copyright 2017 Longri
 * Copyright 2017-2018 devemux86
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

	using CanvasAdapter = org.oscim.backend.CanvasAdapter;
	using Platform = org.oscim.backend.Platform;
	using XMLReaderAdapter = org.oscim.backend.XMLReaderAdapter;
	using Bitmap = org.oscim.backend.canvas.Bitmap;
	using TextureAtlas = org.oscim.renderer.atlas.TextureAtlas;
	using TextureRegion = org.oscim.renderer.atlas.TextureRegion;
	using ThemeException = org.oscim.theme.IRenderTheme.ThemeException;
	using Rule = org.oscim.theme.rule.Rule;
	using RenderStyle = org.oscim.theme.styles.RenderStyle;
	using SymbolStyle = org.oscim.theme.styles.SymbolStyle;
	using SymbolBuilder = org.oscim.theme.styles.SymbolStyle.SymbolBuilder;
	using TextureAtlasUtils = org.oscim.utils.TextureAtlasUtils;


	public class XmlAtlasThemeBuilder : XmlThemeBuilder
	{

		/// <param name="theme"> an input theme containing valid render theme XML data. </param>
		/// <returns> a new RenderTheme which is created by parsing the XML data from the input theme. </returns>
		/// <exception cref="ThemeException"> if an error occurs while parsing the render theme XML. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static IRenderTheme read(ThemeFile theme) throws org.oscim.theme.IRenderTheme.ThemeException
		public static IRenderTheme read(ThemeFile theme)
		{
			return read(theme, null);
		}

		/// <param name="theme">         an input theme containing valid render theme XML data. </param>
		/// <param name="themeCallback"> the theme callback. </param>
		/// <returns> a new RenderTheme which is created by parsing the XML data from the input theme. </returns>
		/// <exception cref="ThemeException"> if an error occurs while parsing the render theme XML. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static IRenderTheme read(ThemeFile theme, ThemeCallback themeCallback) throws org.oscim.theme.IRenderTheme.ThemeException
		public static IRenderTheme read(ThemeFile theme, ThemeCallback themeCallback)
		{
			IDictionary<object, TextureRegion> outputMap = new Dictionary<object, TextureRegion>();
			IList<TextureAtlas> atlasList = new List<TextureAtlas>();
			XmlAtlasThemeBuilder renderThemeHandler = new XmlAtlasThemeBuilder(theme, themeCallback, outputMap, atlasList);

			try
			{
				(new XMLReaderAdapter()).parse(renderThemeHandler, theme.RenderThemeAsStream);
			}
			catch (Exception e)
			{
				throw new ThemeException(e.Message);
			}

			TextureAtlasUtils.createTextureRegions(renderThemeHandler.bitmapMap, outputMap, atlasList, true, CanvasAdapter.platform == Platform.IOS);

			return replaceThemeSymbols(renderThemeHandler.mRenderTheme, outputMap);
		}

		private static IRenderTheme replaceThemeSymbols(RenderTheme renderTheme, IDictionary<object, TextureRegion> regionMap)
		{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: org.oscim.theme.styles.SymbolStyle.SymbolBuilder<?> symbolBuilder = org.oscim.theme.styles.SymbolStyle.builder();
			SymbolStyle.SymbolBuilder<object> symbolBuilder = SymbolStyle.builder();
			foreach (Rule rule in renderTheme.Rules)
			{
				replaceRuleSymbols(rule, regionMap, symbolBuilder);
			}
			return renderTheme;
		}

		private static void replaceRuleSymbols<T1>(Rule rule, IDictionary<object, TextureRegion> regionMap, SymbolStyle.SymbolBuilder<T1> b)
		{
			for (int i = 0, n = rule.styles.length; i < n; i++)
			{
				RenderStyle style = rule.styles[i];
				if (style is SymbolStyle)
				{
					SymbolStyle symbol = (SymbolStyle) style;
					TextureRegion region = regionMap[symbol.hash];
					if (region != null)
					{
						rule.styles[i] = b.set(symbol).bitmap(null).texture(region).build();
					}
				}
			}
			foreach (Rule subRule in rule.subRules)
			{
				replaceRuleSymbols(subRule, regionMap, b);
			}
		}

		private readonly IDictionary<object, TextureRegion> regionMap;
		private readonly IList<TextureAtlas> atlasList;

		private readonly IDictionary<object, Bitmap> bitmapMap = new Dictionary<object, Bitmap>();

		public XmlAtlasThemeBuilder(ThemeFile theme, IDictionary<object, TextureRegion> regionMap, IList<TextureAtlas> atlasList) : this(theme, null, regionMap, atlasList)
		{
		}

		public XmlAtlasThemeBuilder(ThemeFile theme, ThemeCallback themeCallback, IDictionary<object, TextureRegion> regionMap, IList<TextureAtlas> atlasList) : base(theme, themeCallback)
		{
			this.regionMap = regionMap;
			this.atlasList = atlasList;
		}

		internal override RenderTheme createTheme(Rule[] rules)
		{
			return new AtlasRenderTheme(mMapBackground, mTextScale, rules, mLevels, regionMap, atlasList);
		}

		internal override SymbolStyle buildSymbol<T1>(SymbolStyle.SymbolBuilder<T1> b, string src, Bitmap bitmap)
		{
			// we need to hash with the width/height included as the same symbol could be required
			// in a different size and must be cached with a size-specific hash
			string absoluteName = CanvasAdapter.getAbsoluteFile(mTheme.RelativePathPrefix, src).AbsolutePath;
			int hash = (new StringBuilder()).Append(absoluteName).Append(b.symbolWidth).Append(b.symbolHeight).Append(b.symbolPercent).ToString().GetHashCode();
			bitmapMap[hash] = bitmap;
			return b.hash(hash).build();
		}
	}

}