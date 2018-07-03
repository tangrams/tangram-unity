using System;

namespace Nextzen.Unity
{
    /// <summary>
    /// A UV mode determines how material texture coordinates are applied to the faces of polygons.
    /// </summary>
    [Serializable]
    public enum UVMode
    {
        /// <summary>
        /// Stretch the texture to fill the face of the polygon in both dimensions.
        /// </summary>
        Stretch,

        /// <summary>
        /// Repeat the texture in both dimensions to fill the face of the polygon without stretching.
        /// </summary>
        Tile,

        /// <summary>
        /// Stretch the texture horizontally and repeat it vertically.
        /// </summary>
        StretchUTileV,

        /// <summary>
        /// Repeat the texture vertically and stretch it horizontally.
        /// </summary>
        TileUStretchV,
    }
}