using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fraktalia.VoxelGen.Samples
{
    public class DistanceBasedLOD : MonoBehaviour
    {
        public List<VoxelGenerator> AffectedGenerators;
        public bool UseAllActiveGenerators;
        
        public Transform ReferencePoint;

        public float[] LODDistances;

        // Update is called once per frame
        void Update()
        {
            if (UseAllActiveGenerators)
            {
                foreach (var item in VoxelGenerator.ActiveGenerators)
                {
                    CalculateLOD(item.Value, ReferencePoint.position);
                }
            }
        }

        public void CalculateLOD(VoxelGenerator generator, Vector3 cameraPosition)
        {
            float distance = Vector3.Distance(generator.transform.position, cameraPosition);
            int selectedLOD = 0;

            for (int i = 0; i < LODDistances.Length; i++)
            {
                if (distance > LODDistances[i])
                {
                    selectedLOD = i + 1;
                }
                else
                {
                    break;
                }
            }

            selectedLOD = Mathf.Clamp(selectedLOD, 0, LODDistances.Length);
            generator.SetLOD(selectedLOD);
        }

        private void OnDrawGizmosSelected()
        {
            if (ReferencePoint == null || LODDistances == null)
                return;

            for (int i = 0; i < LODDistances.Length; i++)
            {
                Gizmos.color = GetLODColor(i);
                Gizmos.DrawWireSphere(ReferencePoint.position, LODDistances[i]);
            }

            // Draw extra sphere for LODs beyond defined range
            Gizmos.color = GetLODColor(LODDistances.Length);
            if (LODDistances.Length > 0)
            {
                float extraRadius = LODDistances[LODDistances.Length - 1] + 5f;
                Gizmos.DrawWireSphere(ReferencePoint.position, extraRadius);
            }
        }

        private Color GetLODColor(int lod)
        {
            switch (lod)
            {
                case 0: return Color.white;
                case 1: return Color.blue;
                case 2: return Color.green;
                case 3: return Color.red;
                default: return Color.black;
            }
        }
    }
}