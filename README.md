# Library Management System API

A **RESTful Web API** built with **ASP.NET Core (.NET 8)** that allows users to manage books in a library system.
The API supports full **CRUD operations** and uses **Entity Framework Core with SQLite** for data persistence.

This project demonstrates backend development with modern .NET technologies and clean API design.

---

# Tech Stack

**Backend**

* C#
* ASP.NET Core Web API (.NET 8)
* Entity Framework Core
* SQLite

**Tools**

* Swagger / OpenAPI
* .NET CLI

---

# Features

* Create new books
* Retrieve all books
* Retrieve a single book by ID
* Update book information
* Delete books
* Input validation using Data Annotations
* Swagger API documentation

---

# Project Structure

```
LibraryManagement.Api
│
├── Controllers
│   └── BooksController.cs
│
├── Data
│   └── LibraryDbContext.cs
│
├── Models
│   └── Book.cs
│
├── Migrations
│
├── Program.cs
└── appsettings.json
```

---

# Book Model

Each book contains the following fields:

```
Id
Title
Author
ISBN
PublicationYear
Available
```

Example JSON:

```json
{
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "isbn": "9780132350884",
  "publicationYear": 2008,
  "available": true
}
```

---

# API Endpoints

| Method | Endpoint          | Description       |
| ------ | ----------------- | ----------------- |
| GET    | `/api/books`      | Get all books     |
| GET    | `/api/books/{id}` | Get book by ID    |
| POST   | `/api/books`      | Create a new book |
| PUT    | `/api/books/{id}` | Update a book     |
| DELETE | `/api/books/{id}` | Delete a book     |

---

# Running the Project

### 1. Clone the repository

```bash
git clone https://github.com/kayanr/library-management-system-dotnet-api.git
```

### 2. Navigate to the project

```bash
cd library-management-system-dotnet-api/LibraryManagement.Api
```

### 3. Restore dependencies

```bash
dotnet restore
```

### 4. Apply database migrations

```bash
dotnet ef database update
```

### 5. Run the API

```bash
dotnet run
```

---

# Swagger API Documentation

After running the project, open Swagger in your browser:

```
http://localhost:xxxx/swagger
```

Swagger allows you to interactively test all API endpoints.

---

# Future Improvements

* Authentication and authorization
* DTO layer for API responses
* Service layer for business logic
* Integration with React frontend
* PostgreSQL or SQL Server support

---
