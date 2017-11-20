using System;
using System.Collections.Generic;
using Mapzen.Unity;

namespace Mapzen
{
    [Flags]
    public enum SceneGroupType
    {
        None = 0,
        Tile = 1 << 0,
        Filter = 1 << 1,
        Layer = 1 << 2,
        Feature = 1 << 3,
        All = ~None,
    }

    public class SceneGroup 
    {
        /// <summary>
        /// Tests whether this group is enabled in the group type options.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="options">The group type options.</param>
        public static bool Test(SceneGroupType type, SceneGroupType options)
        {
            return type == SceneGroupType.None ? type == options : ((int)type & (int)options) == (int)type;
        }

        /// <summary>
        /// Returns the leaf for the hierarchy of group type options
        /// </summary>
        /// <returns>The leaf type for the group options.</returns>
        /// <param name="options">The group type options.</param>
        public static SceneGroupType GetLeaf(SceneGroupType options)
        {
            if (options == SceneGroupType.None)
            {
                return options;
            }

            int enumCount = Enum.GetNames(typeof(SceneGroupType)).Length;
            int leftMost = 1 << enumCount;

            while (!Test((SceneGroupType)leftMost, options))
            {
                leftMost >>= 1;
            }

            return (SceneGroupType)leftMost;
        }
    }
}
