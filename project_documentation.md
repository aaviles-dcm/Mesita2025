# HelpDesk System Documentation

This document provides a comprehensive overview of the HelpDesk System, including its architecture, technologies, features, and technical details. It is intended to be a living document, updated with each significant change to the codebase.

## 1. Technologies and Architecture

### Architecture
The solution follows a **Client-Server architecture** using modern .NET technologies:

- **Frontend**: Blazor Server (`HelpDesk.Blazor`)
    - Provides a rich, interactive user interface.
    - Communicates with the backend API via HTTP requests.
    - Uses server-side rendering with SignalR for real-time UI updates.
- **Backend**: ASP.NET Core Web API (`HelpDesk.Api`)
    - Exposes RESTful endpoints for data management.
    - Handles business logic and database interactions.
- **Data Layer**: Entity Framework Core (`HelpDesk.Data`)
    - Defines the database schema using Code-First approach.
    - Manages relationships between entities (Users, Tickets, Categories, WorkLogs).
- **Database**: SQLite
    - Lightweight, file-based relational database for development and small-scale deployments.

### Technology Stack
- **Framework**: .NET 8 / 9 (depending on specific project configuration)
- **UI Framework**: Blazor Server
- **Styling**: Bootstrap 5 with custom dark theme CSS
- **ORM**: Entity Framework Core
- **Database**: SQLite
- **Authentication**: Custom Cookie-based authentication with ClaimsPrincipal
- **HTTP Client**: `System.Net.Http.HttpClient` for API communication

## 2. Main Features and Business Logic

### Core Features
- **Dashboard**:
    - Real-time statistics (Total Tickets, Open Tickets, Resolved This Week).
    - "Recent Tickets" list filtered by user role.
- **Ticket Management**:
    - **Create**: Users can submit new tickets with Title, Description, Category, and Priority.
    - **View**: Detailed view of ticket information, including status, assignee, and history.
    - **Edit**: Engineers and Admins can update ticket details.
- **Workflow & Statuses**:
    - Supported Statuses: `New`, `InProgress`, `Resolved`, `Closed`, `Cancelled`, `OnHold`, `Reopened`.
    - **Automatic Assignment**: New tickets are automatically assigned to an engineer based on the selected Category.
- **Audit Trail**:
    - Tracks all actions (comments, status changes, assignments) in a `WorkLog`.
    - Displays a timeline of activity in the Ticket Details view.

### Business Logic & Roles
The system enforces Role-Based Access Control (RBAC) with three distinct roles:

#### **User**
- **View**: Can only see tickets they created.
- **Create**: Can submit new tickets.
- **Actions**:
    - **Cancel**: Can cancel their own tickets if status is `New`, `InProgress`, or `OnHold`.
    - **Close**: Can close their own tickets if status is `Resolved`.
    - **Reopen**: Can reopen their own tickets if status is `Resolved` or `Closed`.

#### **Engineer**
- **View**: Can see tickets assigned to them OR unassigned tickets with `New` status.
- **Actions**:
    - **Take Ticket**: Can assign unassigned tickets to themselves.
    - **Work**: Can change status to `InProgress`, `OnHold`, `Resolved`, or `Cancelled`.
    - **Solution**: Can provide a solution summary.

#### **Administrator**
- **View**: Can see ALL tickets in the system.
- **Actions**:
    - **Full Control**: Can perform ANY operation on ANY ticket, regardless of its status (even `Closed` or `Cancelled`).
    - **Overrides**:
        - Manually change Status to any value.
        - Manually change Priority.
        - Reassign tickets to specific engineers.
        - Add comments to closed tickets.

## 3. Technical Documentation

### API Endpoints
- **`GET /api/dashboard`**: Returns stats and recent tickets. Supports filtering by `userId` and `role`.
- **`GET /api/tickets`**: Returns a list of tickets. Supports filtering.
- **`POST /api/tickets`**: Creates a new ticket. Triggers auto-assignment logic.
- **`PUT /api/tickets/{id}`**: Updates ticket details.
- **`PUT /api/tickets/{id}/assign`**: Assigns a ticket to an engineer.
- **`GET /api/worklogs/ticket/{id}`**: Retrieves the audit trail for a ticket.
- **`POST /api/worklogs`**: Adds a new entry to the audit trail.

### Key Components
- **`Dashboard.razor`**:
    - Fetches data from `DashboardController`.
    - Implements role-based filtering logic in `OnInitializedAsync`.
- **`TicketDetails.razor`**:
    - Complex component handling ticket display, tabs (Description, Audit, Solution), and action buttons.
    - Contains extensive logic to show/hide buttons based on `currentUserRole` and `ticket.Status`.
- **`TicketsController.cs`**:
    - `PostTicket`: Contains the logic for **Automatic Assignment** (finding the first engineer in the target category).
    - `GetTickets`: Implements the backend filtering for roles.

### Database Schema (Key Entities)
- **Ticket**: `TicketId`, `Title`, `Description`, `Status`, `Priority`, `CategoryId`, `CreatedById`, `AssignedEngineerId`.
- **User**: `UserId`, `DisplayName`, `Role` (Enum), `Categories` (Many-to-Many).
- **Category**: `CategoryId`, `Name`, `Engineers` (Many-to-Many).
- **WorkLog**: `LogId`, `TicketId`, `EngineerId` (Actor), `Description`, `StartTime`.

### Recent Updates
- **Restricted Dashboard**: Dashboard now filters tickets based on the logged-in user's role.
- **Automatic Assignment**: New tickets are auto-assigned to an engineer associated with the selected category.
- **Admin Overrides**: Administrators have full control to change status, priority, and comment on closed tickets.
- **User Actions**: Users can now Cancel `New`/`InProgress` tickets and Reopen `Closed` tickets.
- **Real-time Updates**: Implemented SignalR to push ticket updates (status changes, comments, assignments) to all connected clients instantly without page reload.
