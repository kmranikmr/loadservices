# Data Analytics Platform Load Services

## Overview

The Data Analytics Platform Load Services is a robust and scalable framework designed to process, transform, and manage large datasets. It leverages actor-based concurrency to handle various data processing tasks efficiently.

## System Architecture

![System Architecture Diagram](https://github.com/kmranikmr/loadservices/raw/master/system_diagram.png)

### Data Ingestion Pipeline

The Data Analytics Platform implements a comprehensive data ingestion pipeline that can process various data formats:

1. **Data Sources**:
   - CSV files
   - JSON data
   - Third-party Twitter data (JSON format)
   - Custom data sources

2. **Processing Flow**:
   - **FolderWatcher** monitors directories for new files to process
   - **Database Watcher** tracks changes in database sources
   - **Rest API Endpoint** accepts data through API requests
   - **Automation Coordinator** orchestrates the ingestion workflow
   - **Leader (Master)** delegates work to multiple Workers
   - **Worker** nodes perform the actual data processing

3. **Preview Services**:
   - Schema detection and preview functionality
   - CSV/JSON Schema Generator creates appropriate models
   - UI-driven entity mapping (external component)
   - Schema/Model storage in SQL Server database

4. **Processing Components**:
   - **Reader** components handle different data formats
   - **Writer** components output to various destinations:
     - Elasticsearch
     - SQL databases
     - MongoDB

5. **Transformation Engine**:
   - Dynamic code generation using C#
   - Schema mapping and transformation
   - Field-level data mapping and type conversion

All components are configurable and can be extended to support additional data sources or destinations.

## Key Components

### Data Processing Pipeline

- **Readers** - Import data from various sources (CSV, JSON, API)
- **Processors** - Transform and analyze data
- **Writers** - Export processed data to destinations (databases, files)

### Concurrency Model

The platform uses an actor-based concurrency model with Akka.NET, providing:
- Fault tolerance and supervision
- High throughput processing
- Scalable and distributed workloads

## Directory Structure

| Component | Description |
|-----------|-------------|
| **Actors** | Contains actors responsible for different processing tasks |
| **Processors** | Handles reading, transforming, and writing data |
| **Registry** | Manages schema registry and type configurations |
| **System** | System-level functionalities such as notifications |
| **Utils** | Utility functions and helpers |
| **Worker** | Manages worker nodes and distributes processing |
| **Application.Net** | .NET application files, including forms and configurations |
| **Common** | Common utilities and constants |
| **Readers** | File reading implementations for various formats |
| **Writers** | Data export implementations |
| **Service.MasterAPI** | Service implementation for the Master API |

## Prerequisites

- .NET Core 3.1 or higher
- Visual Studio 2019 or higher
- Any database supported by the platform (SQL Server, PostgreSQL, MongoDB)
- Docker (optional, for containerized deployment)

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/kmranikmr/loadservices.git
cd loadservices
```

### Setup Development Environment

1. **Open in Visual Studio**
   - Open the `DataAnalyticsPlatform.sln` solution file in Visual Studio

2. **Restore NuGet Packages**
   - Right-click on the solution in Solution Explorer
   - Select "Restore NuGet Packages"

3. **Build the Solution**
   - Build the solution to ensure all dependencies are correctly resolved
   - `Ctrl+Shift+B` or Build → Build Solution

4. **Configure Application Settings**
   - Update the connection strings in `appsettings.json` files
   - Configure any required API keys or service endpoints

5. **Run the Application**
   - Set the appropriate startup project
   - Press F5 to run in debug mode

## Features

- **Multi-format Data Import** - CSV, JSON, API, and custom formats
- **Schema Detection** - Automatic schema detection and mapping
- **Transformation** - Data cleaning, transformation, and enrichment
- **Batched Processing** - Efficient processing of large datasets
- **Multiple Export Options** - SQL, MongoDB, CSV, Elasticsearch
- **Real-time Progress Tracking** - SignalR-based progress notifications

## Testing

The solution includes NUnit tests for verifying reader and writer components:

- **Readers.Tests** - Tests for data import functionality
- **Writers.Tests** - Tests for data export functionality

Run tests through Visual Studio's Test Explorer or via the command line:

```bash
cd DataAnalyticsPlatform.Tests
dotnet test
```

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit your changes: `git commit -m 'Add amazing feature'`
4. Push to the branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

## License

This project is licensed under the MIT License.

---

## Architecture Details

### Actor System

The platform is built on an actor-based architecture for concurrent processing:

- **Master Actors** - Coordinate overall processing
- **Worker Actors** - Execute processing tasks
- **Reader/Writer Actors** - Handle data I/O operations

### Data Flow

```
Data Source → Reader → Processor → Transformer → Writer → Data Destination
     ↑                                               ↓
     └───────────── Progress Feedback ──────────────┘
```

### Extensibility

The platform can be extended with:

- Custom readers for new data sources
- Custom writers for new destinations
- Custom processors for specialized transformations
- Custom actors for domain-specific processing


### Open in Visual Studio
Open the DAPLoadServices.sln solution file in Visual Studio.

### Restore NuGet Packages
Restore the required NuGet packages for the solution.

### Build the Solution
Build the solution to ensure all dependencies are correctly resolved.

### Configure Application Settings
Update the App.config file in the Application.Net project with the appropriate settings, such as database connection strings and API keys.

### Run the Application
Set the DataAnalyticsPlatform.Application.Net project as the startup project and run it.

Usage
The application provides various forms and utilities to interact with the data processing actors. Use the provided forms to input data, configure processing tasks, and monitor the system.

Contributing
Contributions are welcome! Please submit pull requests or open issues to contribute to the project.

License
This project is licensed under the MIT License.

## Design Outline
### Actors
Actors are the core components responsible for executing various tasks in the system. The design follows the actor model, promoting high concurrency and fault tolerance.

### Processors
ReaderActor: Reads data from various sources.
TransformerActor: Transforms data into the required format.
WriterActor: Writes processed data to the destination.
WriterManager: Manages writer actors and ensures data consistency.
Registry
GetAllRegistry: Retrieves all registry entries.
RegistryActor: Manages individual registry entries.
RegistryActorProvider: Provides registry actors as needed.
System
DAP: Main system actor responsible for overall coordination.
Notifier: Handles system notifications.
Utils
HoconLoader: Utility for loading HOCON (Human-Optimized Config Object Notation) configurations.
Worker
WorkerActor: Executes tasks assigned to worker nodes.
WorkerNode: Represents individual worker nodes.

Common
Contains shared utilities and constants used across the system.

CommonActor: Base class for common actor functionalities.
Constants: Defines various constants used in the system.
Data
Data provider actors manage the data access layer.

## License

This project is licensed under the MIT License.

---

## Architecture Details

### Actor System

The platform is built on an actor-based architecture for concurrent processing:

- **Master Actors** - Coordinate overall processing
- **Worker Actors** - Execute processing tasks
- **Reader/Writer Actors** - Handle data I/O operations

### Data Flow

```
Data Source → Reader → Processor → Transformer → Writer → Data Destination
     ↑                                               ↓
     └───────────── Progress Feedback ──────────────┘
```

### Extensibility

The platform can be extended with:

- Custom readers for new data sources
- Custom writers for new destinations
- Custom processors for specialized transformations
- Custom actors for domain-specific processing