// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Runtime.InteropServices;
using SharpGen.Runtime.Win32;

namespace SharpGen.Runtime
{
    public unsafe class InspectableVtbl : ComObjectVtbl
    {
        public InspectableVtbl(int numberOfCallbackMethods) : base(numberOfCallbackMethods + 3)
        {
            AddMethod(new GetIidsDelegate(GetIids), 3);
            AddMethod(new GetRuntimeClassNameDelegate(GetRuntimeClassName), 4);
            AddMethod(new GetTrustLevelDelegate(GetTrustLevel), 5);
        }

        /// <unmanaged>
        /// HRESULT STDMETHODCALLTYPE GetIids(
        ///   /* [out] */ __RPC__out ULONG *iidCount,
        ///   /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
        /// )
        /// </unmanaged>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetIidsDelegate(IntPtr thisPtr, int* iidCount, IntPtr* iids);

        private static int GetIids(IntPtr thisPtr, int* iidCount, IntPtr* iids)
        {
            try
            {
                var shadow = ToShadow<InspectableShadow>(thisPtr);
                var callback = (IInspectable) shadow.Callback;

                var container = callback.Shadow;

                var countGuids = container.Guids.Length;

                // Copy GUIDs deduced from Callback
                iids = (IntPtr*) Marshal.AllocCoTaskMem(IntPtr.Size * countGuids);
                *iidCount = countGuids;

                MemoryHelpers.CopyMemory(iids, new ReadOnlySpan<IntPtr>(container.Guids));
            }
            catch (Exception exception)
            {
                return (int) Result.GetResultFromException(exception);
            }

            return Result.Ok.Code;
        }

        /// <unmanaged>
        /// HRESULT STDMETHODCALLTYPE GetRuntimeClassName([out] __RPC__deref_out_opt HSTRING *className)
        /// </unmanaged>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetRuntimeClassNameDelegate(IntPtr thisPtr, IntPtr* className);

        private static int GetRuntimeClassName(IntPtr thisPtr, IntPtr* className)
        {
            try
            {
                var shadow = ToShadow<InspectableShadow>(thisPtr);
                var callback = (IInspectable) shadow.Callback;

                // Use the name of the callback class
                var name = callback.GetType().FullName;

                *className = WinRTStrings.WindowsCreateString(name);
            }
            catch (Exception exception)
            {
                return (int) Result.GetResultFromException(exception);
            }

            return Result.Ok.Code;
        }

        /// <unmanaged>
        /// HRESULT STDMETHODCALLTYPE GetTrustLevel(/* [out] */ __RPC__out TrustLevel *trustLevel);
        /// </unmanaged>
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetTrustLevelDelegate(IntPtr thisPtr, int* trustLevel);

        private static int GetTrustLevel(IntPtr thisPtr, int* trustLevel)
        {
            try
            {
                // Write full trust
                *trustLevel = (int) TrustLevel.FullTrust;
            }
            catch (Exception exception)
            {
                return (int) Result.GetResultFromException(exception);
            }

            return Result.Ok.Code;
        }
    }
}