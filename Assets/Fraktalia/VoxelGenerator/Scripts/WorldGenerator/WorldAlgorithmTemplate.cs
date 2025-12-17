using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fraktalia.VoxelGen
{
    [CreateAssetMenu(fileName = "WorldAlgorithmTemplate", menuName = "Fraktalia/CreateWorldAlgorithmTemplate", order = 1)]
    public class WorldAlgorithmTemplate : ScriptableObject
    {
        public WorldAlgorithmTemplateObject WorldAlgorithmContainer;
    }
}
