using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance
{
    /// <summary>
    /// Struct for handlign sending and receiving preset XML from native to managed code.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativePreset
    {
        public IntPtr XMLString;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativePresetInfo
    {
        public IntPtr mPackageUrl;
        public IntPtr mLabel;
        public IntPtr mDescription;
    }
}