using System;
using System.Collections.Generic;

namespace Mapzen
{
    public class SceneGroup
    {
        public List<SceneGroup> childs;
        public MeshData meshData;
        public string name;

        public SceneGroup(string name, MeshData meshData)
        {
            this.childs = new List<SceneGroup>();
            this.name = name;
            this.meshData = meshData;
        }
    }
}
