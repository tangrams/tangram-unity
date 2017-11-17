using System;
using System.Collections.Generic;
using Mapzen.Unity;

namespace Mapzen
{
    public class SceneGroup
    {
        // Children of this scene group, identified by their name
        public List<SceneGroup> children;

        // A name identifer
        public string name;

        // The mesh data, may be empty
        public MeshData meshData;

        public SceneGroupType type;

        public SceneGroup(SceneGroupType type, string name)
        {
            this.children = new List<SceneGroup>();
            this.type = type;
            this.name = name;
            this.meshData = new MeshData();
        }

        /// <summary>
        /// Tests whether this group is enabled in the group type options.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="options">The group type options.</param>
        public static bool Test(SceneGroupType type, SceneGroupType options)
        {
            return ((int)type & (int)options) == (int)type;
        }

        /// <summary>
        /// Whether this group is the deepest group type in the hierarchy of group type options.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <param name="options">The group type options.</param>
        public static bool IsLeaf(SceneGroupType type, SceneGroupType options)
        {
            return ((int)type ^ (int)options) < (int)type;
        }

        public override string ToString()
        {
            if (type == 0)
            {
                return name;
            }

            return type.ToString() + "_" + name;
        }
    }
}
