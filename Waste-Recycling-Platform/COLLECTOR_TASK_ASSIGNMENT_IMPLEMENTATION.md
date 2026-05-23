# Collector Task Assignment Feature Implementation

## Overview
This document describes the implementation of the Collector Task Assignment feature for the Waste Recycling Platform, addressing the requirements from the project management interface showing tasks WRP-87, WRP-88, WRP-89, and WRP-90.

## Tasks Completed

### ✅ WRP-87: Create CollectionTask Table
- **Status**: Already existed
- **Location**: [db/migrations/V1__create_base_tables.sql](db/migrations/V1__create_base_tables.sql)
- **Details**: 
  - `collection_tasks` table with proper foreign keys
  - Status enum: `assigned`, `on_the_way`, `collected`
  - Stores task metadata including collector assignment, weights, and completion data

### ✅ WRP-88: Create API for Collector Task Management
- **Status**: Partially existing, enhanced
- **Location**: [backend/src/WastePlatform.API/Controllers/CollectorTaskController.cs](backend/src/WastePlatform.API/Controllers/CollectorTaskController.cs)
- **Endpoints**:
  - `GET /api/collector/tasks` - List collector's assigned tasks with optional status filter
  - `PUT /api/collector/tasks/{id}/on-the-way` - Mark task as "on the way"
  - `PUT /api/collector/tasks/{id}/complete` - Complete task with weight, notes, and images
  - `GET /api/collector/tasks/stats` - Get collector dashboard statistics

### ✅ WRP-89: Allow Enterprises to Assign Collectors to Tasks
- **Status**: Newly implemented
- **Location**: [backend/src/WastePlatform.API/Controllers/EnterpriseTaskController.cs](backend/src/WastePlatform.API/Controllers/EnterpriseTaskController.cs)
- **Endpoints**:
  - `GET /api/enterprise/tasks` - List enterprise's collection tasks
    - Query params: `status` (filter by task status), `unassigned` (show only unassigned tasks)
  - `PUT /api/enterprise/tasks/{id}/assign-collector` - Assign collector to a task
    - Body: `{ collectorId: string }`
  - `GET /api/enterprise/tasks/collectors` - List available collectors for assignment
  - `GET /api/enterprise/tasks/stats` - Get enterprise task dashboard statistics

### ✅ WRP-90: Display List of Collection Tasks
- **Status**: Fully implemented (both collector and enterprise views)
- **Collector View**: CollectorTaskController endpoints listed above
- **Enterprise View**: EnterpriseTaskController endpoints listed above
- **Frontend**: New component `EnterpriseTaskManagement` created

## Backend Implementation

### New Controllers
1. **[EnterpriseTaskController.cs](backend/src/WastePlatform.API/Controllers/EnterpriseTaskController.cs)**
   - Full CRUD and assignment operations for enterprises
   - Includes statistics and filtering

### Application Layer (CQRS Pattern)
1. **Commands**:
   - [AcceptReportAndCreateTaskCommand.cs](backend/src/WastePlatform.Application/Reports/Commands/AcceptReportAndCreateTaskCommand.cs)
   - Handler for validating report acceptance

2. **Queries**:
   - [GetEnterpriseTasksQuery.cs](backend/src/WastePlatform.Application/Tasks/Queries/GetEnterpriseTasksQuery.cs)
   - [GetAvailableCollectorsQuery.cs](backend/src/WastePlatform.Application/Tasks/Queries/GetAvailableCollectorsQuery.cs)
   - [GetEnterpriseByUserIdQuery.cs](backend/src/WastePlatform.Application/Enterprise/Queries/GetEnterpriseByUserIdQuery.cs)

### Enhanced ReportController
- New endpoint: `POST /api/reports/{id}/accept` - Enterprise accepts a report and creates collection task
  - Validates report is pending
  - Creates collection task atomically
  - Updates report status to "Accepted"

## Frontend Implementation

### New API Client
- **[enterpriseTaskApi.ts](frontend/src/lib/api/enterpriseTaskApi.ts)**
  - TypeScript client for all enterprise task operations
  - Supports task listing, filtering, collector assignment, and statistics

### New Components
- **[EnterpriseTaskManagement.tsx](frontend/src/components/enterprise/EnterpriseTaskManagement.tsx)**
  - Complete task management interface for enterprises
  - Features:
    - Task list with detailed information
    - Status filtering
    - Unassigned tasks filter
    - Collector assignment modal
    - Real-time error handling
    - Task statistics display

### Updated Components
- **[EnterpriseDashboard.tsx](frontend/src/components/enterprise/EnterpriseDashboard.tsx)**
  - Added "Task Assignment" tab with new component
  - Integrated task management into main dashboard

## API Request/Response Examples

### List Enterprise Tasks
```bash
GET /api/enterprise/tasks?status=Assigned&unassigned=false
Authorization: Bearer {token}
```

**Response**:
```json
{
  "id": "task-uuid",
  "reportId": "report-uuid",
  "enterpriseId": "enterprise-uuid",
  "collectorId": "collector-uuid",
  "collectorName": "Collector Name",
  "status": "Assigned",
  "assignedAt": "2024-03-22T10:00:00Z",
  "report": {
    "id": "report-uuid",
    "description": "Plastic waste at location",
    "address": "123 Main St",
    "latitude": 10.7769,
    "longitude": 106.7009,
    "status": "Assigned",
    "categoryName": "Plastic",
    "citizenName": "Citizen Name",
    "citizenPhone": "0123456789"
  }
}
```

### Assign Collector to Task
```bash
PUT /api/enterprise/tasks/{taskId}/assign-collector
Content-Type: application/json
Authorization: Bearer {token}

{
  "collectorId": "collector-uuid"
}
```

### Accept Report and Create Task
```bash
POST /api/reports/{reportId}/accept
Authorization: Bearer {token}
```

**Response**:
```json
{
  "message": "Report accepted and collection task created successfully",
  "reportId": "report-uuid",
  "collectionTaskId": "task-uuid",
  "reportStatus": "Accepted"
}
```

## Database Changes
No new migrations required. The collection_tasks table was already created in V1__create_base_tables.sql with all necessary fields.

## Security & Authorization
- All enterprise endpoints require `Authorize(Roles = "Enterprise")`
- All collector endpoints require `Authorize(Roles = "Collector")`
- Enterprise can only see and manage their own tasks
- Collectors can only see tasks assigned to them

## Testing Checklist
- [ ] Test GET /api/enterprise/tasks endpoint with various filters
- [ ] Test assigning a collector to a task
- [ ] Test listing available collectors
- [ ] Test enterprise statistics
- [ ] Test collector task list and updates
- [ ] Test report acceptance creating collection task
- [ ] Test frontend UI renders correctly
- [ ] Test error handling for invalid inputs
- [ ] Test authorization (non-enterprise users can't access enterprise endpoints)

## Future Enhancements
1. Add real-time notifications when tasks are assigned
2. Implement task history and audit logs
3. Add performance metrics and analytics
4. Implement task reassignment workflows
5. Add collector availability scheduling
6. Implement SMS/push notifications for task assignments
7. Add geographic-based task recommendations
