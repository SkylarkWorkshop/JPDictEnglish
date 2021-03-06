﻿using JapaneseDict.Models;
using SQLite.Net;
using SQLite.Net.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace JapaneseDict.QueryEngine
{
    public class QueryEngine
    {

        public static class MainDictQueryEngine
        {
            private static SQLiteConnection _conn = new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), Path.Combine(ApplicationData.Current.LocalFolder.Path, "dict.db"));
            /// <summary>
            /// Query MainDict database using the given keyword
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public static IEnumerable<MainDict> Query(string key)
            {
                return _conn.Query<MainDict>("SELECT * FROM MainDict WHERE JpChar = ?", key);
            }
            /// <summary>
            /// Fuzzy query MainDict database using the given keyword
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public static IEnumerable<MainDict> FuzzyQuery(string key)
            {

                return _conn.Query<MainDict>("SELECT * FROM MainDict WHERE JpChar LIKE ?", key + "%");

            }
            /// <summary>
            /// Query MainDict database using the given keyword and return the result in ObservableCollection type
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public static async Task<ObservableCollection<MainDict>> QueryForUIAsync(string key)
            {
                return await Task.Run(() =>
                {
                    if (!(string.IsNullOrEmpty(key)))
                    {
                        var result = new ObservableCollection<MainDict>(_conn.Query<MainDict>("SELECT * FROM MainDict WHERE JpChar = ?", key.Replace(" ", "").Replace("　", "")));
                        if (result.Count != 0)
                        {
                            return result;
                        }
                        else
                        {
                            var err = new ObservableCollection<MainDict>();
                            err.Add(new MainDict() { JpChar = key, Explanation = "没有本地释义" });
                            return err;
                        }
                    }
                    else
                    {
                        return new ObservableCollection<MainDict>();
                    }
                });

            }
            public static async Task<ObservableCollection<MainDict>> QueryCn2JpForUIAsync(string key)
            {
                return await Task.Run(() =>
                {
                    if (!(string.IsNullOrEmpty(key)))
                    {
                        //@"\[[^\]]+\]" Regex pat for removing [XXX]
                        //@"[\（][\s\S]*[\）]" Regex pat for removing （XXX）
                        var queryres = _conn.Table<MainDict>().Select(s => s).Where(w =>
                        {
                            string a = Regex.Replace(Regex.Replace(w.PreviewExplanation, @"\[[^\]]+\]", ""), @"[\（][\s\S]*[\）]", "");
                            if (a.Contains(key + "；") || a.Contains(key + "，") || w.PreviewExplanation.EndsWith("] " + key + " ..."))
                            {
                                return true;
                            }
                            return false;
                        });
                        var result = new ObservableCollection<MainDict>(queryres);

                        if (result.Count != 0)
                        {
                            return result;
                        }
                        else
                        {
                            var err = new ObservableCollection<MainDict>();
                            err.Add(new MainDict() { JpChar = key, Explanation = "没有本地释义" });
                            return err;
                        }
                    }
                    else
                    {
                        return new ObservableCollection<MainDict>();
                    }
                });

            }
            /// <summary>
            /// Query MainDict database using given index and return the result in ObservableCollection type
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public static async Task<ObservableCollection<MainDict>> QueryForUIAsync(int id)
            {
                return await Task.Run(() =>
                {

                    var result = new ObservableCollection<MainDict>(_conn.Query<MainDict>("SELECT * FROM MainDict WHERE ID = ?", id));
                    if (result.Count != 0)
                    {
                        return result;
                    }
                    else
                    {
                        var err = new ObservableCollection<MainDict>();
                        err.Add(new MainDict() { JpChar = "没有本地释义", Explanation = "请查看网络释义" });
                        return err;
                    }

                });

            }
            public static async Task<ObservableCollection<MainDict>> FuzzyQueryForUIAsync(string key)
            {
                return await Task.Run(() =>
                {
                    if (!(string.IsNullOrEmpty(key)))
                    {
                        var result = new ObservableCollection<MainDict>(_conn.Query<MainDict>("SELECT * FROM MainDict WHERE JpChar LIKE ?", key.Replace(" ", "").Replace("　", "") + "%").Take(5));
                        if (result.Count != 0)
                        {
                            return result;
                        }
                        else
                        {
                            var err = new ObservableCollection<MainDict>();
                            err.Add(new MainDict() { JpChar = key, Explanation = "没有本地释义" });
                            return err;
                        }
                    }
                    else
                    {
                        return new ObservableCollection<MainDict>();
                    }
                });
            }
            public static ObservableCollection<MainDict> GetTable()
            {
                return new ObservableCollection<MainDict>(_conn.Table<MainDict>());
            }
            private static void CloseConnection()
            {
                _conn.Close();
            }
            private static void Reconnect()
            {
                _conn = new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), Path.Combine(ApplicationData.Current.LocalFolder.Path, "note.db"));
            }
            /// <summary>
            /// Insert an item into Maindict db
            /// </summary>
            /// <param name="jpchar"></param>
            /// <param name="explanation"></param>
            /// <param name="Kana"></param>
            public static void Add(string jpchar, string explanation, string Kana, int id)
            {
                _conn.CreateTable<MainDict>();
                var maxid = _conn.ExecuteScalar<int>("SELECT MAX( ID ) FROM MainDict ;");
                _conn.Execute($"UPDATE SQLITE_SEQUENCE SET seq = {maxid}  WHERE name = 'MainDict'");
                var entries = _conn.Query<MainDict>("SELECT * FROM MainDict WHERE ID = ?", id);
                if (entries.Count == 0)
                {
                    _conn.Insert(new MainDict() { JpChar = jpchar, Explanation = explanation, Kana = Kana, ID = id });
                    _conn.Commit();
                }

            }
        }
        public static class KanjiDictQueryEngine
        {
            private static SQLiteConnection _kanjiconn = new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), Path.Combine(ApplicationData.Current.LocalFolder.Path, "kanji.db"));
            /// <summary>
            /// Query the database with the specifed kanji
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public static IEnumerable<MainDict> Query(string key)
            {
                return _kanjiconn.Query<MainDict>("SELECT * FROM Kanjidict WHERE Kanji = ?", key);
            }
            /// <summary>
            /// Query kanji database using the given kanji and return the result in ObservableCollection type
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public static async Task<ObservableCollection<Kanjidict>> QueryForUIAsync(string key)
            {
                return await Task.Run(() =>
                {
                    if (!(string.IsNullOrEmpty(key)))
                    {
                        var result = new ObservableCollection<Kanjidict>(_kanjiconn.Query<Kanjidict>("SELECT * FROM Kanjidict WHERE Kanji = ?", key.Replace(" ", "").Replace("　", "")));
                        if (result.Count != 0)
                        {
                            return result;
                        }
                        else
                        {
                            var err = new ObservableCollection<Kanjidict>();
                            return err;
                        }
                    }
                    else
                    {
                        return new ObservableCollection<Kanjidict>();
                    }
                });

            }
            /// <summary>
            /// Query kanji database using the given jlpt level and return the result in ObservableCollection type
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public static async Task<ObservableCollection<Kanjidict>> QueryForUIAsync(int jlpt)
            {
                return await Task.Run(() =>
                {
                    if (jlpt < 6 & jlpt > 0)
                    {
                        var result = new ObservableCollection<Kanjidict>(_kanjiconn.Query<Kanjidict>("SELECT * FROM Kanjidict WHERE Jlpt = ?", jlpt));
                        if (result.Count != 0)
                        {
                            return result;
                        }
                        else
                        {
                            var err = new ObservableCollection<Kanjidict>();
                            return err;
                        }
                    }
                    else
                    {
                        return new ObservableCollection<Kanjidict>();
                    }
                });

            }
            /// <summary>
            /// Query kanji database using the given jlpt level and return the result in ObservableCollection type
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public static async Task<List<Kanjidict>> QueryAsync(int jlpt)
            {
                return await Task.Run(() =>
                {
                    if (jlpt < 6 & jlpt > 0)
                    {
                        var result = _kanjiconn.Query<Kanjidict>("SELECT * FROM Kanjidict WHERE Jlpt = ?", jlpt);
                        if (result.Count != 0)
                        {
                            return result;
                        }
                        else
                        {
                            var err = new List<Kanjidict>();
                            return err;
                        }
                    }
                    else
                    {
                        return new List<Kanjidict>();
                    }
                });

            }
            /// <summary>
            /// Query kanjis database using the given kanji and return the result in ObservableCollection type
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public static async Task<ObservableCollection<Kanjidict>> QueryForUIAsync(MatchCollection keywords)
            {
                return await Task.Run(() =>
                {
                    if (keywords.Count != 0)
                    {
                        ObservableCollection<Kanjidict> result = new ObservableCollection<Kanjidict>();
                        foreach (var ks in keywords)
                        {
                            var distincted = ks.ToString().Distinct();
                            foreach (var k in distincted)
                            {
                                foreach (var r in _kanjiconn.Query<Kanjidict>("SELECT * FROM Kanjidict WHERE Kanji = ?", k.ToString()))
                                {
                                    result.Add(r);
                                }
                            }
                        }
                        if (result != null && result.Count != 0)
                        {
                            return result;
                        }
                        else
                        {
                            var err = new ObservableCollection<Kanjidict>();
                            return err;
                        }
                    }
                    else
                    {
                        return new ObservableCollection<Kanjidict>();
                    }
                });

            }
        }
        


    }
}
