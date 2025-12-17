using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Fraktalia.VoxelGen
{
    public class VoxelSubmersionCheck : MonoBehaviour
    {
        public UnityEvent<float> OnEvaluateSubmersion; 

        [Range(0, 1)]
        public float SubmersionPercentage;

        public int SubmersionThreshold = 128;

        public VoxelGenerator Target;
        public int TargetDimension;
        public Vector3[] LocalPointCloud;
        public float SecondsBetweenChecks = 0.5f;


        private Vector3[] WorldPointCloud;
        private int[] SubmersionResult;

        private void Awake()
        {
            WorldPointCloud = new Vector3[LocalPointCloud.Length];
            SubmersionResult = new int[LocalPointCloud.Length];   
        }

        private void OnDrawGizmosSelected()
        {
            for (int i = 0; i < LocalPointCloud.Length; i++)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawSphere(LocalPointCloud[i], 0.1f);
            }
        }

        private void OnEnable()
        {
            StartCoroutine(IntervalCheck());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator IntervalCheck()
        {
            while (true)
            {
                if (Target.IsInitialized)
                {
                    for (int i = 0; i < LocalPointCloud.Length; i++)
                    {
                        Vector3 worldVertex = transform.TransformPoint(LocalPointCloud[i]);
                        WorldPointCloud[i] = worldVertex;
                    }

                    Target.GetVoxelValuesAt(WorldPointCloud, TargetDimension, 0, 0, SubmersionResult);

                    float result = 0;
                    for (int i = 0; i < SubmersionResult.Length; i++)
                    {
                        if (SubmersionResult[i] > SubmersionThreshold) result++;
                    }
                    SubmersionPercentage = result / SubmersionResult.Length;
                    OnEvaluateSubmersion.Invoke(SubmersionPercentage);
                }

                yield return new WaitForSeconds(SecondsBetweenChecks);
            }
        }
    }
}