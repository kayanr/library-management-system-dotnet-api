# Library Management System API

A **RESTful Web API** built with **ASP.NET Core (.NET 8)** that allows users to manage books, members, and loans in a library system.
The API supports full **CRUD operations** and uses **Entity Framework Core with SQLite** for data persistence.

This project demonstrates backend development with modern .NET technologies, clean API design, and a layered service architecture.

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

**Books**
* Full CRUD — create, retrieve, update, and delete books
* Input validation using Data Annotations

**Members**
* Full CRUD — create, retrieve, update, and delete library members
* Email uniqueness validation
* Layered architecture — Controller → Service → Database
* Structured error handling via `ServiceResult<T>`

**Loans**
* Borrow a book — enforces availability and membership rules
* Return a book — updates availability automatically
* Maximum 3 active loans per member
* Full loan history per member

**General**
* Swagger API documentation

---

# Project Structure

```
LibraryManagement.Api
│
├── Controllers
│   ├── BooksController.cs
│   ├── MembersController.cs
│   └── LoansController.cs
│
├── Data
│   └── LibraryDbContext.cs
│
├── DTOs
│   ├── Books
│   │   ├── CreateBookRequest.cs
│   │   ├── UpdateBookRequest.cs
│   │   └── BookResponse.cs
│   ├── Members
│   │   ├── CreateMemberRequest.cs
│   │   ├── UpdateMemberRequest.cs
│   │   └── MemberResponse.cs
│   └── Loans
│       ├── CreateLoanRequest.cs
│       └── LoanResponse.cs
│
├── Models
│   ├── Book.cs
│   ├── Member.cs
│   └── Loan.cs
│
├── Services
│   ├── ServiceResult.cs
│   ├── IBookService.cs
│   ├── BookService.cs
│   ├── IMemberService.cs
│   ├── MemberService.cs
│   ├── ILoanService.cs
│   └── LoanService.cs
│
├── Migrations
├── Program.cs
└── appsettings.json
```

---

# API Endpoints

### Books

| Method | Endpoint          | Description       |
| ------ | ----------------- | ----------------- |
| GET    | `/api/books`      | Get all books     |
| GET    | `/api/books/{id}` | Get book by ID    |
| POST   | `/api/books`      | Create a new book |
| PUT    | `/api/books/{id}` | Update a book     |
| DELETE | `/api/books/{id}` | Delete a book     |

### Members

| Method | Endpoint            | Description         |
| ------ | ------------------- | ------------------- |
| GET    | `/api/members`      | Get all members     |
| GET    | `/api/members/{id}` | Get member by ID    |
| POST   | `/api/members`      | Create a new member |
| PUT    | `/api/members/{id}` | Update a member     |
| DELETE | `/api/members/{id}` | Delete a member     |

### Loans

| Method | Endpoint                      | Description                  |
| ------ | ----------------------------- | ---------------------------- |
| GET    | `/api/loans`                  | Get all loans                |
| GET    | `/api/loans/member/{memberId}`| Get loans by member          |
| POST   | `/api/loans/borrow`           | Borrow a book                |
| PUT    | `/api/loans/return/{loanId}`  | Return a book                |

# Example Payloads

### Create Book
```json
{
  "title": "Clean Code",
  "author": "Robert C. Martin",
  "isbn": "9780132350884",
  "publicationYear": 2008
}
```

### Create Member
```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane.doe@example.com",
  "phone": "555-1234"
}
```

### Borrow a Book
```json
{
  "bookId": 1,
  "memberId": 1
}
```

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

* Authentication and authorization (JWT)
* Unit and integration tests
* React frontend
* PostgreSQL or SQL Server support

---
