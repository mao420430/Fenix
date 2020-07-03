﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace DotNetty.Transport.Libuv.Native
{
    using DotNetty.Common.Internal.Logging;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public abstract unsafe class NativeHandle : IDisposable
    {
        protected static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<NativeHandle>();
        private static readonly uv_close_cb CloseCallback = OnCloseHandle;
        internal readonly uv_handle_type HandleType;
        internal IntPtr Handle;

        internal NativeHandle(uv_handle_type handleType)
        {
            HandleType = handleType;
        }

        internal IntPtr LoopHandle()
        {
            Validate();
            return ((uv_handle_t*)Handle)->loop;
        }

        protected bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Handle != IntPtr.Zero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Validate()
        {
            if (!IsValid)
            {
                ThrowObjectDisposedException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void ThrowObjectDisposedException()
        {
            throw GetObjectDisposedException();
            ObjectDisposedException GetObjectDisposedException()
            {
                return new ObjectDisposedException($"{GetType()}");
            }
        }

        internal void RemoveReference()
        {
            Validate();
            NativeMethods.uv_unref(Handle);
        }

        internal bool IsActive => IsValid && NativeMethods.uv_is_active(Handle) > 0;

        internal void CloseHandle()
        {
            IntPtr handle = Handle;
            if (handle == IntPtr.Zero)
            {
                return;
            }

            int result = NativeMethods.uv_is_closing(handle);
            if (0u >= (uint)result)
            {
                NativeMethods.uv_close(handle, CloseCallback);
            }
        }

        protected virtual void OnClosed()
        {
            Handle = IntPtr.Zero;
        }

        static void OnCloseHandle(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                return;
            }

            NativeHandle nativeHandle = null;

            // Get gc handle first
            IntPtr pHandle = ((uv_handle_t*)handle)->data;
            if (pHandle != IntPtr.Zero)
            {
                GCHandle gcHandle = GCHandle.FromIntPtr(pHandle);
                if (gcHandle.IsAllocated)
                {
                    nativeHandle = gcHandle.Target as NativeHandle;
                    gcHandle.Free();

                    ((uv_handle_t*)handle)->data = IntPtr.Zero;
                }
            }

            // Release memory
            NativeMethods.FreeMemory(handle);
            nativeHandle?.OnClosed();
        }

        void Dispose(bool disposing)
        {
            try
            {
                if (IsValid)
                {
                    CloseHandle();
                }
            }
            catch (Exception exception)
            {
                Logger.ErrorWhilstClosingHandle(Handle, exception);
                // For finalizer, we cannot allow this to escape.
                if (disposing) throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NativeHandle() => Dispose(false);

        internal static T GetTarget<T>(IntPtr handle)
        {
            Debug.Assert(handle != IntPtr.Zero);

            IntPtr inernalHandle = ((uv_handle_t*)handle)->data;
            if (inernalHandle != IntPtr.Zero)
            {
                GCHandle gcHandle = GCHandle.FromIntPtr(inernalHandle);
                if (gcHandle.IsAllocated)
                {
                    return (T)gcHandle.Target;
                }
            }
            return default;
        }
    }
}
