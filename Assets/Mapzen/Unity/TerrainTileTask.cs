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
            // Create pre-allocated arrays for all of the mesh values we need to set.
            int totalVertices = (resolution + 1) * (resolution + 1);
            int totalIndices = resolution * resolution * 6;
            var vertices = new Vector3[totalVertices];
            var indices = new int[totalIndices];
            var uvs = new Vector2[totalVertices];

            // Iterate over the rows and columns of a grid in X and Z.
            int textureWidth = elevationTexture.width;
            int textureHeight = elevationTexture.height;
            int vertexCount = 0;
            int indexCount = 0;
            for (int col = 0; col <= resolution; col++)
            {
                float z = (float)col / resolution;
                for (int row = 0; row <= resolution; row++)
                {
                    float x = (float)row / resolution;

                    int xPixel = (int)(x * textureWidth);
                    int yPixel = (int)(z * textureHeight);
                    float elevation = ColorToElevation(elevationTexture.GetPixel(xPixel, yPixel));
                    float y = elevation / metersPerTile;

                    // Add the values for a new vertex.
                    vertices[vertexCount] = new Vector3(x, y, z) * (unitsPerMeter * metersPerTile);
                    uvs[vertexCount] = new Vector2(x, z);

                    // Add indices for form triangles between this vertex and its neighbors left and down, unless
                    // we're at the end of a column or row.
                    if (row < resolution && col < resolution)
                    {
                        indices[indexCount++] = vertexCount;
                        indices[indexCount++] = vertexCount + resolution + 1;
                        indices[indexCount++] = vertexCount + 1;

                        indices[indexCount++] = vertexCount + 1;
                        indices[indexCount++] = vertexCount + resolution + 1;
                        indices[indexCount++] = vertexCount + resolution + 2;
                    }

                    vertexCount++;
                }
            }

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = indices;
            return mesh;
        }
    }
}
