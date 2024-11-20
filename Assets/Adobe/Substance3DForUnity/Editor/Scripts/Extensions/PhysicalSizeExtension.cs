using Adobe.Substance;

namespace Adobe.SubstanceEditor
{
    internal static class PhysicalSizeExtension
    {
        public static bool IsSupported()
        {
            return PluginPipelines.IsHDRP();
        }
    }
}