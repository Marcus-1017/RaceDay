namespace RaceDay.Api.Models;

public class Event
{
    public int EventID { get; set; }
    public string EventName { get; set; }
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; }
    public decimal? Distance { get; set; }
    public string EventType { get; set; }
    public int OrganiserID { get; set; }
    public User Organiser { get; set; }
}
