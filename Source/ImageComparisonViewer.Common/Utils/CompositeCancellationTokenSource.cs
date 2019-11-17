using ImageComparisonViewer.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ImageComparisonViewer.Common.Utils
{
    /// <summary>
    /// 複数の処理要求があった場合、最後の1つのみを処理する
    /// </summary>
    public class CompositeCancellationTokenSource : IDisposable
    {
        private readonly IList<CancellationTokenSource> _tokenSources = new List<CancellationTokenSource>();

        private CancellationTokenSource GetCancellationTokenSource()
        {
            // 処理中のものはキャンセル
            _tokenSources.ForEach(ts => ts.Cancel());

            var tokenSource = new CancellationTokenSource();
            _tokenSources.Add(tokenSource);
            return tokenSource;
        }

        public CancellationToken GetCancellationToken() => GetCancellationTokenSource().Token;

        public void Clear()
        {
            _tokenSources.ForEach(cts => cts.Dispose());
            _tokenSources.Clear();
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                this.Clear();

                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion

    }
}
