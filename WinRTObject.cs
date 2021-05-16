using System;
using System.Runtime.InteropServices;
using SharpGen.Runtime.Win32;

namespace SharpGen.Runtime
{
    public unsafe partial class WinRTObject
    {
        public Guid[] Iids
        {
            get
            {
                IntPtr iids = default;
                GetIids(out var count, (IntPtr)(&iids));
                var iid = new Guid[count];
                MemoryHelpers.Read<Guid>(iids, iid, (int) count);
                Marshal.FreeCoTaskMem(iids);
                return iid;
            }
        }

        public string RuntimeClassName
        {
            get
            {
                GetRuntimeClassName(out var nativeStringPtr);
                using WinRTString nativeString = new(nativeStringPtr);
                return nativeString.Value;
            }
        }
    }
}