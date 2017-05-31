/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014-2015 Ludwig M Brinckmann
 * Copyright 2014, 2015 devemux86
 * Copyright 2015 lincomatic
 * Copyright 2016 bvgastel
 * Copyright 2016 Michael Oed
 * Copyright 2017 Dirk Weltz
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
namespace Mapsui.VectorTiles.Mapsforge.Reader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Geometries;
    using Logging;
    using Header;
    using Utils;
    using Datastore;
    using VectorTiles;

    /// <summary>
    /// A class for reading binary map files.
    /// <para>
    /// The readMapData method is now thread safe, but care should be taken that not too much data is
    /// read at the same time (keep simultaneous requests to minimum)
    /// 
    /// </para>
    /// </summary>
    /// <seealso cref= <a href="https://github.com/mapsforge/mapsforge/blob/master/docs/Specification-Binary-Map-File.md">Specification</a> </seealso>
    public class MapFile
	{
        private object sync = new object();
		/// <summary>
		/// Bitmask to extract the block offset from an index entry.
		/// </summary>
		private const long BITMASK_INDEX_OFFSET = 0x7FFFFFFFFFL;

		/// <summary>
		/// Bitmask to extract the water information from an index entry.
		/// </summary>
		private const long BITMASK_INDEX_WATER = 0x8000000000L;

		/// <summary>
		/// Default start zoom level.
		/// </summary>
		private static readonly sbyte? DEFAULT_START_ZOOM_LEVEL = 12;

		/// <summary>
		/// Amount of cache blocks that the index cache should store.
		/// </summary>
		private const int INDEX_CACHE_SIZE = 64;

		/// <summary>
		/// Error message for an invalid first way offset.
		/// </summary>
		private const string INVALID_FIRST_WAY_OFFSET = "invalid first way offset: ";

		/// <summary>
		/// Bitmask for the optional POI feature "elevation".
		/// </summary>
		private const int POI_FEATURE_ELEVATION = 0x20;

		/// <summary>
		/// Bitmask for the optional POI feature "house number".
		/// </summary>
		private const int POI_FEATURE_HOUSE_NUMBER = 0x40;

		/// <summary>
		/// Bitmask for the optional POI feature "name".
		/// </summary>
		private const int POI_FEATURE_NAME = 0x80;

		/// <summary>
		/// Bitmask for the POI layer.
		/// </summary>
		private const int POI_LAYER_BITMASK = 0xf0;

		/// <summary>
		/// Bit shift for calculating the POI layer.
		/// </summary>
		private const int POI_LAYER_SHIFT = 4;

		/// <summary>
		/// Bitmask for the number of POI tags.
		/// </summary>
		private const int POI_NUMBER_OF_TAGS_BITMASK = 0x0f;

        /// <summary>
        /// Read only access mode.
        /// </summary>
        private const string READ_ONLY_MODE = "r";

		/// <summary>
		/// Length of the debug signature at the beginning of each block.
		/// </summary>
		private const sbyte SIGNATURE_LENGTH_BLOCK = 32;

		/// <summary>
		/// Length of the debug signature at the beginning of each POI.
		/// </summary>
		private const sbyte SIGNATURE_LENGTH_POI = 32;

		/// <summary>
		/// Length of the debug signature at the beginning of each way.
		/// </summary>
		private const sbyte SIGNATURE_LENGTH_WAY = 32;

		/// <summary>
		/// The key of the elevation OpenStreetMap tag.
		/// </summary>
		private const string TAG_KEY_ELE = "ele";

		/// <summary>
		/// The key of the house number OpenStreetMap tag.
		/// </summary>
		private const string TAG_KEY_HOUSE_NUMBER = "addr:housenumber";

		/// <summary>
		/// The key of the name OpenStreetMap tag.
		/// </summary>
		private const string TAG_KEY_NAME = "name";

		/// <summary>
		/// The key of the reference OpenStreetMap tag.
		/// </summary>
		private const string TAG_KEY_REF = "ref";

		/// <summary>
		/// Bitmask for the optional way data blocks byte.
		/// </summary>
		private const int WAY_FEATURE_DATA_BLOCKS_BYTE = 0x08;

		/// <summary>
		/// Bitmask for the optional way double delta encoding.
		/// </summary>
		private const int WAY_FEATURE_DOUBLE_DELTA_ENCODING = 0x04;

		/// <summary>
		/// Bitmask for the optional way feature "house number".
		/// </summary>
		private const int WAY_FEATURE_HOUSE_NUMBER = 0x40;

		/// <summary>
		/// Bitmask for the optional way feature "label position".
		/// </summary>
		private const int WAY_FEATURE_LABEL_POSITION = 0x10;

		/// <summary>
		/// Bitmask for the optional way feature "name".
		/// </summary>
		private const int WAY_FEATURE_NAME = 0x80;

		/// <summary>
		/// Bitmask for the optional way feature "reference".
		/// </summary>
		private const int WAY_FEATURE_REF = 0x20;

		/// <summary>
		/// Bitmask for the way layer.
		/// </summary>
		private const int WAY_LAYER_BITMASK = 0xf0;

		/// <summary>
		/// Bit shift for calculating the way layer.
		/// </summary>
		private const int WAY_LAYER_SHIFT = 4;

		/// <summary>
		/// Bitmask for the number of way tags.
		/// </summary>
		private const int WAY_NUMBER_OF_TAGS_BITMASK = 0x0f;

		/* Only for testing, an empty file. */
		public static readonly MapFile TEST_MAP_FILE = new MapFile();
		/// <summary>
		/// Way filtering reduces the number of ways returned to only those that are
		/// relevant for the tile requested, leading to performance gains, but can
		/// cause line clipping artifacts (particularly at higher zoom levels). The
		/// risk of clipping can be reduced by either turning way filtering off or by
		/// increasing the wayFilterDistance which governs how large an area surrounding
		/// the requested tile will be returned.
		/// For most use cases the standard settings should be sufficient.
		/// </summary>
		public static bool wayFilterEnabled = true;
		public static int wayFilterDistance = 20;
		private readonly IndexCache databaseIndexCache;
		private readonly long fileSize;
		private readonly Stream inputStream;

        private readonly MapFileHeader mapFileHeader;

		private readonly DateTime timestamp;

        private sbyte zoomLevelMin = 0;
        private sbyte zoomLevelMax = sbyte.MaxValue;

		private MapFile()
		{
			// only to create a dummy empty file.
			databaseIndexCache = null;
			fileSize = 0;
			inputStream = null;
			mapFileHeader = null;
			timestamp = DateTime.Now;
		}

		/// <summary>
		/// Opens the given map file, reads its header data and validates them. Uses default language.
		/// </summary>
		/// <param name="mapFile"> the map file. </param>
		/// <exception cref="MapFileException"> if the given map file is null or invalid. </exception>
		public MapFile(Stream mapFile) : this(mapFile, null)
		{
		}

		/// <summary>
		/// Opens the given map file, reads its header data and validates them.
		/// </summary>
		/// <param name="stream"> the map file. </param>
		/// <param name="language"> the language to use (may be null). </param>
		/// <exception cref="MapFileException"> if the given map file is null or invalid. </exception>
		public MapFile(Stream stream, string language)
		{
            this.preferredLanguage = language;

			if (stream == null)
			{
				throw new MapFileException("map file must not be null");
			}

            this.inputStream = stream;

            this.fileSize = this.inputStream.Length;
            ReadBuffer readBuffer = new ReadBuffer(this.inputStream);
            this.mapFileHeader = new MapFileHeader();
            this.mapFileHeader.ReadHeader(readBuffer, this.fileSize);
            this.databaseIndexCache = new IndexCache(this.inputStream, INDEX_CACHE_SIZE);

            // TODO: Find the creation date of file
            this.timestamp = DateTime.Now;  // mapFile.lastModified();
        }

        /// <summary>
        /// Extracts substring of preferred language from multilingual string.<br/>
        /// Example multilingual string: "Base\ren\bEnglish\rjp\bJapan\rzh_py\bPin-yin".
        /// <para>
        /// Use '\r' delimiter among names and '\b' delimiter between each language and name.
        /// </para>
        /// </summary>
        public static string Extract(string s, string language)
        {
            if (string.ReferenceEquals(s, null) || s.Trim().Length == 0)
            {
                return null;
            }

            string[] langNames = s.Split(new char[] { '\r' }, StringSplitOptions.None);
            if (string.ReferenceEquals(language, null) || language.Trim().Length == 0)
            {
                return langNames[0];
            }

            string fallback = null;
            for (int i = 1; i < langNames.Length; i++)
            {
                string[] langName = langNames[i].Split(new char[] { '\b' }, StringSplitOptions.None);
                if (langName.Length != 2)
                {
                    continue;
                }

                // Perfect match
                if (langName[0].Equals(language, StringComparison.CurrentCultureIgnoreCase))
                {
                    return langName[1];
                }

                // Fall back to base, e.g. zh-min-lan -> zh
                if (string.ReferenceEquals(fallback, null) && !langName[0].Contains("-") && (language.Contains("-") || language.Contains("_")) && language.ToLower().StartsWith(langName[0].ToLower(), StringComparison.Ordinal))
                {
                    fallback = langName[1];
                }
            }
            return (!string.ReferenceEquals(fallback, null)) ? fallback : langNames[0];
        }

        /// <summary>
        /// the preferred language when extracting labels from this data store. The actual
        /// implementation is up to the concrete implementation, which can also simply ignore
        /// this setting.
        /// </summary>
        protected internal string preferredLanguage;

        /// <summary>
        /// Extracts substring of preferred language from multilingual string using
        /// the preferredLanguage setting.
        /// </summary>
        protected internal virtual string ExtractLocalized(string s)
        {
            return Extract(s, preferredLanguage);
        }

        public BoundingBox BoundingBox
		{
            get
            {
                return MapFileInfo.BoundingBox;
            }
            set
            {
            }
		}

		public void Close()
		{
			CloseFile();
		}

		/// <summary>
		/// Closes the map file and destroys all internal caches. Has no effect if no map file is currently opened.
		/// </summary>
		private void CloseFile()
		{
			try
			{
				this.databaseIndexCache.Destroy();
                if (this.inputStream != null)
                {
                    this.inputStream.Dispose();
                }
			}
			catch (Exception e)
			{
				Logger.Log(LogLevel.Error, e.Message, e);
			}
		}

		private void decodeWayNodesDoubleDelta(List<Point> waySegment, double tileLatitude, double tileLongitude, ReadBuffer readBuffer)
		{
			// get the first way node latitude offset (VBE-S)
			double wayNodeLatitude = tileLatitude + PointUtils.MicrodegreesToDegrees(readBuffer.ReadSignedInt());

			// get the first way node longitude offset (VBE-S)
			double wayNodeLongitude = tileLongitude + PointUtils.MicrodegreesToDegrees(readBuffer.ReadSignedInt());

			// store the first way node
			waySegment.Add(new Point(wayNodeLongitude, wayNodeLatitude));

			double previousSingleDeltaLatitude = 0;
			double previousSingleDeltaLongitude = 0;

			for (int wayNodesIndex = 1; wayNodesIndex < waySegment.Capacity; ++wayNodesIndex)
			{
				// get the way node latitude double-delta offset (VBE-S)
				double doubleDeltaLatitude = PointUtils.MicrodegreesToDegrees(readBuffer.ReadSignedInt());

				// get the way node longitude double-delta offset (VBE-S)
				double doubleDeltaLongitude = PointUtils.MicrodegreesToDegrees(readBuffer.ReadSignedInt());

				double singleDeltaLatitude = doubleDeltaLatitude + previousSingleDeltaLatitude;
				double singleDeltaLongitude = doubleDeltaLongitude + previousSingleDeltaLongitude;

				wayNodeLatitude = wayNodeLatitude + singleDeltaLatitude;
				wayNodeLongitude = wayNodeLongitude + singleDeltaLongitude;

                // Decoding near international date line can return values slightly outside valid [-180°, 180°] due to calculation precision
                if (wayNodeLongitude < PointUtils.LONGITUDE_MIN
                     && (PointUtils.LONGITUDE_MIN - wayNodeLongitude) < 0.001)
                {
                    wayNodeLongitude = PointUtils.LONGITUDE_MIN;
                }
                else if (wayNodeLongitude > PointUtils.LONGITUDE_MAX
                     && (wayNodeLongitude - PointUtils.LONGITUDE_MAX) < 0.001)
                {
                    wayNodeLongitude = PointUtils.LONGITUDE_MAX;
                }

                waySegment.Add(new Point(wayNodeLongitude, wayNodeLatitude));

				previousSingleDeltaLatitude = singleDeltaLatitude;
				previousSingleDeltaLongitude = singleDeltaLongitude;
			}
		}

		private void decodeWayNodesSingleDelta(List<Point> waySegment, double tileLatitude, double tileLongitude, ReadBuffer readBuffer)
		{
			// get the first way node latitude single-delta offset (VBE-S)
			double wayNodeLatitude = tileLatitude + PointUtils.MicrodegreesToDegrees(readBuffer.ReadSignedInt());

			// get the first way node longitude single-delta offset (VBE-S)
			double wayNodeLongitude = tileLongitude + PointUtils.MicrodegreesToDegrees(readBuffer.ReadSignedInt());

			// store the first way node
			waySegment.Add(new Point(wayNodeLongitude, wayNodeLatitude));

			for (int wayNodesIndex = 1; wayNodesIndex < waySegment.Capacity; ++wayNodesIndex)
			{
				// get the way node latitude offset (VBE-S)
				wayNodeLatitude = wayNodeLatitude + PointUtils.MicrodegreesToDegrees(readBuffer.ReadSignedInt());

				// get the way node longitude offset (VBE-S)
				wayNodeLongitude = wayNodeLongitude + PointUtils.MicrodegreesToDegrees(readBuffer.ReadSignedInt());

                // Decoding near international date line can return values slightly outside valid [-180°, 180°] due to calculation precision
                if (wayNodeLongitude < PointUtils.LONGITUDE_MIN
                     && (PointUtils.LONGITUDE_MIN - wayNodeLongitude) < 0.001)
                {
                    wayNodeLongitude = PointUtils.LONGITUDE_MIN;
                }
                else if (wayNodeLongitude > PointUtils.LONGITUDE_MAX
                     && (wayNodeLongitude - PointUtils.LONGITUDE_MAX) < 0.001)
                {
                    wayNodeLongitude = PointUtils.LONGITUDE_MAX;
                }

                waySegment.Add(new Point(wayNodeLongitude, wayNodeLatitude));
			}
		}

		/// <summary>
		/// Returns the creation timestamp of the map file. </summary>
		/// <param name="tile"> not used, as all tiles will shared the same creation date. </param>
		/// <returns> the creation timestamp inside the map file. </returns>
		public DateTime GetDataTimestamp(Tile tile)
		{
			return this.timestamp;
		}

		/// <returns> the metadata for the current map file. </returns>
		/// <exception cref="IllegalStateException">
		///             if no map is currently opened. </exception>
		public virtual MapFileInfo MapFileInfo
		{
			get
			{
				return this.mapFileHeader.MapFileInfo;
			}
		}

		/// <returns> the map file supported languages (may be null). </returns>
		public virtual string[] MapLanguages
		{
			get
			{
				string languagesPreference = MapFileInfo.LanguagesPreference;
				if (!string.ReferenceEquals(languagesPreference, null) && languagesPreference.Trim().Length > 0)
				{
					return languagesPreference.Split(new char[] { ',' }, StringSplitOptions.None);
				}
				return null;
			}
		}

		private PoiWayBundle ProcessBlock(QueryParameters queryParameters, SubFileParameter subFileParameter, BoundingBox boundingBox, double tileLatitude, double tileLongitude, ReadBuffer readBuffer)
		{
			if (!ProcessBlockSignature(readBuffer))
			{
				return null;
			}

			int[][] zoomTable = ReadZoomTable(subFileParameter, readBuffer);
			int zoomTableRow = queryParameters.queryZoomLevel - subFileParameter.ZoomLevelMin;
			int poisOnQueryZoomLevel = zoomTable[zoomTableRow][0];
			int waysOnQueryZoomLevel = zoomTable[zoomTableRow][1];

			// get the relative offset to the first stored way in the block
			int firstWayOffset = readBuffer.ReadUnsignedInt();
			if (firstWayOffset < 0)
			{
				Logger.Log(LogLevel.Warning, INVALID_FIRST_WAY_OFFSET + firstWayOffset);
				return null;
			}

			// add the current buffer position to the relative first way offset
			firstWayOffset += readBuffer.BufferPosition;
			if (firstWayOffset > readBuffer.BufferSize)
			{
				Logger.Log(LogLevel.Warning, INVALID_FIRST_WAY_OFFSET + firstWayOffset);
				return null;
			}

			bool filterRequired = queryParameters.queryZoomLevel > subFileParameter.BaseZoomLevel;

			IList<PointOfInterest> pois = ProcessPOIs(tileLatitude, tileLongitude, poisOnQueryZoomLevel, boundingBox, filterRequired, readBuffer);
			if (pois == null)
			{
				return null;
			}

			// finished reading POIs, check if the current buffer position is valid
			if (readBuffer.BufferPosition > firstWayOffset)
			{
				Logger.Log(LogLevel.Warning, "invalid buffer position: " + readBuffer.BufferPosition);
				return null;
			}

			// move the pointer to the first way
			readBuffer.BufferPosition = firstWayOffset;

			IList<Way> ways = ProcessWays(queryParameters, waysOnQueryZoomLevel, boundingBox, filterRequired, tileLatitude, tileLongitude, readBuffer);
			if (ways == null)
			{
				return null;
			}

			return new PoiWayBundle(pois, ways);
		}

		private MapReadResult ProcessBlocks(QueryParameters queryParameters, SubFileParameter subFileParameter, BoundingBox boundingBox)
		{
			bool queryIsWater = true;
			bool queryReadWaterInfo = false;

			MapReadResult mapFileReadResult = new MapReadResult();

			// read and process all blocks from top to bottom and from left to right
			for (long row = queryParameters.fromBlockY; row <= queryParameters.toBlockY; ++row)
			{
				for (long column = queryParameters.fromBlockX; column <= queryParameters.toBlockX; ++column)
				{
					// calculate the actual block number of the needed block in the file
					long blockNumber = row * subFileParameter.BlocksWidth + column;

					// get the current index entry
					long currentBlockIndexEntry = this.databaseIndexCache.GetIndexEntry(subFileParameter, blockNumber);

					// check if the current query would still return a water tile
					if (queryIsWater)
					{
						// check the water flag of the current block in its index entry
						queryIsWater &= (currentBlockIndexEntry & BITMASK_INDEX_WATER) != 0;
						queryReadWaterInfo = true;
					}

					// get and check the current block pointer
					long currentBlockPointer = currentBlockIndexEntry & BITMASK_INDEX_OFFSET;
					if (currentBlockPointer < 1 || currentBlockPointer > subFileParameter.SubFileSize)
					{
						Logger.Log(LogLevel.Warning, "invalid current block pointer: " + currentBlockPointer);
						Logger.Log(LogLevel.Warning, "subFileSize: " + subFileParameter.SubFileSize);
						return null;
					}

					long nextBlockPointer;
					// check if the current block is the last block in the file
					if (blockNumber + 1 == subFileParameter.NumberOfBlocks)
					{
						// set the next block pointer to the end of the file
						nextBlockPointer = subFileParameter.SubFileSize;
					}
					else
					{
						// get and check the next block pointer
						nextBlockPointer = this.databaseIndexCache.GetIndexEntry(subFileParameter, blockNumber + 1) & BITMASK_INDEX_OFFSET;
						if (nextBlockPointer > subFileParameter.SubFileSize)
						{
							Logger.Log(LogLevel.Warning, "invalid next block pointer: " + nextBlockPointer);
							Logger.Log(LogLevel.Warning, "sub-file size: " + subFileParameter.SubFileSize);
							return null;
						}
					}

					// calculate the size of the current block
					int currentBlockSize = (int)(nextBlockPointer - currentBlockPointer);
					if (currentBlockSize < 0)
					{
						Logger.Log(LogLevel.Warning, "current block size must not be negative: " + currentBlockSize);
						return null;
					}
					else if (currentBlockSize == 0)
					{
						// the current block is empty, continue with the next block
						continue;
					}
					else if (currentBlockSize > ReadBuffer.MaximumBufferSize)
					{
						// the current block is too large, continue with the next block
						Logger.Log(LogLevel.Warning, "current block size too large: " + currentBlockSize);
						continue;
					}
					else if (currentBlockPointer + currentBlockSize > this.fileSize)
					{
						Logger.Log(LogLevel.Warning, "current block largher than file size: " + currentBlockSize);
						return null;
					}

                    // read the current block into the buffer
                    ReadBuffer readBuffer = new ReadBuffer(inputStream);
					if (!readBuffer.ReadFromStream(subFileParameter.StartAddress + currentBlockPointer, currentBlockSize))
					{
						// skip the current block
						Logger.Log(LogLevel.Warning, "reading current block has failed: " + currentBlockSize);
						return null;
					}

					// calculate the top-left coordinates of the underlying tile
					double tileLatitude = MercatorProjection.TileYToLatitude(subFileParameter.BoundaryTileTop + row, subFileParameter.BaseZoomLevel);
					double tileLongitude = MercatorProjection.TileXToLongitude(subFileParameter.BoundaryTileLeft + column, subFileParameter.BaseZoomLevel);

					try
					{
						PoiWayBundle poiWayBundle = ProcessBlock(queryParameters, subFileParameter, boundingBox, tileLatitude, tileLongitude, readBuffer);
						if (poiWayBundle != null)
						{
							mapFileReadResult.Add(poiWayBundle);
						}
					}
					catch (System.IndexOutOfRangeException e)
					{
						Logger.Log(LogLevel.Error, e.Message, e);
					}
				}
			}

			// the query is finished, was the water flag set for all blocks?
			if (queryIsWater && queryReadWaterInfo)
			{
                // Deprecate water tiles rendering
                // mapFileReadResult.IsWater = true;
			}

			return mapFileReadResult;
		}

		/// <summary>
		/// Processes the block signature, if present.
		/// </summary>
		/// <returns> true if the block signature could be processed successfully, false otherwise. </returns>
		private bool ProcessBlockSignature(ReadBuffer readBuffer)
		{
			if (this.mapFileHeader.MapFileInfo.DebugFile)
			{
				// get and check the block signature
				string signatureBlock = readBuffer.ReadUTF8EncodedString(SIGNATURE_LENGTH_BLOCK);
				if (!signatureBlock.StartsWith("###TileStart", StringComparison.Ordinal))
				{
					Logger.Log(LogLevel.Warning, "invalid block signature: " + signatureBlock);
					return false;
				}
			}
			return true;
		}

		private IList<PointOfInterest> ProcessPOIs(double tileLatitude, double tileLongitude, int numberOfPois, BoundingBox boundingBox, bool filterRequired, ReadBuffer readBuffer)
		{
			IList<PointOfInterest> pois = new List<PointOfInterest>();
			Tag[] poiTags = this.mapFileHeader.MapFileInfo.PoiTags;

			for (int elementCounter = numberOfPois; elementCounter != 0; --elementCounter)
			{
				if (this.mapFileHeader.MapFileInfo.DebugFile)
				{
					// get and check the POI signature
					string signaturePoi = readBuffer.ReadUTF8EncodedString(SIGNATURE_LENGTH_POI);
					if (!signaturePoi.StartsWith("***POIStart", StringComparison.Ordinal))
					{
						Logger.Log(LogLevel.Warning, "invalid POI signature: " + signaturePoi);
						return null;
					}
				}

				// get the POI latitude offset (VBE-S)
				double latitude = tileLatitude + PointUtils.MicrodegreesToDegrees(readBuffer.ReadSignedInt());

				// get the POI longitude offset (VBE-S)
				double longitude = tileLongitude + PointUtils.MicrodegreesToDegrees(readBuffer.ReadSignedInt());

				// get the special byte which encodes multiple flags
				sbyte specialByte = readBuffer.ReadByte();

				// bit 1-4 represent the layer with
				sbyte layer = (sbyte)((int)((uint)(specialByte & POI_LAYER_BITMASK) >> POI_LAYER_SHIFT));
				// bit 5-8 represent the number of tag IDs
				sbyte numberOfTags = (sbyte)(specialByte & POI_NUMBER_OF_TAGS_BITMASK);

				IList<Tag> tags = new List<Tag>();

				// get the tag IDs (VBE-U)
				for (sbyte tagIndex = numberOfTags; tagIndex != 0; --tagIndex)
				{
					int tagId = readBuffer.ReadUnsignedInt();
					if (tagId < 0 || tagId >= poiTags.Length)
					{
						Logger.Log(LogLevel.Warning, "invalid POI tag ID: " + tagId);
						return null;
					}
					tags.Add(poiTags[tagId]);
				}

				// get the feature bitmask (1 byte)
				sbyte featureByte = readBuffer.ReadByte();

				// bit 1-3 enable optional features
				bool featureName = (featureByte & POI_FEATURE_NAME) != 0;
				bool featureHouseNumber = (featureByte & POI_FEATURE_HOUSE_NUMBER) != 0;
				bool featureElevation = (featureByte & POI_FEATURE_ELEVATION) != 0;

				// check if the POI has a name
				if (featureName)
				{
					tags.Add(new Tag(TAG_KEY_NAME, ExtractLocalized(readBuffer.ReadUTF8EncodedString())));
				}

				// check if the POI has a house number
				if (featureHouseNumber)
				{
					tags.Add(new Tag(TAG_KEY_HOUSE_NUMBER, readBuffer.ReadUTF8EncodedString()));
				}

				// check if the POI has an elevation
				if (featureElevation)
				{
					tags.Add(new Tag(TAG_KEY_ELE, Convert.ToString(readBuffer.ReadSignedInt())));
				}

				Point position = new Point(longitude, latitude);
				// depending on the zoom level configuration the poi can lie outside
				// the tile requested, we filter them out here
				if (!filterRequired || boundingBox.Contains(position))
				{
					pois.Add(new PointOfInterest(layer, tags, position));
				}
			}

			return pois;
		}

		private List<List<Point>> ProcessWayDataBlock(double tileLatitude, double tileLongitude, bool doubleDeltaEncoding, ReadBuffer readBuffer)
		{
			// get and check the number of way coordinate blocks (VBE-U)
			int numberOfWayCoordinateBlocks = readBuffer.ReadUnsignedInt();
			if (numberOfWayCoordinateBlocks < 1 || numberOfWayCoordinateBlocks > short.MaxValue)
			{
				Logger.Log(LogLevel.Warning, "invalid number of way coordinate blocks: " + numberOfWayCoordinateBlocks);
				return null;
			}

			// create the array which will store the different way coordinate blocks
			List<List<Point>> wayCoordinates = new List<List<Point>>(numberOfWayCoordinateBlocks);

			// read the way coordinate blocks
			for (int coordinateBlock = 0; coordinateBlock < numberOfWayCoordinateBlocks; ++coordinateBlock)
			{
				// get and check the number of way nodes (VBE-U)
				int numberOfWayNodes = readBuffer.ReadUnsignedInt();
				if (numberOfWayNodes < 2 || numberOfWayNodes > short.MaxValue)
				{
					Logger.Log(LogLevel.Warning, "invalid number of way nodes: " + numberOfWayNodes);
					// returning null here will actually leave the tile blank as the
					// position on the ReadBuffer will not be advanced correctly. However,
					// it will not crash the app.
					return null;
				}

				// create the array which will store the current way segment
				List<Point> waySegment = new List<Point>(numberOfWayNodes);

				if (doubleDeltaEncoding)
				{
					decodeWayNodesDoubleDelta(waySegment, tileLatitude, tileLongitude, readBuffer);
				}
				else
				{
					decodeWayNodesSingleDelta(waySegment, tileLatitude, tileLongitude, readBuffer);
				}

				wayCoordinates.Add(waySegment);
			}

			return wayCoordinates;
		}

		private IList<Way> ProcessWays(QueryParameters queryParameters, int numberOfWays, BoundingBox boundingBox, bool filterRequired, double tileLatitude, double tileLongitude, ReadBuffer readBuffer)
		{
			IList<Way> ways = new List<Way>();
			Tag[] wayTags = this.mapFileHeader.MapFileInfo.WayTags;

            // Extend BoundingBox about wayFilterDistance meters, so that ways crossing the border added
            double verticalExpansion = PointUtils.LatitudeDistance(wayFilterDistance);
            double horizontalExpansion = PointUtils.LongitudeDistance(wayFilterDistance, Math.Max(Math.Abs(boundingBox.MinY), Math.Abs(boundingBox.MaxY)));

            double minLat = Math.Max(MercatorProjection.LATITUDE_MIN, boundingBox.MinY - verticalExpansion);
            double minLon = Math.Max(-180, boundingBox.MinX - horizontalExpansion);
            double maxLat = Math.Min(MercatorProjection.LATITUDE_MAX, boundingBox.MaxY + verticalExpansion);
            double maxLon = Math.Min(180, boundingBox.MaxX + horizontalExpansion);

            BoundingBox wayFilterBbox = new BoundingBox(minLon, minLat, maxLon, maxLat);

			for (int elementCounter = numberOfWays; elementCounter != 0; --elementCounter)
			{
				if (this.mapFileHeader.MapFileInfo.DebugFile)
				{
					// get and check the way signature
					string signatureWay = readBuffer.ReadUTF8EncodedString(SIGNATURE_LENGTH_WAY);
					if (!signatureWay.StartsWith("---WayStart", StringComparison.Ordinal))
					{
						Logger.Log(LogLevel.Warning, "invalid way signature: " + signatureWay);
						return null;
					}
				}

				// get the size of the way (VBE-U)
				int wayDataSize = readBuffer.ReadUnsignedInt();
				if (wayDataSize < 0)
				{
                    Logger.Log(LogLevel.Warning, "invalid way data size: " + wayDataSize);
					return null;
				}

				if (queryParameters.useTileBitmask)
				{
					// get the way tile bitmask (2 bytes)
					int tileBitmask = readBuffer.ReadShort();
					// check if the way is inside the requested tile
					if ((queryParameters.queryTileBitmask & tileBitmask) == 0)
					{
						// skip the rest of the way and continue with the next way
						readBuffer.SkipBytes(wayDataSize - 2);
						continue;
					}
				}
				else
				{
					// ignore the way tile bitmask (2 bytes)
					readBuffer.SkipBytes(2);
				}

				// get the special byte which encodes multiple flags
				sbyte specialByte = readBuffer.ReadByte();

                // bit 1-4 represent the layer
                sbyte layer = (sbyte)((int)((uint)(specialByte & WAY_LAYER_BITMASK) >> WAY_LAYER_SHIFT));
				// bit 5-8 represent the number of tag IDs
				sbyte numberOfTags = (sbyte)(specialByte & WAY_NUMBER_OF_TAGS_BITMASK);

				IList<Tag> tags = new List<Tag>();

				for (sbyte tagIndex = numberOfTags; tagIndex != 0; --tagIndex)
				{
					int tagId = readBuffer.ReadUnsignedInt();
					if (tagId < 0 || tagId >= wayTags.Length)
					{
                        Logger.Log(LogLevel.Warning, "invalid way tag ID: " + tagId);
						return null;
					}
					tags.Add(wayTags[tagId]);
				}

				// get the feature bitmask (1 byte)
				sbyte featureByte = readBuffer.ReadByte();

				// bit 1-6 enable optional features
				bool featureName = (featureByte & WAY_FEATURE_NAME) != 0;
				bool featureHouseNumber = (featureByte & WAY_FEATURE_HOUSE_NUMBER) != 0;
				bool featureRef = (featureByte & WAY_FEATURE_REF) != 0;
				bool featureLabelPosition = (featureByte & WAY_FEATURE_LABEL_POSITION) != 0;
				bool featureWayDataBlocksByte = (featureByte & WAY_FEATURE_DATA_BLOCKS_BYTE) != 0;
				bool featureWayDoubleDeltaEncoding = (featureByte & WAY_FEATURE_DOUBLE_DELTA_ENCODING) != 0;

				// check if the way has a name
				if (featureName)
				{
					tags.Add(new Tag(TAG_KEY_NAME, ExtractLocalized(readBuffer.ReadUTF8EncodedString())));
				}

				// check if the way has a house number
				if (featureHouseNumber)
				{
					tags.Add(new Tag(TAG_KEY_HOUSE_NUMBER, readBuffer.ReadUTF8EncodedString()));
				}

				// check if the way has a reference
				if (featureRef)
				{
					tags.Add(new Tag(TAG_KEY_REF, readBuffer.ReadUTF8EncodedString()));
				}

				Point labelPosition = ReadOptionalLabelPosition(tileLatitude, tileLongitude, featureLabelPosition, readBuffer);

				int wayDataBlocks = ReadOptionalWayDataBlocksByte(featureWayDataBlocksByte, readBuffer);
				if (wayDataBlocks < 1)
				{
                    Logger.Log(LogLevel.Warning, "invalid number of way data blocks: " + wayDataBlocks);
					return null;
				}

				for (int wayDataBlock = 0; wayDataBlock < wayDataBlocks; ++wayDataBlock)
				{
					List<List<Point>> wayNodes = ProcessWayDataBlock(tileLatitude, tileLongitude, featureWayDoubleDeltaEncoding, readBuffer);
					if (wayNodes != null)
					{
						if (filterRequired && wayFilterEnabled)
						{
                            double minX = double.MaxValue;
                            double minY = double.MaxValue;
                            double maxX = 0;
                            double maxY = 0;
                            for(int i = 0; i < wayNodes.Count; i++)
                            {
                                for(int j = 0; j < wayNodes[i].Count; j++)
                                {
                                    minX = Math.Min(minX, wayNodes[i][j].X);
                                    minY = Math.Min(minY, wayNodes[i][j].Y);
                                    maxX = Math.Max(maxX, wayNodes[i][j].X);
                                    maxY = Math.Max(maxY, wayNodes[i][j].Y);
                                }
                            }
                            if (!wayFilterBbox.Intersects(new BoundingBox(minX, minY, maxX, maxY)))
							    continue;
						}
						ways.Add(new Way(layer, tags, wayNodes, labelPosition));
					}
				}
			}

			return ways;
		}

		/// <summary>
		/// Reads all map data for the area covered by the given tile at the tile zoom level.
		/// </summary>
		/// <param name="tile">
		///            defines area and zoom level of read map data. </param>
		/// <returns> the read map data. </returns>
		public MapReadResult ReadMapData(Tile tile)
		{
			lock (sync)
			{
				try
				{
					QueryParameters queryParameters = new QueryParameters();
					queryParameters.queryZoomLevel = this.mapFileHeader.GetQueryZoomLevel((sbyte)tile.ZoomLevel);
        
					// get and check the sub-file for the query zoom level
					SubFileParameter subFileParameter = this.mapFileHeader.GetSubFileParameter(queryParameters.queryZoomLevel);
					if (subFileParameter == null)
					{
                        Logger.Log(LogLevel.Warning, "no sub-file for zoom level: " + queryParameters.queryZoomLevel);
						return null;
					}
        
					queryParameters.CalculateBaseTiles(tile, subFileParameter);
					queryParameters.CalculateBlocks(subFileParameter);

                    // we enlarge the bounding box for the tile slightly in order to retain any data that
                    // lies right on the border, some of this data needs to be drawn as the graphics will
                    // overlap onto this tile.
                    return ProcessBlocks(queryParameters, subFileParameter, tile.ToBoundingBox());
				}
				catch (IOException e)
				{
					Logger.Log(LogLevel.Error, e.Message, e);
					return null;
				}
			}
		}

		private Point ReadOptionalLabelPosition(double tileLatitude, double tileLongitude, bool featureLabelPosition, ReadBuffer readBuffer)
		{
			if (featureLabelPosition)
			{
				// get the label position latitude offset (VBE-S)
				double latitude = tileLatitude + PointUtils.MicrodegreesToDegrees(readBuffer.ReadSignedInt());

				// get the label position longitude offset (VBE-S)
				double longitude = tileLongitude + PointUtils.MicrodegreesToDegrees(readBuffer.ReadSignedInt());

				return new Point(longitude, latitude);
			}

			return null;
		}

		private int ReadOptionalWayDataBlocksByte(bool featureWayDataBlocksByte, ReadBuffer readBuffer)
		{
			if (featureWayDataBlocksByte)
			{
				// get and check the number of way data blocks (VBE-U)
				return readBuffer.ReadUnsignedInt();
			}
			// only one way data block exists
			return 1;
		}

		private int[][] ReadZoomTable(SubFileParameter subFileParameter, ReadBuffer readBuffer)
		{
			int rows = subFileParameter.ZoomLevelMax - subFileParameter.ZoomLevelMin + 1;
            int[][] zoomTable = new int[rows][];
            for (int array1 = 0; array1 < rows; array1++)
            {
                zoomTable[array1] = new int[2];
            }

            int cumulatedNumberOfPois = 0;
			int cumulatedNumberOfWays = 0;

			for (int row = 0; row < rows; ++row)
			{
				cumulatedNumberOfPois += readBuffer.ReadUnsignedInt();
				cumulatedNumberOfWays += readBuffer.ReadUnsignedInt();

				zoomTable[row][0] = cumulatedNumberOfPois;
				zoomTable[row][1] = cumulatedNumberOfWays;
			}

			return zoomTable;
		}

		/// <summary>
		/// Restricts returns of data to zoom level range specified. This can be used to restrict
		/// the use of this map data base when used in MultiMapDatabase settings. </summary>
		/// <param name="minZoom"> minimum zoom level supported </param>
		/// <param name="maxZoom"> maximum zoom level supported </param>
		public virtual void RestrictToZoomRange(sbyte minZoom, sbyte maxZoom)
		{
			this.zoomLevelMax = maxZoom;
			this.zoomLevelMin = minZoom;
		}

		public Point StartPosition
        {
            get
            {
                if (null != MapFileInfo.StartPosition)
                {
                    return MapFileInfo.StartPosition;
                }
                return MapFileInfo.BoundingBox.GetCentroid();
            }
            set
            {
            }
        }

        public sbyte? StartZoomLevel
        {
            get
            {
                if (null != MapFileInfo.StartZoomLevel)
                {
                    return MapFileInfo.StartZoomLevel;
                }
                return DEFAULT_START_ZOOM_LEVEL;
            }
            set
            {
            }
        }

		public bool SupportsTile(Tile tile)
		{
			return tile.ToBoundingBox().Intersects(MapFileInfo.BoundingBox)
                && tile.ZoomLevel >= this.zoomLevelMin && tile.ZoomLevel <= this.zoomLevelMax;
        }
	}
}