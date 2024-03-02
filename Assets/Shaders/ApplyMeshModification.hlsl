#define HAVE_MESH_MODIFICATION
  
AttributesMesh ApplyMeshModification(AttributesMesh input, uint SV_INSTANCEID)
{
    input.positionOS += float3(0.0f, SV_INSTANCEID * 500, 0.0f);
    return input;
}