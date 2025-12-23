using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Map
{
    public class MapDrawer : MonoBehaviour
    {
        [SerializeField] Mesh _voxelMesh;
        [SerializeField] Material _voxelMaterial;
        [SerializeField] ShadowCastingMode _shadowCastingMode;

        GraphicsBuffer _drawArgsBuffer;
        GraphicsBuffer _dataBuffer;
        RenderParams _renderParams;
        Vector3 _worldSize;
        int _totalSize;

        public void Redraw(bool[,,] voxels, int visibleVoxelsCount, int width, int height, int depth)
        {
            _totalSize = width * height * depth;
            _worldSize = new Vector3(width, height, depth);

            _renderParams = new RenderParams(_voxelMaterial)
            {
                receiveShadows = false,
                shadowCastingMode = _shadowCastingMode,
                worldBounds = new Bounds(Vector3.zero, _worldSize)
            };

            _drawArgsBuffer = CreateDrawArgsBufferForRenderMeshIndirect(_voxelMesh, visibleVoxelsCount);
            _dataBuffer = CreateDataBuffer<Matrix4x4>(visibleVoxelsCount);

            NativeArray<Matrix4x4> transformMatrixArray = TransformMatrixArrayFactory.CreateFromVoxels(
                visibleVoxelsCount,
                voxels
            );

            _dataBuffer.SetData(transformMatrixArray);

            _voxelMaterial.SetBuffer("_TransformMatrixArray", _dataBuffer);
            _voxelMaterial.SetVector("_BoundsOffset", transform.position);

            transformMatrixArray.Dispose();
        }

        void Update()
        {
            if (_totalSize == 0)
                return;

            Graphics.RenderMeshIndirect(_renderParams, _voxelMesh, _drawArgsBuffer);
        }

        void OnDestroy()
        {
            _drawArgsBuffer?.Release();
            _dataBuffer?.Release();
        }

        static GraphicsBuffer CreateDrawArgsBufferForRenderMeshIndirect(Mesh mesh, int instanceCount)
        {
            var commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];

            commandData[0] = new GraphicsBuffer.IndirectDrawIndexedArgs
            {
                indexCountPerInstance = mesh.GetIndexCount(0),
                instanceCount = (uint)instanceCount,
                startIndex = mesh.GetIndexStart(0),
                baseVertexIndex = mesh.GetBaseVertex(0),
            };

            var drawArgsBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.IndirectArguments,
                1,
                GraphicsBuffer.IndirectDrawIndexedArgs.size
            );

            drawArgsBuffer.SetData(commandData);

            return drawArgsBuffer;
        }

        static GraphicsBuffer CreateDataBuffer<T>(int instanceCount)
            where T : struct
        {
            return new GraphicsBuffer(GraphicsBuffer.Target.Structured, instanceCount, Marshal.SizeOf(typeof(T)));
        }
    }
}
