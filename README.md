# Data Analytics Platform Load Services

## Overview

The Data Analytics Platform Load Services is a robust and scalable framework designed to process, transform, and manage large datasets. It leverages actor-based concurrency to handle various data processing tasks efficiently.

## System Architecture

![System Architecture Diagram](https://github.com/kmranikmr/loadservices/raw/master/Presentation1.png)

### Data Ingestion Pipeline

This platform handles data ingestion from various sources:
- CSV files
- JSON data 
- Twitter API data (also JSON format)
- Custom sources

The pipeline process:
1. Detects incoming data through file watchers, database watchers, or REST API endpoints
2. Extracts and previews data schema (showing sample data for user inspection)
3. Provides UI-driven entity mapping interface (external component) where users can:
   - Map source fields to destination fields
   - Set up transformations
   - Configure data type conversions
4. Transforms data using dynamic C# code generation via the CodeGenerator
5. Maps data to appropriate tables/collections
6. Outputs processed data to configurable destinations:
   - Elasticsearch
   - SQL databases
   - MongoDB

Preview endpoints allow users to view sample data and schema information before finalizing the mapping and processing.

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

### Windows Development Environment

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

### Docker Deployment (Linux)

The platform can be easily deployed on Linux using Docker and docker-compose:

1. **Prerequisites**
   - Docker installed on your Linux system
   - docker-compose installed

2. **Run the Application**
   ```bash
   # Start all services
   docker-compose up -d
   
   # Start specific service
   docker-compose up -d masterapi
   ```

3. **Configuration**
   - Modify `docker-compose.yml` to adjust service configurations
   - Environment variables can be set in the docker-compose file

4. **Accessing Services**
   - Master API: http://localhost:5000
   - Preview Service: http://localhost:5001

5. **Monitoring**
   ```bash
   # View logs
   docker-compose logs -f
   
   # Check service status
   docker-compose ps
   ```

6. **Scaling Workers**
   ```bash
   # Scale up worker nodes
   docker-compose up -d --scale worker=3
   ```

## To Be Done

The following enhancements are planned for future releases:

1. **Docker Compose Improvements**
   - Add dependencies and databases as part of the Docker Compose setup
   - Create a standalone system that includes all required components
   - Implement proper service discovery and networking

2. **Example API Documentation**
   - Add example API calls for common operations:
     - Uploading data files
     - Creating and modifying data mappings
     - Triggering data processing jobs
     - Monitoring job status
     - Retrieving processed data

3. **Developer Documentation**
   - Detailed setup instructions for development environments
   - API reference documentation
   - Component diagrams and architecture details

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

## Usage
The application provides forms and utilities to interact with the data processing actors:
- Configure data sources
- Map and transform data fields
- Monitor processing progress
- View results in target systems

## Design Outline

### Actors
Actors are the core components responsible for executing tasks in the system:

- **Master/Leader Actors**: Coordinate processing
- **Worker Actors**: Execute data processing tasks
- **Reader Actors**: Handle data ingestion
- **Writer Actors**: Output to destinations

### Core Components

- **Processors**: Transform and process data
- **Registry**: Manage schema registry and type configurations
- **System**: Handle notifications and coordination
- **Common**: Shared utilities and code generators

