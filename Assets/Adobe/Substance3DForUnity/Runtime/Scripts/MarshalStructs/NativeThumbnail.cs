using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeThumbnail
    {
        public IntPtr Size;
        public IntPtr Data;
    }
}