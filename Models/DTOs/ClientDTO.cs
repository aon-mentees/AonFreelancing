using AonFreelancing.Utilities;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class ClientDTO : UserOutDTO
    {
        public string CompanyName { get; set; }
        public IEnumerable<ProjectOutDTO> Projects { get; set; }
    }

    public class ClientInputDTO : UserDTO
    {
        [Required]
        [MinLength(4, ErrorMessage = "Invalid Company Name")]
        public string CompanyName { get; set; }
    }

    public class ClientResponseDTO : UserResponseDTO
    {
        public string CompanyName { get; set; }
        public IEnumerable<ProjectHistoryDTO>? Projects { get; set; }

        ClientResponseDTO(Client client)
        {
            Id = client.Id;
            Name = client.Name;
            Username = client.UserName;
            PhoneNumber = client.PhoneNumber;
            Email = client.Email;
            About = client.About;
            UserType = Constants.USER_TYPE_CLIENT;
            IsPhoneNumberVerified = client.PhoneNumberConfirmed;
            CompanyName = client.CompanyName;
            Projects = client.Projects.Select(p => ProjectHistoryDTO.FromProject(p));
            //Role = new RoleResponseDTO { Name = Constants.USER_TYPE_CLIENT };
        }
        public static ClientResponseDTO FromClient(Client client) => new ClientResponseDTO(client);
    }
}
