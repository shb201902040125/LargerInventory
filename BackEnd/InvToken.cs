using SML.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Terraria;

namespace LargerInventory.BackEnd
{
    public class InvToken
    {
        private static object _lock = new();
        private static bool inLock = false;
        private static Queue<Action<Token>> _waitForTokens = [];
        public class Token
        {
            private DisposeWapper _disposeWapper;
            internal Token(DisposeWapper disposeWapper)
            {
                _disposeWapper = disposeWapper;
            }
            public void Return() => _disposeWapper.Dispose();
            public bool InValid => !(_disposeWapper?.Disposed ?? true);
        }
        public static bool TryGetToken(out Token token) => TryGetToken(TimeSpan.Zero, out token);
        public static bool TryGetToken(TimeSpan waitTime, out Token token)
        {
            if (Monitor.TryEnter(_lock, waitTime) && !inLock)
            {
                inLock = true;
                token = new(new(ReturnToken));
                return true;
            }
            token = null;
            return false;
        }
        public static void WaitForToken(Action<Token> whenGetToken)
        {
            if (whenGetToken is null)
            {
                return;
            }
            if (TryGetToken(out Token token))
            {
                whenGetToken(token);
                token.Return();
            }
            else
            {
                _waitForTokens.Enqueue(whenGetToken);
            }
        }

        private static void ReturnToken()
        {
            Monitor.Enter(_lock);
            inLock = false;
            if (_waitForTokens.TryDequeue(out Action<Token> waiter))
            {
                Token token = new(new(ReturnToken));
                Monitor.Exit(_lock);
                waiter(token);
            }
            else
            {
                Monitor.Exit(_lock);
            }
        }
    }
    public class DisposeWapper : IDisposable
    {
        private Action _dispose;
        public bool Disposed { get; private set; }
        public DisposeWapper(Action dispose)
        {
            _dispose = dispose;
        }
        ~DisposeWapper()
        {
            if (Disposed)
            {
                return;
            }
            _dispose?.Invoke();
            _dispose = null;
        }
        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }
            _dispose?.Invoke();
            _dispose = null;
            Disposed = true;
            GC.SuppressFinalize(this);
        }
    }
    public class DisposeWapper<T> : IDisposable
    {
        private Action<T> _dispose;
        public T Value { get; private set; }
        public bool Disposed { get; private set; }
        public DisposeWapper(T value, Action<T> dispose)
        {
            Value = value;
            _dispose = dispose;
        }
        ~DisposeWapper()
        {
            if (Disposed)
            {
                return;
            }
            _dispose?.Invoke(this);
            _dispose = null;
            Value = default;
        }
        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }
            _dispose?.Invoke(Value);
            _dispose = null;
            Value = default;
            Disposed = true;
            GC.SuppressFinalize(this);
        }
        public static implicit operator T(DisposeWapper<T> wapper)
        {
            return wapper.Value;
        }
    }
}