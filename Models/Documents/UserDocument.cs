namespace AonFreelancing.Models.Documents;

public class UserDocument
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? About { get; set; }
    public string ProfilePicture { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    public UserDocument()
    {
    }

    public UserDocument(User user)
    {
        Id = user.Id;
        Name = user.Name;
        About = user.About;
        ProfilePicture = user.ProfilePicture;
        UserName = user.UserName;
        Email = user.Email;
        PhoneNumber = user.PhoneNumber;
    }
}