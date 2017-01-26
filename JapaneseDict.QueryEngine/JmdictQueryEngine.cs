using JapaneseDict.QueryEngine.Models.JmdictModels;
using SQLite.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using static JapaneseDict.QueryEngine.Models.JmdictModels.JmDict;

namespace JapaneseDict.QueryEngine
{
    public static class JmdictQueryEngine
    {
        private static SQLiteConnection _conn = new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), Path.Combine(ApplicationData.Current.LocalFolder.Path, "maindb.db"));
        public static IEnumerable<Sense> QuerySenses(int id)
        {
            var senses = _conn.Query<Sense>("SELECT * FROM Sense WHERE EntryId = ?", id);
            ObservableCollection<Sense> result = new ObservableCollection<Sense>(senses);
            return result;
        }
        public static async Task<ObservableCollection<Sense>> QuerySensesAsync(int id)
        {
            return await Task.Run(() =>
            {
                var senses = _conn.Query<Sense>("SELECT * FROM Sense WHERE EntryId = ?", id);
                ObservableCollection<Sense> result = new ObservableCollection<Sense>(senses);
                return result;
            });
        }
        public static IEnumerable<KEle> QueryKEle(int id)
        {
            var kele = _conn.Query<KEle>("SELECT * FROM KEle WHERE EntryId = ?", id);
            ObservableCollection<KEle> result = new ObservableCollection<KEle>(kele);
            return result;
        }
        public static async Task<ObservableCollection<KEle>> QueryKEleAsync(int id)
        {
            return await Task.Run(() =>
            {
                var kele = _conn.Query<KEle>("SELECT * FROM KEle WHERE EntryId = ?", id);
                ObservableCollection<KEle> result = new ObservableCollection<KEle>(kele);
                return result;
            });
        }
        public static IEnumerable<REle> QueryREle(int id)
        {
            var rele = _conn.Query<REle>("SELECT * FROM REle WHERE EntryId = ?", id);
            ObservableCollection<REle> result = new ObservableCollection<REle>(rele);
            return result;
        }
        public static async Task<ObservableCollection<REle>> QueryREleAsync(int id)
        {
            return await Task.Run(() =>
            {
                var rele = _conn.Query<REle>("SELECT * FROM REle WHERE EntryId = ?", id);
                ObservableCollection<REle> result = new ObservableCollection<REle>(rele);
                return result;
            });
        }
        public static IEnumerable<KEle> FuzzyQueryKEle(string keyword)
        {
            var kele = _conn.Query<KEle>("SELECT * FROM KEle WHERE Keb LIKE ?", keyword.Replace(" ", "").Replace("　", "") + "%").Take(6);
            return new ObservableCollection<KEle>(kele);
        }
        public static IEnumerable<REle> FuzzyQueryREle(string keyword)
        {
            var rele = _conn.Query<REle>("SELECT * FROM REle WHERE Reb LIKE ?", keyword.Replace(" ", "").Replace("　", "") + "%").Take(6);
            return new ObservableCollection<REle>(rele);
        }
        public static IEnumerable<KEle> QueryKEle(string keyword)
        {
            var kele = _conn.Query<KEle>("SELECT * FROM KEle WHERE Keb = ?", keyword.Replace(" ", "").Replace("　", ""));
            return new ObservableCollection<KEle>(kele);
        }
        public static IEnumerable<REle> QueryREle(string keyword)
        {
            var rele = _conn.Query<REle>("SELECT * FROM REle WHERE Reb = ?", keyword.Replace(" ", "").Replace("　", ""));
            return new ObservableCollection<REle>(rele);
        }
        public static async Task<IEnumerable<SearchResult>> QueryFromKeywordAsync(string keyword)
        {
            var kres = QueryKEle(keyword);
            var rres = QueryREle(keyword);
            var entries = new List<int>();
            foreach(var k in kres)
            {
                entries.Add(k.EntryId);
            }
            foreach(var r in rres)
            {
                entries.Add(r.EntryId);
            }
            entries = entries.Distinct().ToList();
            var res = new ObservableCollection<SearchResult>();
            foreach (var e in entries)
            {
                var kr = await QueryKEleAsync(e);
                var rr = await QueryREleAsync(e);
                var sr = await QuerySensesAsync(e);
                res.Add(new SearchResult() { Kele = kr, Rele = rr, Sense = sr });
            }
            return res;
        }
        public static IEnumerable<PreviewResult> QueryForPreview(string keyword)
        {
            var kres = FuzzyQueryKEle(keyword);
            var rres = FuzzyQueryREle(keyword);
            if((kres.Count()+rres.Count())>6)
            {
                kres = kres.Take(3);
                rres = rres.Take(3);
            }
            var res = new ObservableCollection<PreviewResult>();
            foreach(var k in kres)
            {
                var senseres = QuerySenses(k.EntryId);
                res.Add(new PreviewResult() { JpChar = k.Keb, EntryId = k.EntryId,Result=(senseres.Count()!=0?senseres.First().Gloss+" ...":"") });
            }
            foreach(var r in rres)
            {
                var senseres = QuerySenses(r.EntryId);
                res.Add(new PreviewResult() { JpChar = r.Reb, EntryId = r.EntryId, Result = (senseres.Count() != 0 ? senseres.First().Gloss+" ..." : "") });
            }
            return res;
        }
        public static async Task<ObservableCollection<PreviewResult>> QueryForPreviewAsync(string keyword)
        {
            return await Task.Run(() =>
            {
                if (!(string.IsNullOrEmpty(keyword)))
                {
                    var kres = FuzzyQueryKEle(keyword);
                    var rres = FuzzyQueryREle(keyword);
                    if ((kres.Count() + rres.Count()) > 6)
                    {
                        kres = kres.Take(3);
                        rres = rres.Take(3);
                    }
                    var res = new ObservableCollection<PreviewResult>();
                    if (rres.Count() == 0&&kres.Count()==0)
                    {
                        res.Add(new PreviewResult { JpChar = keyword, Result = "No local definitions found." });
                        return res;
                    }
                    
                    foreach (var k in kres)
                    {
                        var senseres = QuerySenses(k.EntryId);
                        res.Add(new PreviewResult() { JpChar = k.Keb, EntryId = k.EntryId, Result = (senseres.Count() != 0 ? senseres.First().Gloss + " ..." : "") });
                    }
                    foreach (var r in rres)
                    {
                        var senseres = QuerySenses(r.EntryId);
                        res.Add(new PreviewResult() { JpChar = r.Reb, EntryId = r.EntryId, Result = (senseres.Count() != 0 ? senseres.First().Gloss + " ..." : "") });
                    }
                    return res;
                }
                else
                {
                    return new ObservableCollection<PreviewResult>();
                }
            });

        }
    }
}
