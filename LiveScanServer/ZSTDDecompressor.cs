using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using size_t = System.UIntPtr;

namespace KinectServer
{
    static class ZSTDDecompressor
    {
        private const string dllName = "libzstd.dll";

        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong ZSTD_getDecompressedSize(IntPtr src, size_t srcSize);

        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern size_t ZSTD_decompress(IntPtr dst, size_t dstSize, 
            IntPtr src, size_t srcSize);

        public static byte[] Decompress(byte []array)
        {
            int size = array.Count();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(array, 0, ptr, size);

            int outSize = (int)ZSTD_getDecompressedSize(ptr, (size_t)size);
            
            IntPtr outPtr = Marshal.AllocHGlobal(outSize);
            ZSTD_decompress(outPtr, (size_t)outSize, ptr, (size_t)size);

            byte[] outArray = new byte[outSize];
            Marshal.Copy(outPtr, outArray, 0, outSize);
            
            Marshal.FreeHGlobal(ptr);
            Marshal.FreeHGlobal(outPtr);
            return outArray;
        }
    }
}
