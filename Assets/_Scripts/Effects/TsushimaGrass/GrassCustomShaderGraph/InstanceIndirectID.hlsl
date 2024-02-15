// Obtain InstanceID. e.g. Can be used as a Seed into Random Range node to generate random data per instance
void GetInstanceID_float(out float Out){
    Out = 0;
    #ifndef SHADERGRAPH_PREVIEW
    Out = InstanceID();
    #endif
}