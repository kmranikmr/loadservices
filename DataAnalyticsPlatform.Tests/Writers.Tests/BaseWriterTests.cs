using System;
using System.Collections.Generic;
using System.Linq;
using DataAnalyticsPlatform.Writers;
using DataAnalyticsPlatform.Shared.DataAccess;
using NUnit.Framework;
using Moq;

namespace Writers.Tests
{
    [TestFixture]
    public class BaseWriterTests
    {
        private Mock<BaseWriter> _mockWriter;
        private List<IDictionary<string, object>> _writtenData;

        [SetUp]
        public void Setup()
        {
            _writtenData = new List<IDictionary<string, object>>();
            
            // Create a mock of the abstract BaseWriter class
            _mockWriter = new Mock<BaseWriter>("mock://connection", "MockType") { CallBase = true };
            
            // Setup mock to track written data
            _mockWriter.Setup(w => w.Write(It.IsAny<IDictionary<string, object>>()))
                .Callback<IDictionary<string, object>>(data => _writtenData.Add(data));
        }

        [Test]
        public void BaseWriter_Constructor_InitializesProperties()
        {
            // Assert
            Assert.AreEqual("mock://connection", _mockWriter.Object.ConnectionString);
            Assert.AreEqual("MockType", _mockWriter.Object.DestinationType);
        }
        
        [Test]
        public void BaseWriter_WriteAll_CallsWriteForEachRecord()
        {
            // Arrange
            var testData = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object> { { "Id", 1 }, { "Name", "Test 1" } },
                new Dictionary<string, object> { { "Id", 2 }, { "Name", "Test 2" } },
                new Dictionary<string, object> { { "Id", 3 }, { "Name", "Test 3" } }
            };
            
            // Act
            _mockWriter.Object.WriteAll(testData);
            
            // Assert
            _mockWriter.Verify(w => w.Write(It.IsAny<IDictionary<string, object>>()), Times.Exactly(3));
            Assert.AreEqual(3, _writtenData.Count);
            Assert.AreEqual(1, _writtenData[0]["Id"]);
            Assert.AreEqual("Test 3", _writtenData[2]["Name"]);
        }
        
        [Test]
        public void BaseWriter_Dispose_DoesNotThrow()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => _mockWriter.Object.Dispose());
        }
        
        [Test]
        public void BaseWriter_LogInfo_DoesNotThrow()
        {
            // Accessing protected method through reflection
            var method = typeof(BaseWriter).GetMethod("LogInfo", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act & Assert
            Assert.DoesNotThrow(() => method.Invoke(_mockWriter.Object, new object[] { "Test log message" }));
        }
        
        [Test]
        public void BaseWriter_LogError_DoesNotThrow()
        {
            // Accessing protected method through reflection
            var method = typeof(BaseWriter).GetMethod("LogError", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act & Assert
            Assert.DoesNotThrow(() => method.Invoke(_mockWriter.Object, 
                new object[] { "Test error message", new Exception("Test exception") }));
        }
    }
}
