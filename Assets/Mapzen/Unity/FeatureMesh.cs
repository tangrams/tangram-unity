
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

        public string GetName(SceneGroup.Type groupType)
        {
            string name = "";

            switch (groupType)
            {
                case SceneGroup.Type.Feature: name = identifier; break;
                case SceneGroup.Type.Filter: name = filter; break;
                case SceneGroup.Type.Layer: name = layer; break;
                case SceneGroup.Type.Tile: name = tile; break;
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
