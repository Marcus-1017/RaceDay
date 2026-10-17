namespace RaceDay.Api.Models;

public class EventImage
{
    public int ImageID { get; set; }
    public string ImageURL { get; set; }
    public int EventID { get; set; }
    public Event Event { get; set; }
}