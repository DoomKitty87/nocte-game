#define HAVE_MESH_MODIFICATION
  
AttributesMesh ApplyMeshModification(AttributesMesh input, float3 timeParameters)
{
    input.positionOS += float3(0.5f, 0.5f, 0.5f);
    return input;
}