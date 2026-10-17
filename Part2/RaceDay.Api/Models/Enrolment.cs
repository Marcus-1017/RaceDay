namespace RaceDay.Api.Models;

public class Enrolment
{
    public int EnrolmentID { get; set; }
    public string EnrolmentStatus { get; set; } = "Pending";
    public int ParticipantID { get; set; }
    public int EventID { get; set; }
    public int CategoryID { get; set; }
    public User Participant { get; set; }
    public Event Event { get; set; }
    public Category Category { get; set; }
}