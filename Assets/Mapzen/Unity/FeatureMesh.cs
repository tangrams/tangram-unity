
namespace Mapzen.Unity
{
    public class FeatureMesh
    {
        private string identifier;
        private string tile;
        private string layer;
        private string filter;

        private MeshData mesh;

        public FeatureMesh(string tile, string layer, string filter, string identifier)
        {
            this.tile = tile;
            this.layer = layer;
            this.filter = filter;
            this.identifier = identifier;

            this.mesh = new MeshData();
        }

        public string GetName(SceneGroupType groupType)
        {
            string name;

            switch (groupType)
            {
                case SceneGroupType.Feature: name = identifier; break;
                case SceneGroupType.Filter: name = filter; break;
                case SceneGroupType.Layer: name = layer; break;
                case SceneGroupType.Tile: name = tile; break;
                default: name = ""; break;
            }

            return name;
        }

        public string Identifier
        {
            get { return identifier; }
        }

        public string Tile
        {
            get { return tile; }
        }

        public string Layer
        {
            get { return layer; }
        }

        public string Filter
        {
            get { return filter; }
        }

        public MeshData Mesh
        {
            get { return mesh; }
        }
    }
}
