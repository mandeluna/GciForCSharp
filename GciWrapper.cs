using System;
using System.Runtime.InteropServices;
using System.IO;

namespace GCI
{
    /// <summary>
    /// Struct representing GciErrSType from gci.hf
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct GciErrSType
    {
        public IntPtr category; // Using IntPtr for long to handle 32/64 bit differences
        public int number;
        public int argCount;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1025)]
        public string message;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public long[] args;
    }

    public class GciWrapper
    {
        // gcirtlobj is statically linked at compile time via the .csproj file
        // The P/Invoke declarations below will resolve to the linked object code
        private const string LibName = "libgcirpc"; // base name for the DLL to load

        static GciWrapper()
        {
            // Set up resolver for native libraries that gcirtlobj depends on
            // This allows gcirtlobj to find DLLs in the GEMSTONE/bin directory at runtime
            NativeLibrary.SetDllImportResolver(typeof(GciWrapper).Assembly, (libraryName, assembly, searchPath) =>
            {
                // For any unresolved native library, try to find it in GEMSTONE/bin
                string gemstonePath = Environment.GetEnvironmentVariable("GEMSTONE");
                if (string.IsNullOrEmpty(gemstonePath))
                {
                    return IntPtr.Zero;
                }

                string libDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "bin" : "lib";
                string binPath = Path.Combine(gemstonePath, libDirectory);
                
                // Search for DLL with wildcard to handle version numbers in filename
                // e.g., libgcirpc-3.7.4.3-64.dll
                string pattern = $"{libraryName}*.dll";
                string[] files = Directory.GetFiles(binPath, pattern);
                
                if (files.Length > 0)
                {
                    return NativeLibrary.Load(files[0]);
                }

                return IntPtr.Zero;
            });
        }

        #region P/Invoke Definitions
        
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GciInit();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void GciSetNet(string stoneName, string hostUserId, string hostPassword, string gemService);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern bool GciLogin(string gsUserName, string gsPassword);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GciErr(out GciErrSType errInfo);

        #endregion
    }
}