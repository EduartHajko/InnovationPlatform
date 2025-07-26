# Innovation Platform - Albanian Innovation Agency

A comprehensive web application for managing innovation applications and expert assignments for the Albanian Innovation Agency (AIE).

## Overview

The Innovation Platform is designed to streamline the process of submitting, reviewing, and managing innovation applications. It provides different interfaces for applicants, experts, and executives to efficiently handle the innovation pipeline.

## Features

### For Applicants
- Submit innovation applications with detailed descriptions
- Upload supporting documents (up to 5 files)
- Track application status
- View application history
- Applicant can view only his aplications if he is loged in if is not loged in when creating an application only admin can view them and he can not view it so a simple user can send applications without being loged in

### For Experts
- Review assigned applications
- Add notes and feedback
- Track applications in mentoring phase

### For Executives
- Comprehensive dashboard with KPIs and analytics
- Manage all applications with filtering and search
- Assign experts to applications (individual and bulk)
- Update application statuses
- Add internal notes
- Delete applications when necessary

## Technology Stack

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQLite with Entity Framework Core
- **Frontend**: Bootstrap 5, HTML5, CSS3, JavaScript
- **Authentication**: Custom simple authentication system
- **File Storage**: Local file system

## Project Structure

```
InnovationPlatform/
├── Controllers/           # MVC Controllers
├── Models/               # Data models and entities
├── Views/                # Razor views and templates
├── Data/                 # Database context and configurations
├── wwwroot/              # Static files (CSS, JS, uploads)
├── bin/                  # Compiled binaries
└── obj/                  # Build artifacts
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Installation
1. Clone the repository
2. Navigate to the project directory
3. Restore NuGet packages: `dotnet restore`
4. You can delete courrent db and app will recreate one on startup or you can use courrent'
5. Run the application: `dotnet run`
6. Open browser to `https://localhost:5001`

### Default Users
The system includes test users that can be created via the debug endpoints:
- **Executive**: admin@aie.gov.al / admin123
- **Experts**: expert@aie.gov.al / expert123
- **Applicant**: applicant@aie.gov.al / applicant123

## Database

The application uses SQLite database with the following main entities:
- **Applications**: Innovation applications with status tracking
- **Users**: Simple user management with roles
- **Categories**: Innovation categories
- **Notes**: Internal notes for applications
- **ApplicationFiles**: File attachments

## Configuration

Key settings can be modified in `appsettings.json`:
- Database connection string
- File upload limits
- Application settings

## Contributing

This project is developed for the Albanian Innovation Agency. For contributions or issues, please contact the development team.

## License

Proprietary software for Albanian Innovation Agency (AIE).
