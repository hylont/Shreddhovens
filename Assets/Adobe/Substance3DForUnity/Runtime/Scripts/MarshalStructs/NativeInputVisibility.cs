using System;
using System.Runtime.InteropServices;

namespace Adobe.Substance
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeInputVisibility
    {
        public IntPtr Index;

        public IntPtr IsVisible;
    }
}