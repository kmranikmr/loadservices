using System;
using System.Collections.Generic;
using System.Linq;
using DataAnalyticsPlatform.Readers;
using DataAnalyticsPlatform.Shared.DataAccess;
using NUnit.Framework;
using Moq;
using System.Reflection;

namespace Readers.Tests
{
    [TestFixture]
    public class BaseReaderTests
    {
        private Mock<BaseReader> _mockReader;
        private ReaderConfiguration _config;

        [SetUp]
        public void Setup()
        {
            _config = new ReaderConfiguration
            {
                ConnectionString = "mock://connection",
                TypeName = "MockType",
                HasHeaders = true
            };
            
            // Create a mock of the abstract BaseReader class
            _mockReader = new Mock<BaseReader>(_config.ConnectionString, _config.TypeName) { CallBase = true };
            
            // Setup mocked data
            var testData = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object> { { "Id", 1 }, { "Name", "Test 1" } },
                new Dictionary<string, object> { { "Id", 2 }, { "Name", "Test 2" } },
                new Dictionary<string, object> { { "Id", 3 }, { "Name", "Test 3" } },
                new Dictionary<string, object> { { "Id", 4 }, { "Name", "Test 4" } },
                new Dictionary<string, object> { { "Id", 5 }, { "Name", "Test 5" } }
            };
            
            _mockReader.Setup(r => r.GetData()).Returns(testData);
        }

        [Test]
        public void BaseReader_Constructor_InitializesProperties()
        {
            // Assert
            Assert.AreEqual(_config.ConnectionString, _mockReader.Object.ConnectionString);
            Assert.AreEqual(_config.TypeName, _mockReader.Object.TypeName);
        }
        
        [Test]
        public void BaseReader_GetPreview_ReturnsCorrectNumberOfRecords()
        {
            // Arrange
            int previewCount = 3;
            
            // Act
            var result = _mockReader.Object.GetPreview(previewCount);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(previewCount, result.Count());
        }
        
        [Test]
        public void BaseReader_GetPreview_ReturnsAllWhenCountGreaterThanAvailable()
        {
            // Arrange
            int previewCount = 10; // More than we have test data
            
            // Act
            var result = _mockReader.Object.GetPreview(previewCount);
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Count()); // Should get all 5 test records
        }
        
        [Test]
        public void BaseReader_GetCount_ReturnsCorrectCount()
        {
            // Act
            var count = _mockReader.Object.GetCount();
            
            // Assert
            Assert.AreEqual(5, count);
        }
    }
}
