using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mapzen.Unity
{
    [Serializable]
    [CreateAssetMenu(menuName = "Mapzen/MapStyle")]
    public class MapStyle : ScriptableObject
    {
        // Version information
        // This allows us to check whether an asset was serialized with a different version than this code.
        // If a serialized field of this class is changed or renamed, currentAssetVersion should be incremented.

        private const int currentAssetVersion = 1;
        [SerializeField] private int serializedAssetVersion = currentAssetVersion;

        public List<FeatureLayer> Layers = new List<FeatureLayer>();

        [SerializeField]
        private bool liveUpdateEnabled;

        void OnEnable()
        {
            if (serializedAssetVersion != currentAssetVersion)
            {
                Debug.LogWarningFormat("The MapStyle \"{0}\" was created with a different version of this tool. " +
                    "Some properties may be missing or have unexpected values.", this.name);
                serializedAssetVersion = currentAssetVersion;
            }
        }
    }
}
