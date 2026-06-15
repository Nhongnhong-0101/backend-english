# AI-Powered English Learning Platform - Backend

## Overview

This project is the backend system of an AI-powered English learning platform developed as my graduation project.

The system provides English learners with a personalized learning environment, including vocabulary management, grammar learning, speaking practice, and AI chatbot interaction. I was responsible for designing the database, defining the system architecture, and developing the entire backend application.

The backend is built using ASP.NET Core with a layered architecture to ensure maintainability, scalability, and easier integration of future features.

---

## Key Features

### User Management & Authentication

* User registration and login.
* JWT-based authentication and authorization.
* User profile management.

### Vocabulary Learning

* Search English words and save favorite vocabulary.
* Create and manage custom word sets.
* Learn vocabulary using the flashcard method.

### English Learning Resources

* Search idioms and irregular verbs.
* Access grammar learning paths and speaking lessons.

### AI Features

* AI chatbot integration for English conversation practice.
* Speech-to-text functionality to support speaking exercises.

---

## System Architecture

The project follows a layered architecture:

```
API Layer (Controllers)
          |
          v
Service Layer (Business Logic)
          |
          v
Repository Layer (Data Access)
          |
          v
PostgreSQL Database
```

Main architectural concepts:

* Dependency Injection (DI).
* Repository Pattern.
* RESTful API design.
* DTO-based Request/Response handling.

---

## Technology Stack

### Backend

* C#
* ASP.NET Core (.NET 8)
* RESTful API
* JWT Authentication

### Database

* PostgreSQL
* Dapper
* SQL

### Development Tools

* Swagger/OpenAPI
* Git & GitHub

### External Services

* AI API integration for chatbot and speech-to-text.

---

## My Responsibilities

As the backend developer of this project, I was responsible for:

* Analyzing requirements and designing the backend architecture.
* Designing relational database schemas.
* Developing RESTful APIs using ASP.NET Core.
* Implementing authentication and authorization using JWT.
* Developing data access layers with Dapper and PostgreSQL.
* Integrating AI services for chatbot communication and speech processing.
* Testing APIs and documenting endpoints using Swagger.

---

## API Documentation

API endpoints can be tested and explored through Swagger UI after running the application.

---

## Future Improvements

Potential improvements for this project include:

* Implementing global exception handling middleware.
* Adding logging and monitoring.
* Containerizing the application using Docker.
* Improving API performance with caching mechanisms.
* Writing unit and integration tests.
