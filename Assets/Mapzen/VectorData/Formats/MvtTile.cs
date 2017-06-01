using System;
using System.Collections.Generic;

namespace Mapzen.VectorData.Formats
{
    using PbfTile = Mvtcs.Tile;

    public class MvtTile : Tile
    {
        protected PbfTile tile;

        public MvtTile(TileAddress address, byte[] data)
        {
            this.address = address;
            this.tile = Mvtcs.Tile.Parser.ParseFrom(data);
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
