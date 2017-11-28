using System;

namespace Mapzen
{
    [Flags]
    public enum SceneGroupType
    {
        Nothing = 0,
        Tile = 1 << 1,
        Filter = 1 << 2,
        Layer = 1 << 3,
        Feature = 1 << 4,
        Everything = ~Nothing,
    }
}
