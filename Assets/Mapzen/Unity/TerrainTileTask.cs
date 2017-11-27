using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Mapzen.Unity
{
    public class TerrainTileTask
    {
        public TileAddress Address;

        public String ApiKey;

        public int Resolution = 128;

        public float UnitsPerMeter = 1.0f;

        public Material Material;

        public GameObject TargetObject;

        protected string urlTemplateElevation = "https://tile.mapzen.com/mapzen/terrain/v1/terrarium/{0}/{1}/{2}.png?api_key={3}";

        public IEnumerator Run()
        {
            // Download elevation texture.
            var url = String.Format(urlTemplateElevation, Address.z, Address.x, Address.y, ApiKey);
            var webRequest = UnityWebRequestTexture.GetTexture(url);
            yield return webRequest.Send();
            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.LogError(String.Format("Error: {0} for URL: {1}", webRequest.error, url));
                yield break;
            }
            var textureHandler = webRequest.downloadHandler as DownloadHandlerTexture;
            var texture = textureHandler.texture;
            texture.wrapMode = TextureWrapMode.Clamp;

            // Generate vertices and indices for grid mesh.
            var metersPerTile = (float)Address.GetSizeMercatorMeters();
            var mesh = GenerateElevationGridMesh(texture, Resolution, metersPerTile, UnitsPerMeter);
            mesh.RecalculateNormals();

            // Attach the mesh to the target GameObject.
            TargetObject.AddComponent<MeshFilter>().mesh = mesh;
            TargetObject.AddComponent<MeshRenderer>().material = Material;
        }

        public static float ColorToElevation(Color color)
        {
            // Convert from color channel values in 0.0-1.0 range to elevation in meters:
            // https://mapzen.com/documentation/terrain-tiles/formats/#terrarium
            return (color.r * 256.0f * 256.0f + color.g * 256.0f + color.b) - 32768.0f;
        }

        public static Mesh GenerateElevationGridMesh(Texture2D elevationTexture, int resolution, float metersPerTile, float unitsPerMeter)
        {
            // Create empty lists for all of the vertex values we need to set.
            var vertices = new List<Vector3>();
            var indices = new List<int>();
            var uvs = new List<Vector2>();

            // Iterate over the rows and columns of a grid in X and Z.
            int index = 0;
            for (int col = 0; col <= resolution; col++)
            {
                float z = (float)col / resolution;
                for (int row = 0; row <= resolution; row++)
                {
                    float x = (float)row / resolution;

                    int xPixel = Convert.ToInt32(x * elevationTexture.width);
                    int yPixel = Convert.ToInt32(z * elevationTexture.height);
                    float elevation = ColorToElevation(elevationTexture.GetPixel(xPixel, yPixel));
                    float y = elevation / metersPerTile;

                    // Add the values for a new vertex.
                    vertices.Add(new Vector3(x, y, z) * (unitsPerMeter * metersPerTile));
                    uvs.Add(new Vector2(x, z));

                    // Add indices for form triangles between this vertex and its neighbors left and down, unless
                    // we're at the end of a column or row.
                    if (row < resolution && col < resolution)
                    {
                        indices.Add(index);
                        indices.Add(index + resolution + 1);
                        indices.Add(index + 1);

                        indices.Add(index + 1);
                        indices.Add(index + resolution + 1);
                        indices.Add(index + resolution + 2);
                    }

                    index++;
                }
            }

            var mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(indices, 0);
            return mesh;
        }
    }
}
