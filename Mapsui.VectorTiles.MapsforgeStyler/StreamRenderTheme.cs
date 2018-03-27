using System;

/*
 * Copyright 2016-2017 devemux86
 * Copyright 2017 Andrey Novikov
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

	using Utils = org.oscim.utils.Utils;


	/// <summary>
	/// A StreamRenderTheme allows for customizing the rendering style of the map
	/// via an XML input stream.
	/// </summary>
	[Serializable]
	public class StreamRenderTheme : ThemeFile
	{
		private const long serialVersionUID = 1L;

		private readonly System.IO.Stream mInputStream;
		private XmlRenderThemeMenuCallback mMenuCallback;
		private readonly string mRelativePathPrefix;

		/// <param name="relativePathPrefix"> the prefix for all relative resource paths. </param>
		/// <param name="inputStream">        an input stream containing valid render theme XML data. </param>
		public StreamRenderTheme(string relativePathPrefix, System.IO.Stream inputStream) : this(relativePathPrefix, inputStream, null)
		{
		}

		/// <param name="relativePathPrefix"> the prefix for all relative resource paths. </param>
		/// <param name="inputStream">        an input stream containing valid render theme XML data. </param>
		/// <param name="menuCallback">       the interface callback to create a settings menu on the fly. </param>
		public StreamRenderTheme(string relativePathPrefix, System.IO.Stream inputStream, XmlRenderThemeMenuCallback menuCallback)
		{
			mRelativePathPrefix = relativePathPrefix;
			mInputStream = new BufferedInputStream(inputStream);
			mInputStream.mark(0);
			mMenuCallback = menuCallback;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is StreamRenderTheme))
			{
				return false;
			}
			StreamRenderTheme other = (StreamRenderTheme) obj;
			if (mInputStream != other.mInputStream)
			{
				return false;
			}
			if (!Utils.Equals(mRelativePathPrefix, other.mRelativePathPrefix))
			{
				return false;
			}
			return true;
		}

		public virtual XmlRenderThemeMenuCallback MenuCallback
		{
			get
			{
				return mMenuCallback;
			}
			set
			{
				mMenuCallback = value;
			}
		}

		public virtual string RelativePathPrefix
		{
			get
			{
				return mRelativePathPrefix;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public java.io.InputStream getRenderThemeAsStream() throws org.oscim.theme.IRenderTheme_ThemeException
		public virtual System.IO.Stream RenderThemeAsStream
		{
			get
			{
				try
				{
					mInputStream.reset();
				}
				catch (IOException e)
				{
					throw new IRenderTheme_ThemeException(e.Message);
				}
				return mInputStream;
			}
		}

		public virtual bool MapsforgeTheme
		{
			get
			{
				return ThemeUtils.isMapsforgeTheme(this);
			}
		}

	}

}