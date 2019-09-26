using System;
using System.Collections.Generic;
using System.IO;

#if false
namespace ImageComparisonViewer.Core.IO
{
    /// <summary>
    /// 画像ファイルPATHの管理クラス
    /// </summary>
    public class ImageFiles
    {
        /// <summary>
        /// 画像拡張子
        /// </summary>
        private readonly static string[] _extensions = new[]
        {
            ".jpg", ".jpeg",  // ".jpe", ".jfif", ".jfi", ".jif"
            ".bmp",
            ".png",
            ".tif", ".tiff",
        };

        //public IReadOnlyList<string> SourcesPath { get; } = new List<string>();

        public ImageFiles(string directoryPath)
        {
            
        }

        /// <summary>
        /// 引数画像のディレクトリ内の画像を返す
        /// </summary>
        /// <param name="basePath">File or Directory PATH</param>
        /// <param name="allDirectories">サブディレクトリも含めるフラグ</param>
        /// <returns>対象画像のファイルPATH</returns>
        public static IEnumerable<string> GetImagesPath(string basePath, bool allDirectories = true)
        {
            string directoryPath;

            // ディレクトリPATHを取得
            if (File.Exists(basePath))
            {
                directoryPath = Path.GetDirectoryName(basePath);
            }
            else if (Directory.Exists(basePath))
            {
                directoryPath = basePath;
            }
            else
            {
                yield break;
            }

            var searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            var files = Directory.EnumerateFiles(directoryPath, "*", searchOption);
            foreach (var file in files)
            {
                foreach (var extension in _extensions)
                {
                    if (file.EndsWith(extension, StringComparison.CurrentCultureIgnoreCase))
                        yield return file;
                }
            }
        }

    }
}
#endif
