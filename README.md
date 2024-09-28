# Customer and Order Management API

## Overview

This API provides functionality to manage **Customers** and **Orders** with support for creating, updating, and deleting data. Additionally, it allows querying and iterating over customer orders by order date. The API is built using **C#** and follows **Domain-Driven Design (DDD)** principles with the **CQRS** pattern to separate commands from queries. The application uses **Entity Framework** for data persistence and implements the **repository** and **unit of work** patterns.

## Features

- **Customer Management**: Create, update, and delete customers.
- **Order Management**: Create and manage customer orders, including multiple items per order.
- **Product and Item Management**: Each order consists of items, where each item corresponds to a product with its price.
- **Query Orders by Date**: Retrieve customer orders based on the order date.
- **CQRS Pattern**: Segregation of commands and queries for optimal separation of concerns.
- **Domain-Driven Design**: Emphasis on the domain model and core business logic.
- **Repository and Unit of Work Patterns**: For effective data access layer management.
- **Entity Framework**: Used for persistence.
- **Unit Testing with NUnit**: Unit tests included to ensure code reliability.
- **XML Documentation**: API is fully documented using XML comments for better clarity.

## Technologies and Patterns Used

- **C# .NET**
- **Entity Framework Core**
- **Repository Pattern**
- **Unit of Work Pattern**
- **CQRS (Command Query Responsibility Segregation)**
- **Domain-Driven Design (DDD)**
- **NUnit** for Unit Testing
- **XML Documentation**

## Project Structure

- **Domain**: Contains core domain logic and business rules (Customers, Orders, Products).
- **Infrastructure**: Handles data access, including repositories and database context.
- **Application**: Manages commands and queries, containing application services.
- **API**: Exposes endpoints for managing customers, orders, and products.
- **Tests**: Contains unit tests using NUnit to verify business logic and services.
