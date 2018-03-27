using System;

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

	using XMLReaderAdapter = org.oscim.backend.XMLReaderAdapter;
	using Logger = org.slf4j.Logger;
	using LoggerFactory = org.slf4j.LoggerFactory;
	using Attributes = org.xml.sax.Attributes;
	using SAXException = org.xml.sax.SAXException;
	using DefaultHandler = org.xml.sax.helpers.DefaultHandler;

	/// <summary>
	/// A utility class with theme specific helper methods.
	/// </summary>
	public sealed class ThemeUtils
	{

		private static readonly Logger log = LoggerFactory.getLogger(typeof(ThemeUtils));

		/// <summary>
		/// Check if the given theme is a Mapsforge one.
		/// </summary>
		public static bool isMapsforgeTheme(ThemeFile theme)
		{
			try
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.concurrent.atomic.AtomicBoolean isMapsforgeTheme = new java.util.concurrent.atomic.AtomicBoolean(false);
				AtomicBoolean isMapsforgeTheme = new AtomicBoolean(false);
				try
				{
					(new XMLReaderAdapter()).parse(new DefaultHandlerAnonymousInnerClassHelper(isMapsforgeTheme), theme.RenderThemeAsStream);
				}
				catch (SAXTerminationException)
				{
					// Do nothing
				}
				return isMapsforgeTheme.get();
			}
			catch (Exception e)
			{
				log.error(e.Message, e);
				return false;
			}
		}

		private class DefaultHandlerAnonymousInnerClassHelper : DefaultHandler
		{
			private AtomicBoolean isMapsforgeTheme;

			public DefaultHandlerAnonymousInnerClassHelper(AtomicBoolean isMapsforgeTheme)
			{
				this.isMapsforgeTheme = isMapsforgeTheme;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void startElement(String uri, String localName, String qName, org.xml.sax.Attributes attributes) throws org.xml.sax.SAXException
			public virtual void startElement(string uri, string localName, string qName, Attributes attributes)
			{
				if (localName.Equals("rendertheme"))
				{
					isMapsforgeTheme.set(uri.Equals("http://mapsforge.org/renderTheme"));
					// We have all info, break parsing
					throw new SAXTerminationException();
				}
			}
		}

		private ThemeUtils()
		{
		}
	}

}