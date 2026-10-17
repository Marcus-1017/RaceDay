namespace RaceDay.Api.Models;

public class ProfileImage
{
    public int ImageID { get; set; }
    public string ImageURL { get; set; }
    public int UserID { get; set; }
    public User User { get; set; }
}