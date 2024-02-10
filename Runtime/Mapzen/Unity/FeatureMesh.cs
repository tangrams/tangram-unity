
namespace Mapzen.Unity
{
    public class FeatureMesh
    {
        private string identifier;
        private string tile;
        private string collection;
        private string layer;

        private MeshData mesh;

        public FeatureMesh(string tile, string collection, string layer, string identifier)
        {
            this.tile = tile;
            this.collection = collection;
            this.layer = layer;
            this.identifier = identifier;

            this.mesh = new MeshData();
        }

        public string GetName(SceneGroupType groupType)
        {
            string name;

            switch (groupType)
            {
                case SceneGroupType.Feature: name = identifier; break;
                case SceneGroupType.Layer: name = layer; break;
                case SceneGroupType.Collection: name = collection; break;
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

        public string Collection
        {
            get { return collection; }
        }

        public string Layer
        {
            get { return layer; }
        }

        public MeshData Mesh
        {
            get { return mesh; }
        }
    }
}
