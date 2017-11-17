using System;

namespace Mapzen
{
    [Flags]
    public enum SceneGroupType
    {
        Tile = 1 << 0,
        Filter = 1 << 1,
        Layer = 1 << 2,
        Feature = 1 << 3,
    }
}
