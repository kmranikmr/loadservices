using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataAnalyticsPlatform.Readers;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Shared.Models;
using NUnit.Framework;

namespace Readers.Tests
{
    [TestFixture]
    public class CsvReaderTests
    {
        private string _testDataPath;
        private ReaderConfiguration _config;

        [SetUp]
        public void Setup()
        {
            // Setup test data path
            _testDataPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
            
            // Create test data directory if it doesn't exist
            if (!Directory.Exists(_testDataPath))
            {
                Directory.CreateDirectory(_testDataPath);
            }
            
            // Create a sample CSV file for testing
            string sampleCsvPath = Path.Combine(_testDataPath, "sample.csv");
            if (!File.Exists(sampleCsvPath))
            {
                File.WriteAllText(sampleCsvPath, 
                    "Id,Name,Age,DateJoined\n" +
                    "1,John Doe,30,2020-01-15\n" +
                    "2,Jane Smith,25,2020-02-20\n" +
                    "3,Bob Johnson,-,2020-03-10\n" + // Testing hyphen for age
                    "4,Alice Brown,,2020-04-05\n" +  // Testing empty value for age
                    "5,Charlie Davis,abc,2020-05-01\n"); // Testing invalid numeric format
            }
            
            // Setup configuration
            _config = new ReaderConfiguration
            {
                ConnectionString = sampleCsvPath,
                TypeName = "TestType",
                HasHeaders = true
            };
        }

        [Test]
        public void CsvReader_Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var reader = new CsvReader(_config);
            
            // Assert
            Assert.IsNotNull(reader);
            Assert.AreEqual(_config.ConnectionString, reader.ConnectionString);
            Assert.AreEqual(_config.TypeName, reader.TypeName);
        }
        
        [Test]
        public void CsvReader_GetData_ReturnsAllRecords()
        {
            // Arrange
            var reader = new CsvReader(_config);
            
            // Act
            var records = reader.GetData();
            var recordsList = records.ToList();
            
            // Assert
            Assert.IsNotNull(records);
            Assert.AreEqual(5, recordsList.Count);
        }
        
        [Test]
        public void CsvReader_HyphenToZeroConverter_ConvertsNonNumericValuesToZero()
        {
            // Arrange
            var reader = new CsvReader(_config);
            
            // Act
            var records = reader.GetData();
            var recordsList = records.ToList();
            
            // Assert
            // Check record with hyphen age value
            var record3 = recordsList[2];
            var age3 = Convert.ToInt32(((IDictionary<string, object>)record3)["Age"]);
            Assert.AreEqual(0, age3);
            
            // Check record with empty age value
            var record4 = recordsList[3];
            var age4 = Convert.ToInt32(((IDictionary<string, object>)record4)["Age"]);
            Assert.AreEqual(0, age4);
            
            // Check record with invalid numeric format
            var record5 = recordsList[4];
            var age5 = Convert.ToInt32(((IDictionary<string, object>)record5)["Age"]);
            Assert.AreEqual(0, age5);
        }
        
        [Test]
        public void CsvReader_GetPreview_ReturnsCorrectNumberOfRecords()
        {
            // Arrange
            var reader = new CsvReader(_config);
            
            // Act
            var records = reader.GetPreview(2);
            var recordsList = records.ToList();
            
            // Assert
            Assert.IsNotNull(records);
            Assert.AreEqual(2, recordsList.Count);
        }
    }
}
