# KIEM Postman Test Map

Use this map to run Postman Desktop by task key. Keep Jira cloud updates manual.

## General Rule

- Test only the requests listed for your KIEM key.
- If a request needs a token, run the matching login request first.
- Do not use the `99 - Jira Sync` folder when you are doing the manual Jira update flow.

## KIEM-4: Auth

Recommended requests in `01 - Auth`:
- `GET Roles`
- `POST Register Citizen`
- `POST Register Enterprise`
- `POST Login Citizen`
- `POST Login Enterprise`

Purpose:
- Registration, login, and token capture for citizen and enterprise flows.

## KIEM-14: Collector

Recommended requests:
- `01 - Auth` -> `POST Login Collector`
- `07 - Collector API` -> `GET Collector Profile`
- `07 - Collector API` -> `PATCH Toggle Availability`
- `08 - Collector Tasks` -> `GET My Tasks`
- `08 - Collector Tasks` -> `GET Task By ID`
- `08 - Collector Tasks` -> `PUT Set On The Way`
- `08 - Collector Tasks` -> `PUT Complete Task`
- `08 - Collector Tasks` -> `GET Task Stats`

Purpose:
- Collector identity, availability, task lifecycle, and task summary.

## KIEM-16: Enterprise Task Flow

Recommended requests:
- `01 - Auth` -> `POST Login Admin` only if you need `adminToken` / `managementToken`
- `09 - Enterprise API` -> `GET Task Stats`
- `09 - Enterprise API` -> `GET Task Progress`
- `09 - Enterprise API` -> `GET Available Collectors`
- `09 - Enterprise API` -> `PUT Assign Collector`

Optional enterprise setup requests if your test needs them:
- `GET Enterprise Profile`
- `PUT Enterprise Profile`
- `GET Waste Types`
- `PUT Waste Types`
- `GET Collectors`
- `POST Create Collector`
- `PUT Update Collector`
- `DELETE Collector`

Purpose:
- Task assignment, progress tracking, and collector lookup for enterprise-side flow.

## KIEM-17: Reward Rules

Recommended requests:
- `01 - Auth` -> `POST Login Admin` only if you need `adminToken` / `managementToken`
- `09 - Enterprise API` -> `GET Reward Rules`
- `09 - Enterprise API` -> `PUT Reward Rules`

Purpose:
- View and update enterprise reward rules.

## KIEM-19: Notifications

Recommended requests:
- `01 - Auth` -> `POST Login Admin` only if you need `adminToken` / `managementToken`
- `05 - Notifications` -> `GET Notifications`
- `05 - Notifications` -> `GET Unread Count`
- `05 - Notifications` -> `PUT Mark Notification Read`
- `05 - Notifications` -> `PUT Mark All Notifications Read`

Purpose:
- Read and clear citizen notifications.

## Manual Jira Note

If you need to update Jira, do it manually in Jira Cloud after your Postman run. The collection is kept focused on API testing, not on auto-posting Jira comments or transitioning Jira issues.
