using System;
using System.IO;
using System.Linq;

namespace ImageComparisonViewer.Core.Extensions
{
    static class StringPathExtension
    {

        /// <summary>
        /// 入力PATHからディレクトリPATHを取得する
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns></returns>
        public static string GetDirectoryPath(this string sourcePath)
        {
            if (Directory.Exists(sourcePath))
                return sourcePath;

            if (File.Exists(sourcePath))
            {
                var path = Path.GetDirectoryName(sourcePath);  // DirRootならnullになるらしい(未確認)
                return (path is null) ? sourcePath : path;
            }

            throw new FileNotFoundException(sourcePath);
        }

        /// <summary>
        /// 入力PATHからファイルPATHを取得する
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns></returns>
        public static string GetFilePath(this string sourcePath)
        {
            if (File.Exists(sourcePath))
                return sourcePath;

            // ディレクトリなら先頭ファイル
            if (Directory.Exists(sourcePath))
            {
                var path = Directory.EnumerateFiles(sourcePath, "*.jpg", SearchOption.TopDirectoryOnly).FirstOrDefault();
                return (path is null) ? sourcePath : path;
            }

            throw new FileNotFoundException(sourcePath);
        }

        /// <summary>
        /// ディレクトリ内の先頭ファイルを返す
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public static string? GetFirstFilePathInDirectory(this string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException(directoryPath);

            // ◆拡張子に未対応
            return Directory.EnumerateFiles(directoryPath, "*.jpg", SearchOption.TopDirectoryOnly).FirstOrDefault();
        }

    }
}
