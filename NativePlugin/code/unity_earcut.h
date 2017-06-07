#define UNITY_NATIVE_PLUGIN_EXPORT_API __attribute__ ((visibility ("default")))

extern "C" {
    UNITY_NATIVE_PLUGIN_EXPORT_API unsigned int CreateTesselationContext();
    UNITY_NATIVE_PLUGIN_EXPORT_API void ReleaseTesselationContext(unsigned int context);
    UNITY_NATIVE_PLUGIN_EXPORT_API void TesselatePolygon(unsigned int context, char* pointsBuffer, char* ringsBuffer, int nRings, unsigned int& nIndices, unsigned int& nVertices);
    UNITY_NATIVE_PLUGIN_EXPORT_API void GetIndices(unsigned int context, char* indices);
    UNITY_NATIVE_PLUGIN_EXPORT_API void GetVertices(unsigned int context, char* vertices);
}
