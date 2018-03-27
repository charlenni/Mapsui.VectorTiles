/*
 * Copyright 2010, 2011, 2012 mapsforge.org
 * Copyright 2013 Hannes Janetzek
 * Copyright 2016-2017 devemux86
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

	using ThemeException = org.oscim.theme.IRenderTheme.ThemeException;


	/// <summary>
	/// An ExternalRenderTheme allows for customizing the rendering style of the map
	/// via an XML file.
	/// </summary>
	public class ExternalRenderTheme : ThemeFile
	{
		private const long serialVersionUID = 1L;

		private readonly long mFileModificationDate;
		private XmlRenderThemeMenuCallback mMenuCallback;
		private readonly string mPath;

		/// <param name="fileName"> the path to the XML render theme file. </param>
		/// <exception cref="ThemeException"> if the file does not exist or cannot be read. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ExternalRenderTheme(String fileName) throws org.oscim.theme.IRenderTheme.ThemeException
		public ExternalRenderTheme(string fileName) : this(fileName, null)
		{
		}

		/// <param name="fileName">     the path to the XML render theme file. </param>
		/// <param name="menuCallback"> the interface callback to create a settings menu on the fly. </param>
		/// <exception cref="ThemeException"> if the file does not exist or cannot be read. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ExternalRenderTheme(String fileName, XmlRenderThemeMenuCallback menuCallback) throws org.oscim.theme.IRenderTheme.ThemeException
		public ExternalRenderTheme(string fileName, XmlRenderThemeMenuCallback menuCallback)
		{
			File themeFile = new File(fileName);
			if (!themeFile.exists())
			{
				throw new ThemeException("file does not exist: " + themeFile.AbsolutePath);
			}
			else if (!themeFile.File)
			{
				throw new ThemeException("not a file: " + fileName);
			}
			else if (!themeFile.canRead())
			{
				throw new ThemeException("cannot read file: " + fileName);
			}

			mFileModificationDate = themeFile.lastModified();
			if (mFileModificationDate == 0L)
			{
				throw new ThemeException("cannot read last modification time");
			}
			mPath = fileName;
			mMenuCallback = menuCallback;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is ExternalRenderTheme))
			{
				return false;
			}
			ExternalRenderTheme other = (ExternalRenderTheme) obj;
			if (mFileModificationDate != other.mFileModificationDate)
			{
				return false;
			}
			else if (string.ReferenceEquals(mPath, null) && !string.ReferenceEquals(other.mPath, null))
			{
				return false;
			}
			else if (!string.ReferenceEquals(mPath, null) && !mPath.Equals(other.mPath))
			{
				return false;
			}
			return true;
		}

		public override XmlRenderThemeMenuCallback MenuCallback
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

		public override string RelativePathPrefix
		{
			get
			{
				return (new System.IO.DirectoryInfo(mPath)).Parent.FullName;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public java.io.InputStream getRenderThemeAsStream() throws org.oscim.theme.IRenderTheme.ThemeException
		public override System.IO.Stream RenderThemeAsStream
		{
			get
			{
				System.IO.Stream @is;
    
				try
				{
					@is = new System.IO.FileStream(mPath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				}
				catch (FileNotFoundException e)
				{
					throw new ThemeException(e.Message);
				}
				return @is;
			}
		}

		public override bool MapsforgeTheme
		{
			get
			{
				return ThemeUtils.isMapsforgeTheme(this);
			}
		}

	}

}