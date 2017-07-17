using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Mapzen.VectorData.Formats
{
    using PbfTile = Mvtcs.Tile;

    public class MvtTile : Tile
    {
        protected PbfTile tile;

        public MvtTile(TileAddress address, byte[] data)
        {
            this.address = address;

            // Check the start of the data to see if it is a gzip file.
            // The first two bytes in a gzip file are the magic numbers 31 and 139: http://www.gzip.org/zlib/rfc-gzip.html
            if (data.Length >= 2 && (data[0] == 31 && data[1] == 139))
            {
                // We need to decompress the gzip file before parsing it.
                using (MemoryStream inputMemStream = new MemoryStream(data))
                {
                    using (GZipStream gzStream = new GZipStream(inputMemStream, CompressionMode.Decompress))
                    {
                        this.tile = PbfTile.Parser.ParseFrom(gzStream);
                    }
                }
            }
            else
            {
                // We can parse the bytes directly.
                this.tile = PbfTile.Parser.ParseFrom(data);
            }
        }

        public override IEnumerable<FeatureCollection> FeatureCollections
        {
            get
            {
                foreach (var pbfCollection in tile.Layers)
                {
                    yield return new MvtFeatureCollection(pbfCollection);
                }
            }
        }
    }
}
