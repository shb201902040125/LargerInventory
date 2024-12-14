using SML.Common;
using System;
using System.Collections.Generic;
using System.Threading;

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
}