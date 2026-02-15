# Gym-Management-System-WPF-PostgreSQL-
A desktop application for managing gym operations built with C# (WPF) and PostgreSQL. Features role-based access, dynamic SQL generation, and database schema management via UI. Demonstrates raw ADO.NET.

## Key Features

### 1. Role-Based Access Control
* **Administrator:** Full access to CRUD operations, payroll calculation, and database structure modification.
* **Guest/ReadOnly:** Safe view-only mode with advanced filtering capabilities.

### 2. Database Schema Management (DDL)
* Unique feature allowing administrators to **add, remove, and rename columns** in the database directly from the application UI.
* Includes safety guardrails (e.g., preventing the deletion of Primary Keys).

### 3. Advanced Search & Filtering
* **Dynamic SQL Generation:** The search engine switches logic based on user input.
* **Exact Match:** Uses standard equality operators.
* **Fuzzy Search:** Implements PostgreSQL `ILIKE` for case-insensitive partial matching.

### 4. Security & Data Integrity
* **SQL Injection Prevention:** All database interactions use parameterized queries (`NpgsqlCommand`).
* **Data Validation:** Database-level constraints using **Regex** (for emails, phone numbers) and **CHECK constraints** (e.g., preventing negative prices or conflicting dates).

## Tech Stack

* **Language:** C# (.NET 6/8)
* **UI Framework:** WPF (Windows Presentation Foundation)
* **Database:** PostgreSQL
* **Data Access:** Npgsql (ADO.NET provider)
* **Tools:** Visual Studio, pgAdmin

## Database Architecture

The database is designed according to the **3rd Normal Form (3NF)** to ensure data integrity and reduce redundancy.

* **Logic in Database:** Utilizes SQL Views (e.g., `v_wyplaty_pracownikow`) to calculate employee payrolls automatically based on work logs.
* **Sensitive Data Isolation:** Employee financial data (PESEL, bank accounts) is separated into a `dane_kadrowe` table for security.

## Why Raw SQL / ADO.NET?

I intentionally chose **ADO.NET** over Entity Framework for this project to demonstrate:
1.  A strong understanding of how the application connects to the database "under the hood."
2.  Proficiency in writing complex SQL queries manually.
3.  Knowledge of database resource management (using statements, closing connections).

## How to Run

1.  Clone the repository.
2.  Import the `schema.sql` file into your local PostgreSQL instance.
3.  Update the `connectionString` in `Functions.cs`.
4.  Build and run the solution in Visual Studio.