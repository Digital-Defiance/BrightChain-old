using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BrightChain.Helpers
{
    public sealed class MemoryComparer<T> : IEqualityComparer<Memory<T>>, IComparer<Memory<T>>
        where T : IEquatable<T>, IComparable<T>
    {
        /// <summary> returns true if both arrays contain the exact same set of bytes. </summary>
        public static bool Equals(Memory<T> ar1, Memory<T> ar2) => 0 == Compare(ar1, ar2);

        /// <summary> Compares the contents of the byte arrays and returns the result. </summary>
        public static int Compare(Memory<T> ar1, Memory<T> ar2)
        {
            if (ar1.IsEmpty)
            {
                return ar2.IsEmpty ? 0 : -1;
            }

            if (ar2.IsEmpty)
            {
                return 1;
            }

            int result = 0;
            int i = 0, stop = Math.Min(ar1.Length, ar2.Length);

            for (; 0 == result && i < stop; i++)
            {
                T a = ar1.Slice(i).ToArray()[0];
                T b = ar2.Slice(i).ToArray()[0];
                result = a.CompareTo(b);
            }

            if (result != 0)
            {
                return result;
            }

            if (i == ar1.Length)
            {
                return i == ar2.Length ? 0 : -1;
            }

            return 1;
        }

        /// <summary> Returns a hash code the instance of the object </summary>
        public static int GetHashCode(Memory<T> memoryT)
        {
            var tArray = memoryT.ToArray();

            var size = Marshal.SizeOf(tArray);
            if (size == 0)
            {
                return 0;
            }

            // Both managed and unmanaged buffers required.
            var bytes = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);
            // Copy object byte-to-byte to unmanaged memory.
            Marshal.StructureToPtr(tArray, ptr, false);
            // Copy data from unmanaged memory to managed buffer.
            Marshal.Copy(ptr, bytes, 0, size);
            // Release unmanaged memory.
            Marshal.FreeHGlobal(ptr);

            var crc32Instance = new Crc32();
            return (int)crc32Instance.ComputeChecksum(bytes);
        }


        /// <summary> Compares the contents of the byte arrays and returns the result. </summary> 
        int IComparer<Memory<T>>.Compare(Memory<T> x, Memory<T> y) => MemoryComparer<T>.Compare(x, y);

        /// <summary> Returns true if the two objects are the same instance </summary>
        bool IEqualityComparer<Memory<T>>.Equals(Memory<T> x, Memory<T> y) => 0 == MemoryComparer<T>.Compare(x, y);

        /// <summary> Returns a hash code the instance of the object </summary>
        int IEqualityComparer<Memory<T>>.GetHashCode(Memory<T> bytes) => MemoryComparer<T>.GetHashCode(bytes);
    }
}