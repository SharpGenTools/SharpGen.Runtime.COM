using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SharpGen.Runtime
{
    public static class ComMarshallingHelpers
    {
        /// <summary>
        /// Instantiate a CppObject from a native pointer.
        /// </summary>
        /// <typeparam name="T">The CppObject class that will be returned</typeparam>
        /// <param name="cppObjectPtr">The native pointer to a com object.</param>
        /// <returns>An instance of T binded to the native pointer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T FromPointer<T>(IntPtr cppObjectPtr) where T : CppObject
        {
            return MarshallingHelpers.FromPointer<T>(cppObjectPtr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr ToCallbackPtr<TCallback>(ComObject comObject)
            where TCallback : ICallbackable
        {
            if (comObject == null)
                return IntPtr.Zero;
            // do not create extra object in case of the same type
            if (comObject.GetType() != typeof(TCallback))
            {
                var interfacePtr = comObject.QueryInterfaceOrNull(typeof (TCallback).GetTypeInfo().GUID);
                var newInterface = (TCallback)Activator.CreateInstance(typeof(TCallback), interfacePtr);
                // New pointer given from QI has one extra reference, so we need to release it, but we also need to create a holder for this pointer to make proper AddRef/Release in finalizer
                if (!(newInterface is ComObject newComObject)) 
                    throw new InvalidOperationException("Created object is now COM object");
                newComObject.Release();
                return newComObject.NativePointer;
            }
            return comObject.NativePointer;
        }

        /// <summary>
        /// Return the unmanaged C++ pointer from a <see cref="SharpGen.Runtime.ICallbackable"/> instance.
        /// </summary>
        /// <typeparam name="TCallback">The type of the callback.</typeparam>
        /// <param name="callback">The callback.</param>
        /// <returns>A pointer to the unmanaged C++ object of the callback</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr ToCallbackPtr<TCallback>(ICallbackable callback)
            where TCallback : ICallbackable
        {
            // we have to make QI to normalize pointer when passing COM object into COM call
            if (callback is ComObject comObject)
            {
                return ToCallbackPtr<TCallback>(comObject);
            }
            return MarshallingHelpers.ToCallbackPtr<TCallback>(callback);
        }

        /// <summary>
        /// Return the unmanaged C++ pointer from a <see cref="SharpGen.Runtime.CppObject"/> instance.
        /// </summary>
        /// <typeparam name="TCallback">The type of the callback.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>A pointer to the unmanaged C++ object of the callback</returns>
        /// <remarks>This method is meant as a fast-path for codegen to use to reduce the number of casts.</remarks>
        public static IntPtr ToCallbackPtr<TCallback>(CppObject obj)
            where TCallback : ICallbackable
        {
            return ToCallbackPtr<TCallback>((ICallbackable) obj);
        }

        /// <summary>
        /// Makes additional transformation for the object which was unmarshalled from unmanaged code 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="isOutParam">Indicates that the object was obtained via 'Obj**' parameter</param>
        /// <returns>Transformed object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T TransformObjectFromUnmanaged<T>(T obj, bool isOutParam) where T : CppObject
        {
            // objects returned via Obj** has one extra AddRef, so we need to call Release to balance references 
            if (isOutParam && obj is ComObject comObject)
                comObject.Release();
            return obj;
        }

    }
}