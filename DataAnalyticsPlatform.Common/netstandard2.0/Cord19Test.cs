using Newtonsoft.Json;
using System.Collections.Generic;

namespace DataAnalyticsPlatform.Shared.Cord19
{
    public partial class OriginalRecord
    {
        [JsonProperty("paper_id")]
        public string paper_id { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("abstract")]
        public Abstract[] Abstract { get; set; }

        [JsonProperty("body_text")]
        public Abstract[] body_text { get; set; }

        [JsonProperty("bib_entries")]
        public Dictionary<string, BibEntry> bib_entries { get; set; }

        [JsonProperty("ref_entries")]
        public Dictionary<string, RefEntry> ref_entries { get; set; }

        [JsonProperty("back_matter")]
        public BackAbstract[] back_matter { get; set; }
    }

    public partial class Abstract
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("cite_spans")]
        public Span[] CiteSpans { get; set; }

        [JsonProperty("ref_spans")]
        public Span[] RefSpans { get; set; }

        [JsonProperty("section")]
        public string Section { get; set; }
    }

    public partial class BackAbstract
    {
        [JsonProperty("backtext")]
        public string BackText { get; set; }

        [JsonProperty("cite_spans")]
        public Span[] CiteSpans { get; set; }

        [JsonProperty("ref_spans")]
        public Span[] RefSpans { get; set; }

        [JsonProperty("section")]
        public string Section { get; set; }
    }

    public partial class Span
    {
        [JsonProperty("start")]
        public long Start { get; set; }

        [JsonProperty("end")]
        public long End { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("ref_id")]
        public string ref_id { get; set; }
    }

    public partial class BibEntry
    {
        [JsonProperty("ref_id")]
        public string ref_id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("authors")]
        public MetadataAuthor[] Authors { get; set; }

        [JsonProperty("year")]
        public long? Year { get; set; }

        [JsonProperty("venue")]
        public string Venue { get; set; }

        [JsonProperty("volume")]
        public string Volume { get; set; }

        [JsonProperty("issn")]
        public string Issn { get; set; }

        //   [JsonProperty("pages")]
        //   public Pages Pages { get; set; }

        [JsonProperty("other_ids")]
        public OtherIds OtherIds { get; set; }
    }

    public partial class BibEntryAuthor
    {
        [JsonProperty("first")]
        public string First { get; set; }

        [JsonProperty("middle")]
        public string[] Middle { get; set; }

        [JsonProperty("last")]
        public string Last { get; set; }

        [JsonProperty("suffix")]
        public string Suffix { get; set; }
    }

    public partial class OtherIds
    {
    }

    public partial class Metadata
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("authors")]
        public MetadataAuthor[] Authors { get; set; }
    }

    public partial class MetadataAuthor
    {
        [JsonProperty("first")]
        public string First { get; set; }

        [JsonProperty("middle")]
        public string[] Middle { get; set; }

        [JsonProperty("last")]
        public string Last { get; set; }

        [JsonProperty("suffix")]
        public string Suffix { get; set; }

        [JsonProperty("affiliation")]
        public Affiliation Affiliation { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public partial class Affiliation
    {
        [JsonProperty("laboratory", NullValueHandling = NullValueHandling.Ignore)]
        public string Laboratory { get; set; }

        [JsonProperty("institution", NullValueHandling = NullValueHandling.Ignore)]
        public string Institution { get; set; }

        [JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
        public Location Location { get; set; }
    }

    public partial class Location
    {
        [JsonProperty("addrLine", NullValueHandling = NullValueHandling.Ignore)]
        public string AddrLine { get; set; }

        [JsonProperty("settlement")]
        public string Settlement { get; set; }

        [JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
        public string Country { get; set; }
    }

    public partial class RefEntry
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("latex")]
        public object Latex { get; set; }

        //  [JsonProperty("type")]
        //  public TypeEnum Type { get; set; }
    }
}


//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DataAnalyticsPlatform.Shared.Cord19
//{
//    public partial class OriginalRecord
//    {
//        [JsonProperty("paper_id")]
//        public string PaperId { get; set; }

//        [JsonProperty("metadata")]
//        public Metadata Metadata { get; set; }

//        [JsonProperty("abstract")]
//        public Abstract[] Abstract { get; set; }

//        [JsonProperty("body_text")]
//        public Abstract[] BodyText { get; set; }

//        [JsonProperty("bib_entries")]
//        public Dictionary<string, BibEntry> BibEntries { get; set; }

//        [JsonProperty("ref_entries")]
//        public Dictionary<string, RefEntry> RefEntries { get; set; }

//        [JsonProperty("back_matter")]
//        public Abstract[] BackMatter { get; set; }
//    }

//    public partial class Abstract
//    {
//        [JsonProperty("text")]
//        public string Text { get; set; }

//        [JsonProperty("cite_spans")]
//        public Span[] CiteSpans { get; set; }

//        [JsonProperty("ref_spans")]
//        public Span[] RefSpans { get; set; }

//        [JsonProperty("section")]
//        public string Section { get; set; }
//    }

//    public partial class Span
//    {
//        [JsonProperty("start")]
//        public long Start { get; set; }

//        [JsonProperty("end")]
//        public long End { get; set; }

//        [JsonProperty("text")]
//        public string Text { get; set; }

//        [JsonProperty("ref_id")]
//        public string RefId { get; set; }
//    }

//    public partial class BibEntry
//    {
//        [JsonProperty("ref_id")]
//        public string RefId { get; set; }

//        [JsonProperty("title")]
//        public string Title { get; set; }

//        [JsonProperty("authors")]
//        public BibEntryAuthor[] Authors { get; set; }

//        [JsonProperty("year")]
//        public long? Year { get; set; }

//        [JsonProperty("venue")]
//        public string Venue { get; set; }

//        [JsonProperty("volume")]
//        public string Volume { get; set; }

//        [JsonProperty("issn")]
//        public string Issn { get; set; }

//        //   [JsonProperty("pages")]
//        //   public Pages Pages { get; set; }

//        [JsonProperty("other_ids")]
//        public OtherIds OtherIds { get; set; }
//    }

//    public partial class BibEntryAuthor
//    {
//        [JsonProperty("first")]
//        public string First { get; set; }

//        [JsonProperty("middle")]
//        public string[] Middle { get; set; }

//        [JsonProperty("last")]
//        public string Last { get; set; }

//        [JsonProperty("suffix")]
//        public string Suffix { get; set; }
//    }

//    public partial class OtherIds
//    {
//    }

//    public partial class Metadata
//    {
//        [JsonProperty("title")]
//        public string Title { get; set; }

//        [JsonProperty("authors")]
//        public MetadataAuthor[] Authors { get; set; }
//    }

//    public partial class MetadataAuthor
//    {
//        [JsonProperty("first")]
//        public string First { get; set; }

//        [JsonProperty("middle")]
//        public string[] Middle { get; set; }

//        [JsonProperty("last")]
//        public string Last { get; set; }

//        [JsonProperty("suffix")]
//        public string Suffix { get; set; }

//        [JsonProperty("affiliation")]
//        public Affiliation Affiliation { get; set; }

//        [JsonProperty("email")]
//        public string Email { get; set; }
//    }

//    public partial class Affiliation
//    {
//        [JsonProperty("laboratory", NullValueHandling = NullValueHandling.Ignore)]
//        public string Laboratory { get; set; }

//        [JsonProperty("institution", NullValueHandling = NullValueHandling.Ignore)]
//        public string Institution { get; set; }

//        [JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
//        public Location Location { get; set; }
//    }

//    public partial class Location
//    {
//        [JsonProperty("addrLine", NullValueHandling = NullValueHandling.Ignore)]
//        public string AddrLine { get; set; }

//        [JsonProperty("settlement")]
//        public string Settlement { get; set; }

//        [JsonProperty("country", NullValueHandling = NullValueHandling.Ignore)]
//        public string Country { get; set; }
//    }

//    public partial class RefEntry
//    {
//        [JsonProperty("text")]
//        public string Text { get; set; }

//        [JsonProperty("latex")]
//        public object Latex { get; set; }

//        //  [JsonProperty("type")]
//        //  public TypeEnum Type { get; set; }
//    }
//}
