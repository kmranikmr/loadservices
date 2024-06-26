using CsvHelper.Configuration;

namespace DataAnalyticsPlatform.Shared.Types
{
    public class Model
    {
        public int policyID { get; set; }
        public string statecode { get; set; }
        public string county { get; set; }
        public double eq_site_limit { get; set; }
        public double hu_site_limit { get; set; }
        public double fl_site_limit { get; set; }
        public double fr_site_limit { get; set; }
        public double tiv_2011 { get; set; }
        public double tiv_2012 { get; set; }
        public double eq_site_deductible { get; set; }
        public double hu_site_deductible { get; set; }
        public double fl_site_deductible { get; set; }
        public double fr_site_deductible { get; set; }
        public double point_latitude { get; set; }
        public double point_longitude { get; set; }
        public string line { get; set; }
        public string construction { get; set; }
        public double point_granularity { get; set; }
    }

    public class ModelClassMap : ClassMap<Model>
    {
        public ModelClassMap()
        {
            Map(m => m.policyID).Name("policyID");
            Map(m => m.statecode).Name("statecode");
            Map(m => m.county).Name("county");
            Map(m => m.eq_site_limit).Name("eq_site_limit");
            Map(m => m.hu_site_limit).Name("hu_site_limit");
            Map(m => m.fl_site_limit).Name("fl_site_limit");
            Map(m => m.fr_site_limit).Name("fr_site_limit");
            Map(m => m.tiv_2011).Name("tiv_2011");
            Map(m => m.tiv_2012).Name("tiv_2012");
            Map(m => m.eq_site_deductible).Name("eq_site_deductible");
            Map(m => m.hu_site_deductible).Name("hu_site_deductible");
            Map(m => m.fl_site_deductible).Name("fl_site_deductible");
            Map(m => m.fr_site_deductible).Name("fr_site_deductible");
            Map(m => m.point_latitude).Name("point_latitude");
            Map(m => m.point_longitude).Name("point_longitude");
            Map(m => m.line).Name("line");
            Map(m => m.construction).Name("construction");
            Map(m => m.point_granularity).Name("point_granularity");
        }
    }
}
