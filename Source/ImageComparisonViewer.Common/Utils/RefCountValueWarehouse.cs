using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

// C#の排他制御 - UpgradeableReadLock
// https://days-of-programming.blogspot.com/2017/11/c-upgradeablereadlock.html
namespace ImageComparisonViewer.Common.Utils
{
    public class RefCountValuePair<TValue> : IDisposable
        where TValue : class
    {
        /// <summary>TValueの参照数(1以上)</summary>
        private int _refCounter;

        /// <summary>Dispose後だけnull</summary>
        private TValue? _value;

        public RefCountValuePair(TValue value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            _refCounter = 1;
            _value = value;
        }

        public void IncrementReferenceCounter() =>
            Interlocked.Increment(ref _refCounter);

        public int DecrementReferenceCounter()
        {
            Interlocked.Decrement(ref _refCounter);
            return _refCounter;
        }

        /// <summary>ForDebug</summary>
        public int GetRefCounter() => _refCounter;

        public TValue GetValue()
        {
            if (_value != null) return _value;
            throw new NullReferenceException(nameof(_value));
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    /* TODO: マネージ状態を破棄します (マネージ オブジェクト) */
                    if (_value is IDisposable d) d.Dispose();
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。
                _value = null;

                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion
    }

    public class RefCountValueWarehouse<TKey, TValueType> : IDisposable
        where TKey : notnull
        where TValueType : class
    {
        private readonly Dictionary<TKey, RefCountValuePair<TValueType>> _keyValues =
            new Dictionary<TKey, RefCountValuePair<TValueType>>();

        private readonly ReaderWriterLockSlim _rwlock = new ReaderWriterLockSlim();

        /// <summary>
        /// 辞書にデータがあれば取得(存在しなければnull)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private TValueType? GetValueFromDictionary(TKey key)
        {
            if (_keyValues.TryGetValue(key, out var pair))
            {
                pair.IncrementReferenceCounter();
                //Debug.WriteLine($"GetValue(n): Count={pair.GetRefCounter()} Key={key}");
                return pair.GetValue();
            }
            return default;
        }

        /// <summary>
        /// 辞書にデータがあれば取得(存在しなければnull)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValueType? RentValueIfExist(TKey key)
        {
            try
            {
                _rwlock.EnterReadLock();
                return GetValueFromDictionary(key);
            }
            finally
            {
                _rwlock.ExitReadLock();
            }
        }

        /// <summary>
        /// 辞書からデータを取得(辞書に存在しなければ作成して登録)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public TValueType RentValue(TKey key, Func<TValueType?> func)
        {
            try
            {
                _rwlock.EnterUpgradeableReadLock();

                var value = GetValueFromDictionary(key);
                if (value != default) return value;

                // Create -> Add
                var newValue = func.Invoke();
                if (newValue is null) throw new NullReferenceException("warehouse value isn't allowed null");
                var newPair = new RefCountValuePair<TValueType>(newValue);

                _keyValues.Add(key, newPair);
                //Debug.WriteLine($"RentValue(1): Count={newPair.GetRefCounter()} Key={key}");
                return newValue;
            }
            finally
            {
                _rwlock.ExitUpgradeableReadLock();
            }
        }

        public TValueType RentValueAsync(TKey key, Func<TValueType?> func)
        {
            // 非同期：awaitを含むコードをロックするには？（SemaphoreSlim編）
            // https://www.atmarkit.co.jp/ait/articles/1411/11/news117.html
            throw new NotImplementedException();
        }

        /// <summary>
        /// データ参照の解除通知(参照がゼロになれば辞書から削除)
        /// </summary>
        /// <param name="key"></param>
        public void ReturnValue(TKey key)
        {
            try
            {
                _rwlock.EnterUpgradeableReadLock();

                if (!_keyValues.TryGetValue(key, out var pair))
                {
                    // 返却時に辞書にデータ存在しないは有り得ない
                    throw new KeyNotFoundException(nameof(key));
                }

                // 参照がなくなれば辞書から削除
                var refCounter = pair.DecrementReferenceCounter();
                if (refCounter <= 0)
                {
                    pair.Dispose();
                    _keyValues.Remove(key);
                }
                //Debug.WriteLine($"ReturnValue: Count={refCounter} Key={key}");
            }
            finally
            {
                _rwlock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>辞書データの全削除</summary>
        private void Clear()
        {
            try
            {
                _rwlock.EnterWriteLock();

                foreach (var pair in _keyValues.Values)
                {
                    pair.Dispose();
                }
                _keyValues.Clear();
            }
            finally
            {
                _rwlock.ExitWriteLock();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    /* TODO: マネージ状態を破棄します (マネージ オブジェクト) */
                    Clear();
                    _rwlock.Dispose();
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion

    }
}
