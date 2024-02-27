using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRenderObjectsIndirect : MonoBehaviour
{
    public Mesh mesh;
    public Material mat;

    public int length = 10;
    public float distance = 2.5f;

    public int chunks = 1;
    public float chunksDistance = 50;
    
    private Vector4[,] positions;

    private int commandCount;
    
    GraphicsBuffer commandBuf;
    GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    private static readonly int ObjectToWorld = Shader.PropertyToID("_ObjectToWorld");
    private static readonly int Positions = Shader.PropertyToID("_Positions");

    private RenderParams rp;

    void Start() {

        positions = new Vector4[chunks, length * length * length];

        for (int n = 0; n < chunks; n++) {
            int count = 0;
            for (int i = 0; i < length; i++) {
                for (int j = 0; j < length; j++) {
                    for (int k = 0; k < length; k++) {
                        positions[n, count] = new Vector3(i * distance + n * chunksDistance, j * distance, k * distance);
                        count++;
                    }
                }
            }
        }

        commandCount = positions.Length;
        
        commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        // commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];
        
        rp = new RenderParams(mat);
        rp.matProps = new MaterialPropertyBlock();
        rp.matProps.SetMatrix(ObjectToWorld, Matrix4x4.Translate(Vector3.zero));
        // rp.matProps.SetVectorArray(Positions, positions);
        commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)commandCount;
        commandBuf.SetData(commandData);
    }

    void OnDestroy()
    {
        commandBuf?.Release();
        commandBuf = null;
    }

    void Update()
    {
        Graphics.RenderMeshIndirect(rp, mesh, commandBuf, commandCount);
    }
}
