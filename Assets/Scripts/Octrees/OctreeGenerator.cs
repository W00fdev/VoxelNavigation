using System.Collections;
using Map;
using Pathfinding;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Octrees
{
    public class OctreeGenerator : MonoBehaviour
    {
        const float MinNodeSize = 1f;

        public AStarGraph waypoints => _waypoints;

        [SerializeField] MapDrawer _mapDrawer;
        [SerializeField] MapReader _mapReader;

        readonly AStarGraph _waypoints = new();
        Octree _ot;

        IEnumerator Start()
        {
            yield return null;

            bool[,,] voxels = _mapReader.Read(cull: true, out int totalFilledVoxelsCount);

            _mapDrawer.Redraw(
                voxels,
                totalFilledVoxelsCount,
                width: voxels.GetLength(0),
                height: voxels.GetLength(1),
                depth: voxels.GetLength(2)
            );

            NativeArray<float3> positions =
                TransformMatrixArrayFactory.CreatePositionsFromVoxels(totalFilledVoxelsCount, voxels);

            _ot = new Octree(ref positions, MinNodeSize, _waypoints);
        }

        void OnDrawGizmos()
        {
            if (_ot == null)
                return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_ot.bounds.center, _ot.bounds.size);

            _ot.root.DrawNode();
            _ot.aStarGraph.DrawGraph();
        }
    }
}
