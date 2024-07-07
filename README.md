# Data Analytics Platform Load Services

## Overview

The Data Analytics Platform Load Services is a robust and scalable framework designed to process, transform, and manage large datasets. It leverages actor-based concurrency to handle various data processing tasks efficiently.

## Directory Structure

- **Actors**: Contains actors responsible for different processing tasks.
- **Processors**: Handles reading, transforming, and writing data.
- **Registry**: Manages registry-related operations.
- **System**: System-level functionalities such as notifications.
- **Utils**: Utility functions.
- **Worker**: Manages worker nodes and actors.
- **Application.Net**: .NET application files, including forms and configurations.
- **Common**: Common utilities and constants.
- **Data**: Data provider actors and related functionalities.
- **Readers**: File reading functionalities.
- **Service.MasterAPI**: Service implementation for the Master API.

## Prerequisites

- .NET Core 3.1 or higher
- Visual Studio 2019 or higher
- Any database supported by the DAP (e.g., SQL Server, PostgreSQL)

## Getting Started

### Clone the Repository

```bash
git clone <repository-url>
cd DAPLoadServices


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

DataProviderActor: Provides data access functionalities.
Readers
File readers are responsible for reading data from various file formats.

FileReaderActor: Reads data from files and sends it to the processor actors.
Service.MasterAPI
Implements the Master API service, providing endpoints for managing the data processing tasks.

Program: Entry point for the Master API service.
Startup: Configures the services and middleware for the Master API.

![alt text]https://github.com/kmranikmr/loadservices/blob/master/Presentation1.png

System Architecture

![Presentation1](https://github.com/kmranikmr/loadservices/assets/173465556/25d1e7bc-f747-4d23-934c-a53237e46b06)





