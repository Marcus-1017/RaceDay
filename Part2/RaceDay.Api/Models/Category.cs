namespace RaceDay.Api.Models;

public class Category
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; }
    public int EventID { get; set; }
    public Event Event { get; set; }
}