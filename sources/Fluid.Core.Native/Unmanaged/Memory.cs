﻿using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;

namespace Fluid.Core.Native.Unmanaged
{
    public static class Memory
    {
        [TargetedPatchingOptOut("Internal method only, inlined across NGen boundaries for performance reasons")]
        internal static unsafe void CopyUnmanagedMemory(byte* srcPtr, int srcOffset, byte* dstPtr, int dstOffset, int count)
        {
            srcPtr += srcOffset;
            dstPtr += dstOffset;
            Msvcrt.MemCpy(dstPtr, srcPtr, count);
        }

        [TargetedPatchingOptOut("Internal method only, inlined across NGen boundaries for performance reasons")]
        internal static void SetUnmanagedMemory(IntPtr dst, int filler, int count) => Msvcrt.MemSet(dst, filler, count);
    }
}
