using System;

namespace SharpGen.Runtime.Win32
{
    public sealed unsafe class WinRTString : CppObject
    {
        public string Value
        {
            get
            {
                var handle = NativePointer;
                if (handle == IntPtr.Zero)
                    return null;

                var buffer = (char*) WinRTStrings.WindowsGetStringRawBuffer(handle, out var length);
                return new string(buffer, 0, (int) length);
            }
        }

        public WinRTString(IntPtr pointer) : base(pointer)
        {
        }

        public WinRTString(string value) : base(WinRTStrings.WindowsCreateString(value))
        {
        }

        protected override void Dispose(bool disposing)
        {
            var handle = NativePointer;
            if (handle != IntPtr.Zero)
                WinRTStrings.WindowsDeleteString(handle);
            base.Dispose(disposing);
        }

        public override string ToString() => Value;
    }
}