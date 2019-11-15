using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ICV.Control.ExplorerAddressBar
{
    public class DirectoryNode
    {
        // ディレクトリ略名の最大文字列(5文字を設定したら "dire..." となる)
        private const int _abbreviationNameLengthMax = 10;

        /// <summary>
        /// 対象ディレクトリのフルPATH
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// 対象ディレクトリのフルネーム
        /// </summary>
        public string FullName { get; } // ディレクトリ名

        /// <summary>
        /// 対象ディレクトリの略名
        /// </summary>
        public string AbbrName { get; } // Abbreviation=略語

        public DirectoryNode(string fullPath)
        {
            FullPath = fullPath;
            FullName = GetDirectoryName(fullPath);
            AbbrName = GetAbbreviationDirectoryName(FullName);
        }

        /// <summary>
        /// 子ディレクトリの存在チェックフラグ
        /// </summary>
        /// <returns></returns>
        public bool HasChildDirectory => GetChildDirectoryNodes(FullPath).Any();

        /// <summary>
        /// 引数pathディレクトリ内の子DirectoryNodeを返す
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DirectoryNode> GetChildDirectoryNodes() =>
            GetChildDirectoryNodes(FullPath);

        /// <summary>
        /// 存在するディレクトリに修正する
        /// </summary>
        /// <param name="basePath"></param>
        /// <returns></returns>
        //public static string GetExistsDirectoryPath(string basePath)
        //{
        //    // ディレクトリPATHを深い方から
        //    foreach (var dirPath in GetDirectoriesPath(basePath).Reverse())
        //    {
        //        if (Directory.Exists(dirPath)) return dirPath;
        //    }
        //    // 全ヒットしなければデスクトップに
        //    return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        //}

        /// <summary>
        /// FullPathを整形(ModelからViewModelに入ってくるPATH)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string EmendFullPathFromModel(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

            // 最終は "\" にする(手入力が入力楽なので)
            if (path[^1] != Path.DirectorySeparatorChar)
                return path + Path.DirectorySeparatorChar;

            return path;
        }

        /// <summary>
        /// FullPathを整形(ViewModelから出ていくPATH)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string EmendFullPathFromViewModel(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

            // 最終が "\" なら削除するが("C:\"なら削除しない)
            if (path[^1] == Path.DirectorySeparatorChar)
            {
                if (path[^2] == Path.VolumeSeparatorChar)
                    return path;

                return path.TrimEnd(Path.DirectorySeparatorChar);
            }
            return path;
        }

        /// <summary>
        /// フルパスから末端ディレクトリのView用の略名を取得する
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        private static string GetAbbreviationDirectoryName(string dirName)
        {
            var lengthMax = _abbreviationNameLengthMax;
            if (dirName.Length > lengthMax - 1)
                dirName = dirName.Substring(0, lengthMax - 1) + "...";
            return dirName;
        }

        /// <summary>
        /// ディレクトリのフルパスから末端ディレクトリ名を取得する
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        private static string GetDirectoryName(string fullPath) =>
            fullPath.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries)[^1];

        /// <summary>
        /// 引数pathディレクトリ内の子ディレクトリを返す
        /// </summary>
        /// <param name="basePath">基準ディレクトリPATH</param>
        /// <returns></returns>
        private static IEnumerable<DirectoryNode> GetChildDirectoryNodes(string basePath)
        {
            static IEnumerable<DirectoryNode> func(string path) =>
                Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly)
                    .Select(p => new DirectoryNode(p));

            return FuncToDirectory(basePath, func);
        }

        /// <summary>
        /// 引数pathディレクトリ内のPATHに対してFuncを実行
        /// </summary>
        /// <param name="basePath">基準ディレクトリPATH</param>
        /// <param name="action">Func</param>
        /// <returns></returns>
        private static IEnumerable<T> FuncToDirectory<T>(string basePath, Func<string, IEnumerable<T>> action)
        {
            // Exists()の中で、null/存在しないPATHもチェックしてくれる
            if (!Directory.Exists(basePath)) yield break;

            IEnumerable<T> items;
            try
            {
                items = action(basePath);
            }
            catch (UnauthorizedAccessException)
            {
                yield break;   // アクセス権のないフォルダにアクセスした場合は無視
            }
            foreach (var item in items)
                yield return item;
        }

        /// <summary>
        /// 引数ディレクトリからDirectoryNodeクラスをルートから順番に返す
        /// </summary>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public static IEnumerable<DirectoryNode> GetDirectoryNodes(string basePath) =>
            GetDirectoriesPath(basePath).Select(path => new DirectoryNode(path));

        /// <summary>
        /// 引数ディレクトリからDirectoryPATHをルートから順番に返す
        /// </summary>
        /// <param name="basePath"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetDirectoriesPath(string basePath)
        {
            if (string.IsNullOrEmpty(basePath)) yield break;

            var path = "";
            var dirNames = basePath.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var dirName in dirNames)
            {
                // "C:" になっちゃうので必ず最後に\付ける
                path = Path.Combine(path, dirName) + Path.DirectorySeparatorChar;
                yield return path;
            }
        }

    }
}
