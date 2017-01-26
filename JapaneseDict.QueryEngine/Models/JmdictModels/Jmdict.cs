using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using static JapaneseDict.QueryEngine.Models.JmdictModels.JmDict;

namespace JapaneseDict.QueryEngine.Models.JmdictModels
{
    public class JmDict
    {
        public class Sense
        {
            public int EntryId { get; set; }
            public string Pos { get; set; }
            public string Ant { get; set; }
            public string AntPreview
            {
                get
                {
                    if(!string.IsNullOrWhiteSpace(Ant))
                    {
                        return "Antonyms: " + Ant;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            public string Dialect { get; set; }
            public string DialPreview
            {
                get
                {
                    if(!string.IsNullOrWhiteSpace(Dialect))
                    {
                        return "Dialect: " + Dialect;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            public string Misc { get; set; }
            public string Stagk { get; set; }
            public string Stagr { get; set; }
            public string Xref { get; set; }
            public string XrefPreview
            {
                get
                {
                    if(!string.IsNullOrWhiteSpace(Xref))
                    {
                        return "See also: " + Xref;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            public string Field { get; set; }
            public string Gloss { get; set; }
            [Ignore]
            public Visibility PosVis
            {
                get
                {
                    if(!string.IsNullOrWhiteSpace(Pos))
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            [Ignore]
            public Visibility AntVis
            {
                get
                {
                    if (!string.IsNullOrWhiteSpace(Ant))
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            [Ignore]
            public Visibility DialVis
            {
                get
                {
                    if (!string.IsNullOrWhiteSpace(Dialect))
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            [Ignore]
            public Visibility MiscVis
            {
                get
                {
                    if (!string.IsNullOrWhiteSpace(Misc))
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            [Ignore]
            public Visibility StagkVis
            {
                get
                {
                    if (!string.IsNullOrWhiteSpace(Stagk))
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            [Ignore]
            public Visibility StagrVis
            {
                get
                {
                    if (!string.IsNullOrWhiteSpace(Stagr))
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
            [Ignore]
            public Visibility XrefVis
            {
                get
                {
                    if (!string.IsNullOrWhiteSpace(Xref))
                    {
                        return Visibility.Visible;
                    }
                    else
                    {
                        return Visibility.Collapsed;
                    }
                }
            }
        }
        public class REle
        {
            public int EntryId { get; set; }
            public bool NoKanji { get; set; }
            public string Reb { get; set; }
            public string ReInf { get; set; }
            public string ReStr { get; set; }
            public IEnumerable<KEle> Kebs { get; set; }
            public bool ContainsReStr { get { return !string.IsNullOrWhiteSpace(ReStr); } }
            public Visibility KebsVis
            {
                get
                {
                    if(ContainsReStr)
                    {
                        return Visibility.Collapsed;
                    }
                    else
                    {
                        return Visibility.Visible;
                    }
                }
            }
            public Visibility RestrVis
            {
                get
                {
                    if (!ContainsReStr)
                    {
                        return Visibility.Collapsed;
                    }
                    else
                    {
                        return Visibility.Visible;
                    }
                }
            }
            public ObservableCollection<string> Restrs
            {
                get
                {
                    return new ObservableCollection<string>(ReStr.Replace(" ","").Split(','));
                }
            }
        }
        public class KEle
        {
            public int EntryId { get; set; }
            public string Keb { get; set; }
            public string KeInf { get; set; }
        }
    }
    public class PreviewResult
    {
        public int EntryId { get; set; }
        public string JpChar { get; set; }
        public string Result { get; set; }
    }
    public class SearchTerm
    {
        public int EntryId { get; set; }
        public string Keyword { get; set; }
        public bool IsFromSuggestion { get; set; } = false;
    }
    public class SearchResult
    {
        public ObservableCollection<KEle> Kele { get; set; }
        public ObservableCollection<REle> Rele { get; set; }
        public ObservableCollection<Sense> Sense { get; set; }
    }
}
