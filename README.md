# MicroFin Plus - Microfinance Application

A comprehensive ASP.NET Core MVC application for microfinance institutions to manage clients, loans, savings accounts, payments, and transactions. This application provides a robust platform for financial inclusion and helps empower communities through accessible financial services.

## Features

### Client Management
- Register and manage client profiles
- Track client financial history
- KYC (Know Your Customer) documentation

### Loan Services
- Process loan applications
- Configure loan terms and interest rates
- Track loan disbursements and repayments
- Automated payment reminders

### Savings Accounts
- Open and manage savings accounts
- Competitive interest rates
- Account statements and transaction history

### Payments & Transactions
- Record all financial transactions
- Multiple payment methods supported
- Transaction verification and receipts
- Audit trail for all financial activities

### Reporting & Analytics
- Financial performance metrics
- Client portfolio analysis
- Risk assessment tools
- Export reports in multiple formats

### Security & Compliance
- Role-based access control with ASP.NET Core Identity
- Data encryption for sensitive information
- Audit logging for security monitoring
- Compliance with financial regulations

## Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Authentication**: ASP.NET Core Identity
- **Database**: Entity Framework Core with SQL Server/SQLite
- **Frontend**: Bootstrap 5, Font Awesome, jQuery
- **UI Components**: Custom components, responsive design
- **IDE**: Visual Studio 2022 or Visual Studio Code

## Code Structure

### Controllers
- `AccountController.cs`: Handles user authentication and account management
- `ClientsController.cs`: Manages client information and operations
- `ContactController.cs`: Processes contact form submissions
- `HomeController.cs`: Handles main navigation and dashboard
- `LoansController.cs`: Manages loan applications and processing
- `PaymentsController.cs`: Handles payment recording and tracking
- `SavingsController.cs`: Manages savings account operations
- `TransactionsController.cs`: Handles financial transaction recording

### Models
- `ApplicationUser.cs`: Extended identity user model
- `Client.cs`: Client information model
- `Loan.cs`: Loan application and management model
- `Payment.cs`: Payment transaction model
- `SavingsAccount.cs`: Savings account model
- `Transaction.cs`: Financial transaction model

### Data
- `ApplicationDbContext.cs`: Entity Framework DbContext
- `DatabaseConfigHelper.cs`: Database configuration with fallback options
- `DbInitializer.cs`: Database seeding and initialization

### Views
- Organized by controller with full CRUD operations
- Shared components for consistent UI
- Responsive design for mobile and desktop

### ViewComponents
- `MainNavigationViewComponent`: Implements dynamic navigation menu
- Reusable components across multiple views

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- SQL Server 2019 Express/LocalDB or SQLite
- Visual Studio 2022 or Visual Studio Code

### Database Configuration

The application supports multiple database providers:
1. **SQL Server**: Primary database for production
2. **SQLite**: Fallback database for development and testing

The `DatabaseConfigHelper.cs` file provides automatic fallback to SQLite if SQL Server connection fails.

### Installation

1. Clone the repository
2. Restore packages: `dotnet restore`
3. Update database: `dotnet ef database update`
4. Run the application: `dotnet run`

### Configuration

Database connection can be configured in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=MicrofinanceApp;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

For SQLite configuration:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=MicrofinanceApp.db"
}
```

## Development Utilities

Several utility scripts are included to help with development:

- `RunApp.bat`: Starts the application
- `TestDatabaseConnection.bat`: Tests database connectivity
- `SetupLocalDB.bat`: Configures SQL Server LocalDB
- `SetupSQLiteBackup.bat`: Switches to SQLite database
- `ResetSQLiteDatabase.bat`: Resets the SQLite database to initial state

## Architecture

The application follows standard ASP.NET Core MVC architecture with:

- Clean separation of concerns
- Repository pattern for data access
- Service layer for business logic
- SOLID principles throughout the codebase

## API Endpoints

The application exposes the following API endpoints for integration:

### Client APIs
- `GET /api/clients` - Retrieve list of clients
- `GET /api/clients/{id}` - Retrieve specific client
- `GET /api/clients/{id}/accounts` - Retrieve client accounts
- `POST /api/clients` - Create new client

### Transaction APIs
- `GET /api/transactions` - Retrieve list of transactions
- `POST /api/transactions` - Record new transaction

### Reporting APIs
- `GET /api/reports/loans` - Generate loan performance report
- `GET /api/reports/payments` - Generate payment status report

## UI Components

Custom UI components include:
- `MainNavigationViewComponent`: Responsive navigation menu
- Card-based dashboard layouts
- Interactive form elements with validation
- Responsive tables for data display

## Screenshots

### Dashboard
![Dashboard](wwwroot/images/screenshots/dashboard.png)
*The main dashboard provides an overview of financial performance.*

### Client Management
![Client Management](wwwroot/images/screenshots/clients.png)
*Manage client information with comprehensive profiles.*

### Loan Management
![Loan Management](wwwroot/images/screenshots/loans.png)
*Process and track loans with detailed repayment schedules.*

## Troubleshooting

### Login Credentials
Default admin credentials:
- Email: admin@microfinance.app
- Password: Admin123!

### Database Connectivity
If you experience database connectivity problems:

1. Visit `/Home/Diagnostics` or `/db-test` to check database status
2. Review `DatabaseTroubleshooting.md` for common solutions
3. Try running the database setup scripts provided in the repository

## Deployment

The application can be deployed to various environments:

### Azure App Service
1. Create an Azure App Service
2. Set up CI/CD pipeline with GitHub Actions
3. Configure connection strings in Azure App Service configuration

### Docker Deployment
```bash
docker build -t microfinplus .
docker run -p 8080:80 microfinplus
```

### IIS Deployment
1. Publish the application using Visual Studio
2. Set up an IIS website pointing to the published folder
3. Configure application pool settings

## License

[MIT License](LICENSE)

## Contact

For support or inquiries, please contact us at support@microfinplus.com


