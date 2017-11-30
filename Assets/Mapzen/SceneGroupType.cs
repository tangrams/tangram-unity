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

    public static class SceneGroupTypeExtensions
    {
        /// <summary>
        /// Tests whether this group is enabled in the group type options.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="options">The group type options.</param>
        public static bool Includes(this SceneGroupType options, SceneGroupType type)
        {
            return ((int)type & (int)options) == (int)type;
        }

        /// <summary>
        /// Returns the leaf for the hierarchy of group type options
        /// </summary>
        /// <returns>The leaf type for the group options.</returns>
        /// <param name="options">The group type options.</param>
        public static SceneGroupType GetLeaf(this SceneGroupType options)
        {
            int enumCount = Enum.GetNames(typeof(SceneGroupType)).Length;
            int leftMost = 1 << enumCount;

            while (!options.Includes((SceneGroupType)leftMost))
            {
                leftMost >>= 1;
            }

            return (SceneGroupType)leftMost;
        }
    }
}
