using Map;
using Octrees;
using Pathfinding;
using Root.Statistics;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Root
{
    public class Bootstrap : MonoBehaviour
    {
        public Vector3Int MapSize => _mapSize;
        const float MinNodeSize = 1f;

        [SerializeField] MapDrawer _mapDrawer;
        [SerializeField] MapReader _mapReader;
        [SerializeField] AStarBenchmark _aStarBenchmark;
        [SerializeField] GridMover _gridMover;

        //[SerializeField] Root.Mover _mover;

        readonly AStarGraph _aStarGraph = new();
        readonly FlowFieldGraph _flowFieldGraph = new();
        NativeArray<float3> _positions;
        AStarGrid _aStarGrid;
        Octree _octree;
        Vector3Int _mapSize;

        IEnumerator Start()
        {
            yield return null;

            //bool[,,] voxels = _mapReader.Read(cull: true, out int totalFilledVoxelsCount);
            bool[,,] voxels = CreateLowResolutionMap(out int totalFilledVoxelsCount);

            var positions = TransformMatrixArrayFactory.CreatePositionsFromVoxels(totalFilledVoxelsCount, voxels);

            _octree = CreateOctreeGraph(ref positions);

/*            _mapDrawer.Redraw(
                voxels,
                totalFilledVoxelsCount,
                width: voxels.GetLength(0),
                height: voxels.GetLength(1),
                depth: voxels.GetLength(2)
            );*/

            yield return null;
            yield return null;
            //_aStarBenchmark.GoCheckWithMover(115);
            //_aStarBenchmark.Go();
            //_mover.Go();

            UnityEngine.Debug.Log($"#{Time.frameCount}: since startup {Time.realtimeSinceStartup}");

            //_gridMover.GoRandom();
            //_aStarBenchmark.GoGraph();
            //_aStarBenchmark.GoGrid();
            //_aStarBenchmark.ExploreBenchmarkAt(13);

            //_aStarBenchmark.Initialize(_ot.aStarGraph);

            _positions = positions;

            _flowFieldGraph.FindPath(GetClosestNode(Vector3.zero), GetClosestNode(Vector3.one * 20));
            _flowFieldGraph.FindPath(GetClosestNode(new Vector3(20, 20, 0)), GetClosestNode(Vector3.one * 20));

            yield return null;

            UnityEngine.Debug.Log($"#{Time.frameCount}: start noising {Time.realtimeSinceStartup}");

            List<Vector3> addedVoxels = ShapesFactory.CreateNoiseAt(ref voxels, new Vector3Int(10, 0, 12), new Vector3Int(5, 5, 2));
            foreach (Vector3 addedVoxelPosition in addedVoxels)
            {
                _octree.AddPosition(addedVoxelPosition);
            }

            UnityEngine.Debug.Log($"#{Time.frameCount} end noising {Time.realtimeSinceStartup}");

            _mapDrawer.Dispose();
            _mapDrawer.Redraw(
                voxels,
                totalFilledVoxelsCount,
                width: voxels.GetLength(0),
                height: voxels.GetLength(1),
                depth: voxels.GetLength(2)
            );
        }

        private bool[,,] CreateLowResolutionMap(out int totalFilledVoxelsCount)
        {
            _mapSize = new Vector3Int(20, 20, 20);

            bool[,,] voxels = MapGenerator.CreateEmptyMap(_mapSize.x, _mapSize.y, _mapSize.z);

            ShapesFactory.CreateSphere(ref voxels, new Vector3Int(5, 5, 5), 5);
            ShapesFactory.CreateWireFrameBox(ref voxels, new Vector3Int(15, 15, 5), new Vector3Int(4, 4, 4));

            totalFilledVoxelsCount = 0;
            for (int x = 0; x < _mapSize.x; x++)
            {
                for (int y = 0; y < _mapSize.y; y++)
                {
                    for (int z = 0; z < _mapSize.z; z++)
                    {
                        if (voxels[x, y, z])
                        {
                            totalFilledVoxelsCount++;
                        }
                    }
                }
            }

            return voxels;
        }

        Octree CreateOctreeGraph(ref NativeArray<float3> positions)
        {
            return new Octree(ref positions, MinNodeSize, _flowFieldGraph);
        }

        void ReadMapWithForms(bool[,,] voxels, out int totalFilledVoxelsCount)
        {
            _mapSize = new Vector3Int(voxels.GetLength(0), voxels.GetLength(1), voxels.GetLength(2));

            ShapesFactory.CreateSphere(ref voxels, new Vector3Int(34, 54, 54), 15);
            ShapesFactory.CreateWireFrameBox(ref voxels, new Vector3Int(10, 54, 54), new Vector3Int(5, 5, 5));

            MapCuller.Cull(ref voxels, out totalFilledVoxelsCount);
        }

        void GenerateGrid(bool[,,] voxels, out int totalFilledVoxelsCount)
        {
            totalFilledVoxelsCount = 0;
         
            _mapSize = new Vector3Int(voxels.GetLength(0), voxels.GetLength(1), voxels.GetLength(2));
            _aStarGrid = new AStarGrid(_mapSize);

            ShapesFactory.CreateSphere(ref voxels, new Vector3Int(34, 54, 54), 15);
            ShapesFactory.CreateWireFrameBox(ref voxels, new Vector3Int(10, 54, 54), new Vector3Int(5, 5, 5));

            for (int x = 0; x < _mapSize.x; x++)
            {
                for (int y = 0; y < _mapSize.y; y++)
                {
                    for (int z = 0; z < _mapSize.z; z++)
                    {
                        if (voxels[x, y, z])
                        {
                            _aStarGrid.AddNode(new GridNode(new Vector3Int(x, y, z)));
                            totalFilledVoxelsCount++;
                        }
                    }
                }
            }
        }

        OctreeNode GetClosestNode(Vector3 position)
        {
            OctreeNode closestNode = null;
            float closestDistanceSqr = Mathf.Infinity;

            foreach (var kvNode in _flowFieldGraph.nodes)
            {
                OctreeNode node = kvNode.Key;
                float distanceSqr = (node.bounds.center - position).sqrMagnitude;

                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestNode = node;
                }
            }

            return closestNode;
        }

        void OnDestroy()
        {
            _positions.Dispose();
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            if (_octree != null)
            {
                Gizmos.DrawWireCube(_octree.bounds.center, _mapSize);

                _octree.root.DrawNode();
                _octree.flowFieldGraph.DrawGraph();
            }

            _aStarGrid?.DrawGraph();
        }
    }
}