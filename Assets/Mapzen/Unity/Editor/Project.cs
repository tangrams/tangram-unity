using UnityEditor;

namespace Mapzen.Unity.Editor
{
    public class Project
    {
        public static void ExportPackage()
        {
            AssetDatabase.ExportPackage("Assets", "tangram-unity.unitypackage", ExportPackageOptions.Recurse);
        }
    }
}
