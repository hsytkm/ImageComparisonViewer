using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace ImageComparisonViewer.Common.Extensions
{
    public static class StringShortCutLinkExtension
    {
        /// <summary>
        /// ショートカットから実PATHに取得するTryGetパターン
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="destPath"></param>
        /// <returns></returns>
        public static bool TryGetShortCutDestinationPath(this string srcPath, [NotNullWhen(true)]out string? destPath)
        {
            // ショートカットを実PATHに変換
            if (Path.GetExtension(srcPath).ToLower() == ".lnk")
            {
                destPath = ToShortCutDestinationPath(srcPath);
                return true;
            }

            destPath = default;
            return false;
        }

        /// <summary>
        /// ショートカットPATHからターゲットPATHを取得する
        /// </summary>
        /// <param name="source"></param>
        private static string ToShortCutDestinationPath(this string source)
        {
            // 参照の追加で「COM」の「Windows Script Host Object Model」をチェック

            // オブジェクトの生成、注意：WshShellClassでない
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();

            // ショートカットオブジェクトの生成、注意：キャストが必要
            IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(source);

            //System.Diagnostics.Trace.WriteLine($"ShortcutTargetPath: {shortcut.TargetPath}");
            return shortcut.TargetPath;
        }

    }
}
