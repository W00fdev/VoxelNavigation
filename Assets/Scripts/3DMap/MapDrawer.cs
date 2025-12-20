using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class MapDrawer : MonoBehaviour
{
    public Mesh voxelMesh;
    public Material VoxelMaterial;

    [SerializeField] private ShadowCastingMode _shadowCastingMode;

    private GraphicsBuffer _drawArgsBuffer;
    private GraphicsBuffer _dataBuffer;

    private RenderParams _renderParams;
    private Vector3 _worldSize;
    private int _totalSize = 0;

    public void Redraw(bool[,,] voxels, int width, int height, int depth)
    {
        _totalSize = width * height * depth;
        _worldSize = new Vector3(width, height, depth);
        _renderParams = new RenderParams(VoxelMaterial)
        {
            receiveShadows = false,
            shadowCastingMode = _shadowCastingMode,
            worldBounds = new Bounds(Vector3.zero, _worldSize)
        };

        var matrices = new Matrix4x4[_totalSize];
        uint visibleVoxelsCount = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (voxels[x, y, z])
                    {
                        visibleVoxelsCount++;
                    }
                }
            }
        }

        _drawArgsBuffer = CreateDrawArgsBufferForRenderMeshIndirect(voxelMesh, (int)visibleVoxelsCount);
        _dataBuffer = CreateDataBuffer<Matrix4x4>((int)visibleVoxelsCount);

        NativeArray<Matrix4x4> transformMatrixArray = TransformMatrixArrayFactory.CreateFromVoxels(
            (int)visibleVoxelsCount,
            voxels
        );

        _dataBuffer.SetData(transformMatrixArray);

        VoxelMaterial.SetBuffer("_TransformMatrixArray", _dataBuffer);
        VoxelMaterial.SetVector("_BoundsOffset", transform.position);

        transformMatrixArray.Dispose();
    }

    private void Update()
    {
        if (_totalSize == 0)
            return;

        Graphics.RenderMeshIndirect(
            _renderParams,
            voxelMesh,
            _drawArgsBuffer
        );
    }

    private void OnDestroy()
    {
        _drawArgsBuffer?.Release();
        _dataBuffer?.Release();
    }

    private static GraphicsBuffer CreateDrawArgsBufferForRenderMeshIndirect(Mesh mesh, int instanceCount)
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

    private static GraphicsBuffer CreateDataBuffer<T>(int instanceCount) where T : struct
    {
        return new GraphicsBuffer(
            GraphicsBuffer.Target.Structured, instanceCount,
            Marshal.SizeOf(typeof(T))
        );
    }
}