﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DotNetty.Common.Utilities;

namespace DotNetty.Common.Concurrency
{
    public class TaskCompletionSource : IPromise
    {
        private readonly TaskCompletionSource<int> _tcs;
        private int v_uncancellable = SharedConstants.False;

        public TaskCompletionSource()
        {
            _tcs = new TaskCompletionSource<int>();
        }

        public TaskCompletionSource(object state)
        {
            _tcs = new TaskCompletionSource<int>(state);
        }

        public Task Task
        {
            [MethodImpl(InlineMethod.AggressiveOptimization)]
            get => _tcs.Task;
        }

        public bool IsVoid => false;

        public bool IsSuccess => Task.IsSuccess();

        public bool IsCompleted => Task.IsCompleted;

        public bool IsFaulted => Task.IsFaulted;

        public bool IsCanceled => Task.IsCanceled;

        public virtual bool TryComplete()
        {
            return _tcs.TrySetResult(0);
        }

        public virtual void Complete()
        {
            _tcs.SetResult(0);
        }
        public virtual void SetCanceled()
        {
            if (SharedConstants.False < (uint)Volatile.Read(ref v_uncancellable)) { return; }
            _tcs.SetCanceled();
        }

        public virtual void SetException(Exception exception)
        {
            if (exception is AggregateException aggregateException)
            {
                SetException(aggregateException.InnerExceptions);
                return;
            }
            _tcs.SetException(exception);
        }

        public virtual void SetException(IEnumerable<Exception> exceptions)
        {
            _tcs.SetException(exceptions);
        }

        public virtual bool TrySetCanceled()
        {
            if (SharedConstants.False < (uint)Volatile.Read(ref v_uncancellable)) { return false; }
            return _tcs.TrySetCanceled();
        }

        public virtual bool TrySetException(Exception exception)
        {
            if (exception is AggregateException aggregateException)
            {
                return TrySetException(aggregateException.InnerExceptions);
            }
            return _tcs.TrySetException(exception);
        }

        public virtual bool TrySetException(IEnumerable<Exception> exceptions)
        {
            return _tcs.TrySetException(exceptions);
        }

        public bool SetUncancellable()
        {
            if (SharedConstants.False >= (uint)Interlocked.CompareExchange(ref v_uncancellable, SharedConstants.True, SharedConstants.False))
            {
                return true;
            }
            return !IsCompleted;
        }

        public override string ToString() => "TaskCompletionSource[status: " + Task.Status.ToString() + "]";

        public IPromise Unvoid() => this;
    }
}