using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataAnalyticsPlatform.Shared.DataAccess;
using DataAnalyticsPlatform.Shared.Interfaces;
using DataAnalyticsPlatform.Writers;
using NUnit.Framework;

namespace Writers.Tests
{
    [TestFixture]
    public class CsvWriterTests
    {
        private string _testDataPath;
        private WriterConfiguration _config;
        private string _outputFilePath;

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
            
            // Setup output file path
            _outputFilePath = Path.Combine(_testDataPath, "output.csv");
            
            // Delete any existing output file
            if (File.Exists(_outputFilePath))
            {
                File.Delete(_outputFilePath);
            }
            
            // Setup configuration
            _config = new WriterConfiguration
            {
                ConnectionString = _outputFilePath,
                DestinationType = "Test",
                BatchSize = 2 // Small batch size for testing
            };
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test output files
            if (File.Exists(_outputFilePath))
            {
                try
                {
                    File.Delete(_outputFilePath);
                }
                catch (IOException)
                {
                    // File might be locked, ignore
                }
            }
        }

        [Test]
        public void CsvWriter_Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var writer = new Csvwriter(_config);
            
            // Assert
            Assert.IsNotNull(writer);
            Assert.AreEqual(_config.ConnectionString, writer.ConnectionString);
            Assert.AreEqual(_config.DestinationType, writer.DestinationType);
        }
        
        [Test]
        public void CsvWriter_Write_CreatesFileWithData()
        {
            // Arrange
            var writer = new Csvwriter(_config);
            var testData = GetTestData(5);
            
            // Act
            foreach (var item in testData)
            {
                writer.Write(item);
            }
            writer.Dispose(); // Ensure data is flushed and file is closed
            
            // Assert
            Assert.IsTrue(File.Exists(_outputFilePath));
            var lines = File.ReadAllLines(_outputFilePath);
            Assert.AreEqual(6, lines.Length); // Header + 5 data lines
        }
        
        [Test]
        public void CsvWriter_WriteWithBatches_HandlesCorrectly()
        {
            // Arrange
            _config.BatchSize = 3; // Set batch size to 3
            var writer = new Csvwriter(_config);
            var testData = GetTestData(8); // 8 records should create 2 full batches and 1 partial
            
            // Act
            foreach (var item in testData)
            {
                writer.Write(item);
            }
            writer.Dispose(); // Ensure all batches are written
            
            // Assert
            Assert.IsTrue(File.Exists(_outputFilePath));
            var lines = File.ReadAllLines(_outputFilePath);
            Assert.AreEqual(9, lines.Length); // Header + 8 data lines
        }
        
        [Test]
        public void CsvWriter_WriteAll_WritesAllRecords()
        {
            // Arrange
            var writer = new Csvwriter(_config);
            var testData = GetTestData(4);
            
            // Act
            writer.WriteAll(testData);
            writer.Dispose();
            
            // Assert
            Assert.IsTrue(File.Exists(_outputFilePath));
            var lines = File.ReadAllLines(_outputFilePath);
            Assert.AreEqual(5, lines.Length); // Header + 4 data lines
        }
        
        [Test]
        public void CsvWriter_NoData_CreatesFileWithHeaderOnly()
        {
            // Arrange
            var writer = new Csvwriter(_config);
            
            // Act - add no data, just dispose to create the file
            writer.Dispose();
            
            // Assert - should have a file with just the header
            Assert.IsTrue(File.Exists(_outputFilePath));
            var content = File.ReadAllText(_outputFilePath);
            Assert.IsTrue(content.Trim().Length > 0); // Should have at least a header
        }
        
        private List<IDictionary<string, object>> GetTestData(int count)
        {
            var result = new List<IDictionary<string, object>>();
            
            for (int i = 1; i <= count; i++)
            {
                var record = new Dictionary<string, object>
                {
                    { "Id", i },
                    { "Name", $"Person {i}" },
                    { "Age", 20 + i },
                    { "Email", $"person{i}@example.com" }
                };
                
                result.Add(record);
            }
            
            return result;
        }
    }
}
