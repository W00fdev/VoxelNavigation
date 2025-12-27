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
        public Vector3Int MapSize => _mapSize;
        const float MinNodeSize = 1f;

        public AStarGraph waypoints => _waypoints;

        [SerializeField] MapDrawer _mapDrawer;
        [SerializeField] MapReader _mapReader;
        [SerializeField] DefaultNamespace.Mover _mover;

        NativeArray<float3> _positions;
        readonly AStarGraph _waypoints = new();
        Octree _ot;
        Vector3Int _mapSize;

        IEnumerator Start()
        {
            yield return null;

            //bool[,,] voxels = _mapReader.Read(cull: true, out int totalFilledVoxelsCount);
            //bool[,,] voxels = MapGenerator.GenerateTube();
            
            _mapSize = new Vector3Int(90, 90, 90);

            bool[,,] voxels = MapGenerator.CreateEmptyMap(_mapSize);
            ShapesFactory.CreateSphere(ref voxels, new Vector3Int(25, 25, 25), 10f);
            ShapesFactory.CreateWireFrameBox(ref voxels, new Vector3Int(0, 0, 0), new Vector3Int(10, 5, 20));
            ShapesFactory.CreateTorus(ref voxels, new Vector3Int(25, 0, 15), innerRadius: 5, outerRadius: 10);

            int totalFilledVoxelsCount = 0;
            for (int x = 0; x < 90; x++)
            {
                for (int y = 0; y < 90; y++)
                {
                    for (int z = 0; z < 90; z++)
                    {
                        if (voxels[x, y, z])
                            totalFilledVoxelsCount++;

                    }
                }
            }


            //MapCuller.Cull(ref voxels, out int totalFilledVoxelsCount);

            _mapDrawer.Redraw(
                voxels,
                totalFilledVoxelsCount,
                width: voxels.GetLength(0),
                height: voxels.GetLength(1),
                depth: voxels.GetLength(2)
            );

            _positions = TransformMatrixArrayFactory.CreatePositionsFromVoxels(totalFilledVoxelsCount, voxels);

            _ot = new Octree(ref _positions, MinNodeSize, _waypoints);
            _mover.Go();
        }

        void OnDestroy()
        {
            _positions.Dispose();
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
