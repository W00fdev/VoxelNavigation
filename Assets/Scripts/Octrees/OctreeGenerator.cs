using System.Collections;
using DefaultNamespace.Statistics;
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
        [SerializeField] AStarBenchmark _aStarBenchmark;
        //[SerializeField] DefaultNamespace.Mover _mover;

        NativeArray<float3> _positions;
        readonly AStarGraph _waypoints = new();
        Octree _ot;
        Vector3Int _mapSize;

        IEnumerator Start()
        {
            yield return null;

            bool[,,] voxels = _mapReader.Read(cull: true, out int totalFilledVoxelsCount);
            //bool[,,] voxels = MapGenerator.GenerateTube();

            _mapSize = new Vector3Int(voxels.GetLength(0), voxels.GetLength(1), voxels.GetLength(2));

            //bool[,,] voxels = MapGenerator.CreateEmptyMap(_mapSize);
            ShapesFactory.CreateSphere(ref voxels, new Vector3Int(34, 54, 54), 15);
            ShapesFactory.CreateWireFrameBox(ref voxels, new Vector3Int(10, 54, 54), new Vector3Int(5, 5, 5));

            /*for (int x = 0; x < _mapSize.x; x++)
            {
                for (int y = 0; y < _mapSize.y; y++)
                {
                    for (int z = 0; z < _mapSize.z; z++)
                    {
                        if (voxels[x, y, z])
                            totalFilledVoxelsCount++;
                    }
                }
            }*/


            MapCuller.Cull(ref voxels, out totalFilledVoxelsCount);

            _mapDrawer.Redraw(
                voxels,
                totalFilledVoxelsCount,
                width: voxels.GetLength(0),
                height: voxels.GetLength(1),
                depth: voxels.GetLength(2)
            );

            _positions = TransformMatrixArrayFactory.CreatePositionsFromVoxels(totalFilledVoxelsCount, voxels);

            _ot = new Octree(ref _positions, MinNodeSize, _waypoints);

            yield return null;
            yield return null;
            _aStarBenchmark.GoCheckWithMover(115);
            //_aStarBenchmark.Go();
            //_mover.Go();
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
            Gizmos.DrawWireCube(_ot.bounds.center, _mapSize);

            _ot.root.DrawNode();
            _ot.aStarGraph.DrawGraph();
        }
    }
}