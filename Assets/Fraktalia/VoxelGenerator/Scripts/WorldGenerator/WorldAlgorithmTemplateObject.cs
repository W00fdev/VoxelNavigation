using Fraktalia.VoxelGen.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fraktalia.VoxelGen
{

    public class WorldAlgorithmTemplateObject : MonoBehaviour
    {
        internal WorldAlgorithmCluster[] RetrieveClusters()
        {
            var clusters = GetComponentsInChildren<WorldAlgorithmCluster>();
            return clusters;
        }
    }
}
