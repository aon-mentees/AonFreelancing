namespace AonFreelancing.Models.DTOs
{
    public class UserDetailsDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public string ProfilePicture {  get; set; }
        public UserDetailsDTO(User user,string role, string imageBasePath)
        {
            Id = user.Id;
            Name = user.Name;
            Username = user.UserName;
            PhoneNumber = user.PhoneNumber;
            Role = role;
            ProfilePicture = $"{imageBasePath}/{user.ProfilePicture}";
        }
    }
}
