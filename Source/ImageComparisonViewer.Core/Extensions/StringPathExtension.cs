using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ImageComparisonViewer.Core.Extensions
{
    public static class StringPathExtension
    {
        /// <summary>
        /// サポート対象拡張子
        /// </summary>
        private readonly static string[] _supportedExtensions = new[]
        {
            ".jpg", ".jpeg",  // ".jpe", ".jfif", ".jfi", ".jif"
            ".bmp",
            ".png",
            ".tif", ".tiff",
        };

        /// <summary>
        /// 引数PATHが画像PATHとして有効か判定する
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns></returns>
        public static bool IsSupportedImagePath(this string sourcePath)
        {
            if (File.Exists(sourcePath))
                return IsSupportedImageFile(sourcePath);

            if (Directory.Exists(sourcePath))
                return GetImageFilesPathInDirectory(sourcePath, SearchOption.AllDirectories).Any();

            return false;
        }

        /// <summary>
        /// サポート画像の拡張子チェック
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns>true=サポート対象の画像ファイルである</returns>
        private static bool IsSupportedImageFile(this string sourcePath)
        {
            var extension = Path.GetExtension(sourcePath).ToLower();
            foreach (var supported in _supportedExtensions)
            {
                if (extension == supported) return true;
            }
            return false;
        }

        /// <summary>
        /// 入力PATHからディレクトリPATHに変換する(ファイルなら所属ディレクトリPATHに変換)
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns>ディレクトリPATH</returns>
        internal static string ToDirectoryPath(this string sourcePath)
        {
            if (Directory.Exists(sourcePath)) return sourcePath;

            if (File.Exists(sourcePath))
            {
                var path = Path.GetDirectoryName(sourcePath);  // DirRootならnullになるらしい(未確認)
                return (path is null) ? sourcePath : path;
            }

            throw new FileNotFoundException(sourcePath);
        }

        /// <summary>
        /// ディレクトリ内の先頭画像ファイルを返す(存在しなければnull)
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        internal static string GetFirstImageFilePathInDirectory(this string directoryPath, SearchOption searchOption)
        {
            return directoryPath.GetImageFilesPathInDirectory(searchOption)
                    .FirstOrDefault(p => p.IsSupportedImageFile());
        }

        /// <summary>
        /// ディレクトリ内の画像ファイルを取得する
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        internal static IEnumerable<string> GetImageFilesPathInDirectory(this string directoryPath, SearchOption searchOption)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException(directoryPath);

            try
            {
                return Directory.EnumerateFiles(directoryPath, "*", searchOption)
                    .Where(p => p.IsSupportedImageFile());
            }
            catch (UnauthorizedAccessException) { }
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// 引数ディレクトリ以下の画像を含むディレクトリを返す
        /// </summary>
        /// <param name="sourcePath"></param>
        internal static string ToImagePath(this string sourcePath)
        {
            //Debug.WriteLine($"Drag&Drop: {sourcePath}");
            string outputPath = default!;

            // ショートカットを実PATHに変換
            if (Path.GetExtension(sourcePath).ToLower() == ".lnk")
                sourcePath = ToShortCutDestinationPath(sourcePath);

            // ディレクトリなら先頭のJPEGを取得
            if (Directory.Exists(sourcePath))
            {
                // ドロップフォルダ内の画像PATHを取得してみる
                var imagePath = sourcePath.GetFirstImageFilePathInDirectory(SearchOption.TopDirectoryOnly);
                if (imagePath != null)
                {
                    outputPath = imagePath;
                }
                else
                {
                    // DCIMフォルダ突っ込んだら100_PANAを表示する
                    imagePath = sourcePath.GetFirstImageFilePathInDirectory(SearchOption.AllDirectories);
                    if (imagePath != null)
                        outputPath = imagePath;
                }
            }
            else if (File.Exists(sourcePath))
            {
                // サポート画像かチェックする
                if (sourcePath.IsSupportedImageFile())
                    outputPath = sourcePath;
            }
            else
            {
                throw new FileNotFoundException(sourcePath);
            }

            if (outputPath is null)
                throw new NullReferenceException(nameof(outputPath));

            return outputPath;
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
