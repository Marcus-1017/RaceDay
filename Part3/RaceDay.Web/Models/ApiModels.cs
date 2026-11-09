using RaceDay.Contracts;

namespace RaceDay.Web.Models;

public class EventItem
{
    public int EventID { get; set; }
    public string EventName { get; set; } = "";
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal? Distance { get; set; }
    public string EventType { get; set; } = "";
    public int OrganiserID { get; set; }
}

public class CategoryItem
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = "";
    public int EventID { get; set; }
}

public class EnrolmentItem
{
    public int EnrolmentID { get; set; }
    public string ParticipantName { get; set; } = "";
    public string CategoryName { get; set; } = "";
    public string EnrolmentStatus { get; set; } = "";
}

public class MyEnrolmentItem
{
    public int EnrolmentID { get; set; }
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string CategoryName { get; set; } = "";
    public string EnrolmentStatus { get; set; } = "";
}

public class ResultItem
{
    public string EventName { get; set; } = "";
    public DateTime EventDate { get; set; }
    public string CategoryName { get; set; } = "";
    public string? FinishTime { get; set; }
    public int? FinishPosition { get; set; }
}

public class ProfileItem
{
    public int UserID { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
}

public class DashboardRow
{
    public EventItem Event { get; set; } = new();
    public int Enrolments { get; set; }
}

public class ManageVm
{
    public EventItem Event { get; set; } = new();
    public List<CategoryItem> Categories { get; set; } = new();
    public List<EnrolmentItem> Enrolments { get; set; } = new();
}

public class DetailsVm
{
    public EventItem Event { get; set; } = new();
    public List<CategoryItem> Categories { get; set; } = new();
}

public class EventFormVm : EventRequest
{
    public int EventID { get; set; }
}