# Notifications Test Package

This folder contains the complete unit test package for the notifications feature.

## Coverage

- `NotificationServiceTests.cs`
  - creates notification payloads correctly
  - pushes realtime notifications
  - handles reject messages with and without a reason
  - saves admin escalation notifications without realtime push

- `NotificationControllerTests.cs`
  - validates citizen access
  - validates paging input
  - returns unread count
  - marks one notification as read
  - marks all notifications as read
  - returns `404` when a notification does not exist

- `NotificationRepositoryTests.cs`
  - persists notifications
  - queries by citizen with sorting and paging
  - filters by status
  - counts unread notifications
  - marks one notification as read
  - marks all notifications as read

## Run

```bash
dotnet test .\Waste-Recycling-Platform\backend\tests\WastePlatform.Tests\WastePlatform.Tests.csproj --filter Notification
```

## Expected Result

- All notification tests should pass.
- The command should cover service, controller, and repository tests in one run.