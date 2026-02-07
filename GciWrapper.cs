using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading.Tasks;

namespace GCI
{
    public class GciConfig
    {
        public string StoneName { get; set; } = "stone";
        public string HostUserId { get; set; }
        public string HostPassword { get; set; }
        public string GemService { get; set; } = "gemnetobject";
        public string GsUserName { get; set; } = "SystemUser";
        public string GsPassword { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct GciErrSType
    {
        public IntPtr category; 
        public int number;
        public int argCount;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1025)]
        public string message;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public long[] args;
    }

    public class GciWrapper
    {
        private const string LibName = "libgcirpc";
        
        // Common GemStone Constants
        public const long OOP_NIL = 20; // 0x14
        public const long OOP_CLASS_STRING = 74;

        static GciWrapper()
        {
            NativeLibrary.SetDllImportResolver(typeof(GciWrapper).Assembly, (libraryName, assembly, searchPath) =>
            {
                string gemstonePath = Environment.GetEnvironmentVariable("GEMSTONE");
                if (string.IsNullOrEmpty(gemstonePath)) return IntPtr.Zero;

                string libDirectory = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "bin" : "lib";
                string binPath = Path.Combine(gemstonePath, libDirectory);
                
                string ext = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".dll" : ".so";
                string pattern = $"{libraryName}*{ext}";
                string[] files = Directory.GetFiles(binPath, pattern);
                
                if (files.Length > 0) return NativeLibrary.Load(files[0]);
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

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void GciLogout();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GciErr(out GciErrSType errInfo);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern long GciNewString(string str);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern long GciExecute(long sourceStringOop, long symbolListOop);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GciOopToI32_(long oop, out bool error);

        #endregion

        #region Synchronous API
        
        /// <summary>
        /// Synchronously executes Smalltalk source code.
        /// </summary>
        public static long Execute(string sourceString)
        {
            long oString = GciNewString(sourceString);
            return GciExecute(oString, OOP_NIL);
        }

        #endregion

        #region Task-Based Async API (Promises)

        /// <summary>
        /// Executes Smalltalk source code asynchronously.
        /// </summary>
        public static Task<long> ExecuteAsync(string sourceString)
        {
            return Task.Run(() => Execute(sourceString));
        }

        /// <summary>
        /// Executes Smalltalk source and returns the result as a C# integer asynchronously.
        /// </summary>
        public static async Task<int> ExecuteAndConvertAsync(string sourceString)
        {
            return await Task.Run(() =>
            {
                long resultOop = Execute(sourceString);
                
                if (resultOop == OOP_NIL)
                {
                    if (GciErr(out GciErrSType err))
                        throw new Exception($"GemStone Error {err.number}: {err.message}");
                    
                    throw new Exception("GemStone Execution returned OOP_NIL without specific error info.");
                }

                int val = GciOopToI32_(resultOop, out bool conversionErr);
                if (conversionErr)
                    throw new InvalidCastException("Result of Smalltalk execution could not be converted to a 32-bit integer.");

                return val;
            });
        }

        #endregion
    }
}