using SML.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LargerInventory.BackEnd
{
    public class InvToken
    {
        static object _lock = new();
        static bool inLock = false;
        static Queue<Action<Token>> _waitForTokens = [];
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
            if (TryGetToken(out var token))
            {
                whenGetToken(token);
            }
            else
            {
                _waitForTokens.Enqueue(whenGetToken);
            }
        }
        static void ReturnToken()
        {
            Monitor.Enter(_lock);
            inLock = false;
            if (_waitForTokens.TryDequeue(out var waiter))
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