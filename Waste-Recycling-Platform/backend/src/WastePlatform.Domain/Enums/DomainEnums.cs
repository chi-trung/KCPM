namespace WastePlatform.Domain.Enums;

public enum UserRole
{
    Citizen,
    Enterprise,
    Collector,
    Admin
}

public enum ReportStatus
{
    Pending,
    Accepted,
    Assigned,
    Collected,
    Rejected
}

public enum CollectionTaskStatus
{
    Assigned,
    OnTheWay,
    Collected
}

public enum ComplaintStatus
{
    Open,
    InProgress,
    Resolved,
    Rejected,
    Escalated
}
