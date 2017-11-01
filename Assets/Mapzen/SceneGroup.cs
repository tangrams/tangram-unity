using System;
using System.Collections.Generic;
using Mapzen.Unity;

namespace Mapzen
{
    public class SceneGroup
    {
        [Flags]
        public enum GroupType
        {
            None = 0,
            Tile = 1 << 0,
            Filter = 1 << 1,
            Layer = 1 << 2,
            Feature = 1 << 3,
            All = ~None,
        }

        // Childs of this scene group, identifier by their name
        public Dictionary<string, SceneGroup> childs;
        // A name identifer
        public string name;
        // The mesh data, may be empty
        public MeshData meshData;

        public GroupType type;

        public SceneGroup(GroupType type, string name)
        {
            this.childs = new Dictionary<string, SceneGroup>();
            this.type = type;
            this.name = name;
            this.meshData = new MeshData();
        }

        /// <summary>
        /// Tests whether this group is enabled in the group type options.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="options">The group type options.</param>
        public static bool Test(GroupType type, GroupType options)
        {
            return ((int)type & (int)options) == (int)type;
        }

        /// <summary>
        /// Whether this group is the deepest group type in the hierarchy of group type options.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="options">The group type options.</param>
        public static bool IsLeaf(GroupType type, GroupType options)
        {
            return ((int)type ^ (int)options) < (int)type;
        }

        public override string ToString()
        {
            if (type == GroupType.None || type == GroupType.All)
            {
                return name;
            }

            return type.ToString() + "_" + name;
        }
    }
}
