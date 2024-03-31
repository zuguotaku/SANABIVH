namespace SevenZip
{
    using System;
    using System.Runtime.InteropServices;

#if UNMANAGED
    internal static class NativeMethods
    {
        [DllImport("7z.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int CreateObject(in Guid classID, in Guid interfaceID, [MarshalAs(UnmanagedType.Interface)] out object outObject);

        public static T SafeCast<T>(PropVariant var, T def)
        {
            object obj;
            
            try
            {
                obj = var.Object;
            }
            catch (Exception)
            {
                return def;
            }

            if (obj is T expected)
            {
                return expected;
            }
            
            return def;
        }
    }
#endif
}