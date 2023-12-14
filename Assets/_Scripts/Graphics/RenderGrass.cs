using UnityEngine;

public class RenderGrass : MonoBehaviour
{
    public Material material;
    public Mesh mesh;
    public uint _numberOfInstances;
    
    GraphicsBuffer commandBuf;
    GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    const int commandCount = 1;

    private struct GrassData
    {
        public Vector4 position;
        public Vector2 uv;
        public float displacement;
    }
    
    private struct GrassChunk {
        public ComputeBuffer argsBuffer;
        // public ComputeBuffer argsBufferLOD;
        public ComputeBuffer positionsBuffer;
        // public ComputeBuffer culledPositionsBuffer;
        public Bounds bounds;
        public Material material;
    }
    
    void Start()
    {
        commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];
    }

    void OnDestroy()
    {
        commandBuf?.Release();
        commandBuf = null;
    }

    private void InitializeChunk() {
        GrassChunk chunk = new GrassChunk();

        chunk.argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        chunk.argsBuffer.SetData(args);
    }

    void Update()
    {
        RenderParams rp = new RenderParams(material);
        rp.worldBounds = new Bounds(Vector3.zero, 100000*Vector3.one); // use tighter bounds for better FOV culling
        rp.matProps = new MaterialPropertyBlock();
        rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(-4.5f, 0, 0)));
        rp.matProps.SetInt("_numberOfInstances", (int)_numberOfInstances);
        commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)Mathf.Pow(_numberOfInstances, 3);

        commandBuf.SetData(commandData);
        Graphics.RenderMeshIndirect(rp, mesh, commandBuf);
    }
}