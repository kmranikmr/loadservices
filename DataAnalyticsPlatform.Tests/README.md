# DataAnalyticsPlatform Tests

This project contains NUnit tests for the DataAnalyticsPlatform Readers and Writers components.

## Project Structure

- `Readers.Tests`: Tests for the DataAnalyticsPlatform.Readers project
  - `CsvReaderTests.cs`: Tests for the CsvReader class
  - `BaseReaderTests.cs`: Tests for the BaseReader abstract class
  - `TestData/`: Directory containing test data files

- `Writers.Tests`: Tests for the DataAnalyticsPlatform.Writers project
  - `CsvWriterTests.cs`: Tests for the Csvwriter class
  - `BaseWriterTests.cs`: Tests for the BaseWriter abstract class
  - `TestData/`: Directory containing test output files

## Target Framework

These tests target .NET Core 2.1, which is compatible with the .NET Standard 2.0 libraries being tested.

## Running the Tests

### Using Visual Studio

1. Open the `DataAnalyticsPlatform.Tests.sln` solution in Visual Studio
2. Build the solution
3. Open the Test Explorer (Test > Test Explorer)
4. Click "Run All" to run all tests

### Using the Command Line

1. Navigate to the test project directory:
   ```
   cd DataAnalyticsPlatform.Tests
   ```

2. Run the tests:
   ```
   dotnet test
   ```

## Test Coverage

### Readers Tests

- Constructor initialization
- Data retrieval
- Handling special data formats (empty values, hyphens, non-numeric values)
- Preview functionality
- Record counting

### Writers Tests

- Constructor initialization
- Single record writing
- Batch processing
- Complete dataset writing
- File creation

## Notes

- Tests create and manage their own test data files
- Cleanup is performed after tests to remove output files
- Mock objects are used to test abstract base classes
