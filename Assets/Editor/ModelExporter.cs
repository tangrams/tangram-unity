using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ModelExporter
{
    private struct MaterialDesc
    {
        public string mainTexture;
        public Color color;
    }

    public static void OBJ(List<MeshFilter> meshes, string filename)
    {
        int vertexOffset = 0;
        int normalOffset = 0;
        int uvOffset = 0;

        StringBuilder builder = new StringBuilder();

        builder.Append("# Exported with Tangram Unity\n");
        builder.Append("# Visit https://github.com/tangrams/tangram-unity/ for more details\n\n");
        builder.Append("mtllib " + filename + ".mtl\n");

        foreach (var meshFilter in meshes)
        {
            Mesh mesh = meshFilter.sharedMesh;

            Material[] materials = meshFilter.GetComponent<Renderer>().sharedMaterials;

            builder.Append("g ").Append(meshFilter.name).Append("\n");

            foreach (Vector3 lv in mesh.vertices)
            {
                Vector3 wv = meshFilter.transform.TransformPoint(lv);
                builder.Append(string.Format("v {0} {1} {2}\n", -wv.x, wv.y, wv.z));
            }

            builder.Append("\n");

            foreach (Vector3 ln in mesh.normals)
            {
                Vector3 wn = meshFilter.transform.TransformDirection(ln);
                builder.Append(string.Format("vn {0} {1} {2}\n", -wn.x, wn.y, wn.z));
            }

            builder.Append("\n");

            foreach (Vector2 uv in mesh.uv)
            {
                builder.Append(string.Format("vt {0} {1}\n", uv.x, uv.y));
            }

            for (int i = 0; i < mesh.subMeshCount; ++i)
            {
                var material = materials[i];

                builder.Append("\n");
                builder.Append("usemtl ").Append(material.name).Append("\n");
                builder.Append("usemap ").Append(material.name).Append("\n");

                int[] triangles = mesh.GetTriangles(i);
                for (int j = 0; j < triangles.Length; j += 3)
                {
                    if (mesh.uv.Length > 0)
                    {
                        builder.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n",
                                triangles[j + 0] + 1 + vertexOffset,
                                triangles[j + 1] + 1 + vertexOffset,
                                triangles[j + 2] + 1 + vertexOffset));
                    }
                    else
                    {
                        builder.Append(string.Format("f {1}//{1} {0}//{0} {2}//{2}\n",
                                triangles[j + 0] + 1 + vertexOffset,
                                triangles[j + 1] + 1 + vertexOffset,
                                triangles[j + 2] + 1 + vertexOffset));
                    }
                }
            }

            vertexOffset += mesh.vertices.Length;
        }

        var materialAssetPerName = new Dictionary<string, MaterialDesc>();
        foreach (var meshFilter in meshes)
        {
            Mesh mesh = meshFilter.sharedMesh;
            Material[] materials = meshFilter.GetComponent<Renderer>().sharedMaterials;

            for (int i = 0; i < mesh.subMeshCount; ++i)
            {
                var material = materials[i];
                string mainTexture = null;
                if (material.mainTexture != null)
                {
                    mainTexture = AssetDatabase.GetAssetPath(material.mainTexture);
                }
                if (!materialAssetPerName.ContainsKey(material.name))
                {
                    MaterialDesc materialDesc = new MaterialDesc();
                    materialDesc.mainTexture = mainTexture;
                    materialDesc.color = material.color;
                    materialAssetPerName.Add(material.name, materialDesc);
                }
            }
        }

        using (StreamWriter sw = new StreamWriter(filename + ".mtl"))
        {
            foreach (var materialAssetPair in materialAssetPerName)
            {
                MaterialDesc materialDesc = materialAssetPair.Value;

                sw.Write("\n");
                sw.Write("newmtl {0}\n", materialAssetPair.Key);
                sw.Write("Ka 1.0 1.0 1.0\n");
                sw.Write(string.Format("Kd {0} {1} {2}\n",
                        materialDesc.color.r,
                        materialDesc.color.g,
                        materialDesc.color.b));
                sw.Write("Ks 0.0 0.0 0.0\n");
                sw.Write("d 1.0\n");
                sw.Write("Ns 0.0\n");
                sw.Write("illum 2\n");

                if (materialDesc.mainTexture != null)
                {
                    string dest = materialDesc.mainTexture;
                    int stripIndex = dest.LastIndexOf("/");
                    if (stripIndex >= 0)
                    {
                        dest = dest.Substring(stripIndex + 1).Trim();
                    }
                    File.Copy(materialDesc.mainTexture, dest);
                    sw.Write("map_Kd {0}\n", dest);
                }
            }
        }

        using (StreamWriter sw = new StreamWriter(filename + ".obj"))
        {
            sw.Write(builder.ToString());
        }

        Debug.Log("Model exported as " + filename + ".obj");
    }
}
