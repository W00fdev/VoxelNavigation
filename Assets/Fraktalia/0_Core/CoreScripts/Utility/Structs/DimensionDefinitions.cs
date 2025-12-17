using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fraktalia.Utility
{
    [CreateAssetMenu(fileName = "DimensionDefinitions", menuName = "Fraktalia/DimensionDefinitions", order = 1)]
    public class DimensionDefinitions : ScriptableObject
    {
        public List<DimensionDefinition> dimensionDefinitions = new List<DimensionDefinition>();
    }

    [System.Serializable]
    public class DimensionDefinition
    {
        public string Name;
        public Texture2D Texture;
        public GameObject Prefab;
    }
}