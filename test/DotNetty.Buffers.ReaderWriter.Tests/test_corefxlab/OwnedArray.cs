﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace DotNetty.Buffers
{
    public class OwnedArray<T> : MemoryManager<T>
    {
        T[] _array;
        int _referenceCount;

        public OwnedArray(int length)
        {
            _array = new T[length];
        }

        public OwnedArray(T[] array)
        {
            if (array == null) ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
            _array = array;
        }

        public static implicit operator OwnedArray<T>(T[] array) => new OwnedArray<T>(array);

        public override Span<T> GetSpan()
        {
            if (IsDisposed) ThrowObjectDisposedException();
            return new Span<T>(_array);
        }

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            unsafe
            {
                Retain();
                if ((uint)elementIndex > (uint)_array.Length) ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.elementIndex);
                var handle = GCHandle.Alloc(_array, GCHandleType.Pinned);
                void* pointer = Unsafe.Add<T>((void*)handle.AddrOfPinnedObject(), elementIndex);
                return new MemoryHandle(pointer, handle, this);
            }
        }

        protected override bool TryGetArray(out ArraySegment<T> arraySegment)
        {
            if (IsDisposed) ThrowObjectDisposedException();
            arraySegment = new ArraySegment<T>(_array);
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            _array = null;
        }

        public void Retain()
        {
            if (IsDisposed) ThrowObjectDisposedException();
            Interlocked.Increment(ref _referenceCount);
        }

        public override void Unpin()
        {
            int newRefCount = Interlocked.Decrement(ref _referenceCount);
            if (newRefCount < 0) throw new InvalidOperationException();
            if (newRefCount == 0)
            {
                OnNoReferences();
            }
        }

        protected virtual void OnNoReferences()
        {
        }

        protected bool IsRetained => _referenceCount > 0;

        public bool IsDisposed => _array == null;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowObjectDisposedException()
        {
            throw GetObjectDisposedException();
            ObjectDisposedException GetObjectDisposedException()
            {
                return new ObjectDisposedException(nameof(OwnedArray<T>));
            }
        }
    }
}
