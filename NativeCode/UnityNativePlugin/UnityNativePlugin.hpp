#define UNITY_NATIVE_PLUGIN_EXPORT_API __attribute__ ((visibility ("default")))

extern "C" {
    UNITY_NATIVE_PLUGIN_EXPORT_API int UnityNativePlugin();
}
