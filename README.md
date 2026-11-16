# 🚌 Bus Booking Application

## 📌 About The Project

Bus Booking Application is a full-stack web application developed to simplify the bus ticket booking process. Users can register, log in, book bus tickets through a booking form, and view their previous bookings.

The project is developed using **Angular** for the frontend, **ASP.NET Core Web API** for the backend, and **Microsoft SQL Server** as the database. Secure authentication is handled using **JWT (JSON Web Tokens)**.

## ✨ Features

- User registration and login
- Secure authentication using JWT
- Bus ticket booking form
- View personal bookings (My Bookings)
- RESTful API built with ASP.NET Core
- Frontend and backend integration

## 🛠️ Technologies Used

### Frontend
- Angular
- TypeScript
- HTML5
- CSS3

### Backend
- ASP.NET Core Web API
- C#
- Entity Framework Core
- JWT Authentication

### Database
- Microsoft SQL Server (SQL Server Express)

### Tools
- Visual Studio Code
- Git & GitHub

## 📂 Project Structure

```text
busBookingApp/
│
├── BusBookingApi/            # ASP.NET Core Web API (Backend)
│   ├── Controllers/          # API controllers
│   ├── Data/                 # DbContext and EF Core migrations
│   ├── DTOs/                 # Data transfer objects
│   ├── Models/               # Entity models
│   ├── Services/             # Business logic services
│   ├── Program.cs
│   ├── appsettings.json      # DB connection string, JWT & logging config
│   └── BusBookingApi.csproj
│
├── public/                   # Public assets
│
├── src/                      # Angular Frontend
│   ├── app/
│   │   ├── config/
│   │   ├── guards/           # Route guards
│   │   ├── models/
│   │   ├── services/
│   │   ├── pages/
│   │   │   ├── booking-form/ # Bus ticket booking form
│   │   │   ├── home/         # Home page
│   │   │   ├── my-bookings/  # User's booking history
│   │   │   └── register/     # User registration
│   │   ├── app.html
│   │   ├── app.routes.ts
│   │   └── app.ts
│   ├── index.html
│   ├── main.ts
│   └── styles.css
│
├── angular.json
├── package.json
├── tsconfig.json
├── .gitignore
└── README.md
```

## 🗄️ Database Configuration

This project uses **Microsoft SQL Server**. The connection string is configured in `BusBookingApi/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS;Database=BusBookingDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

> If your SQL Server instance has a different name (for example `(localdb)\\MSSQLLocalDB`), change the `Server` value accordingly.

> ⚠️ Never commit secrets such as the JWT secret key. Use .NET User Secrets or environment variables instead.

## ⚙️ Installation and Setup

### Prerequisites

- [Node.js](https://nodejs.org/) (LTS)
- [Angular CLI](https://angular.dev/tools/cli)
- [.NET SDK](https://dotnet.microsoft.com/download)
- [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) (or LocalDB)

### 1. Clone Repository

```bash
git clone https://github.com/VaruRoshani/busBookingApp.git
cd busBookingApp
```

### 2. Database Setup

Install the EF Core tool (only once):

```bash
dotnet tool install --global dotnet-ef
```

Create the database and tables:

```bash
cd BusBookingApi
dotnet ef database update
```

### 3. Backend Setup

Restore dependencies:

```bash
dotnet restore
```

Run the backend:

```bash
dotnet run
```

### 4. Frontend Setup

Open a new terminal in the project root and install dependencies:

```bash
npm install
```

Run Angular application:

```bash
ng serve
```

Open in browser:

```text
http://localhost:4200
```

## 👩‍💻 Author

**Roshani Varu**

- GitHub: [VaruRoshani](https://github.com/VaruRoshani)