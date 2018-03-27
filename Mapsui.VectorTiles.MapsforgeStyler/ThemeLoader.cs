/*
 * Copyright 2013 Hannes Janetzek
 * Copyright 2016-2017 devemux86
 * Copyright 2017 Longri
 * Copyright 2017 Andrey Novikov
 *
 * This file is part of the OpenScienceMap project (http://www.opensciencemap.org).
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
	using Parameters = org.oscim.utils.Parameters;

	public class ThemeLoader
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static IRenderTheme load(String renderThemePath) throws org.oscim.theme.IRenderTheme_ThemeException
		public static IRenderTheme load(string renderThemePath)
		{
			return load(new ExternalRenderTheme(renderThemePath));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static IRenderTheme load(String renderThemePath, XmlRenderThemeMenuCallback menuCallback) throws org.oscim.theme.IRenderTheme_ThemeException
		public static IRenderTheme load(string renderThemePath, XmlRenderThemeMenuCallback menuCallback)
		{
			return load(new ExternalRenderTheme(renderThemePath, menuCallback));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static IRenderTheme load(String renderThemePath, ThemeCallback themeCallback) throws org.oscim.theme.IRenderTheme_ThemeException
		public static IRenderTheme load(string renderThemePath, ThemeCallback themeCallback)
		{
			return load(new ExternalRenderTheme(renderThemePath), themeCallback);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static IRenderTheme load(String renderThemePath, XmlRenderThemeMenuCallback menuCallback, ThemeCallback themeCallback) throws org.oscim.theme.IRenderTheme_ThemeException
		public static IRenderTheme load(string renderThemePath, XmlRenderThemeMenuCallback menuCallback, ThemeCallback themeCallback)
		{
			return load(new ExternalRenderTheme(renderThemePath, menuCallback), themeCallback);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static IRenderTheme load(ThemeFile theme) throws org.oscim.theme.IRenderTheme_ThemeException
		public static IRenderTheme load(ThemeFile theme)
		{
			return load(theme, null);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static IRenderTheme load(ThemeFile theme, ThemeCallback themeCallback) throws org.oscim.theme.IRenderTheme_ThemeException
		public static IRenderTheme load(ThemeFile theme, ThemeCallback themeCallback)
		{
			IRenderTheme t;
			if (theme.MapsforgeTheme)
			{
				t = Parameters.TEXTURE_ATLAS ? XmlMapsforgeAtlasThemeBuilder.read(theme, themeCallback) : XmlMapsforgeThemeBuilder.read(theme, themeCallback);
			}
			else
			{
				t = Parameters.TEXTURE_ATLAS ? XmlAtlasThemeBuilder.read(theme, themeCallback) : XmlThemeBuilder.read(theme, themeCallback);
			}
			if (t != null)
			{
				t.scaleTextSize(CanvasAdapter.Scale * CanvasAdapter.textScale);
			}
			return t;
		}
	}

}