using System;
using System.Collections.Generic;
using Mapzen.Unity;

namespace Mapzen
{
    public class SceneGroup 
    {
        /// <summary>
        /// Tests whether this group is enabled in the group type options.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="options">The group type options.</param>
        public static bool Test(SceneGroupType type, SceneGroupType options)
        {
            if (options == SceneGroupType.Nothing)
            {
                return type == options;
            }

            if (options == SceneGroupType.Everything)
            {
                return true;
            }

            return ((int)type & (int)options) == (int)type;
        }

        /// <summary>
        /// Returns the leaf for the hierarchy of group type options
        /// </summary>
        /// <returns>The leaf type for the group options.</returns>
        /// <param name="options">The group type options.</param>
        public static SceneGroupType GetLeaf(SceneGroupType options)
        {
            if (options == SceneGroupType.Nothing)
            {
                return options;
            }

            if (options == SceneGroupType.Everything)
            {
                return SceneGroupType.Feature;
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
