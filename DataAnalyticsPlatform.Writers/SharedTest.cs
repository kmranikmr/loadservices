using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using DataAnalyticsPlatform.Shared;

namespace DataAnalyticsPlatform.Shared.Test
{
    
    public class SharedTest
    {
        public static void Main(string[] args)
        {
            ShouldGenerateClass();
        }
        [Fact]
        public static void ShouldGenerateClass()
        {
            CsvModelGenerator csv = new CsvModelGenerator();
            string classdata = csv.ClassGenerator(@"e:\nifi\mydata.csv");
            int g = 0;
        }
    }
}
