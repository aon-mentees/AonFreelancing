using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class ClientDTO:UserOutDTO
    {
        public string CompanyName { get; set; }

        // Has many projects, 1-m
        public IEnumerable<ProjectOutDTO> Projects { get; set; }
    }

    public class ClientInputDTO: UserDTO
    {
        [Required]
        [MinLength(4,ErrorMessage ="Invalid Company Name")]
        public string CompanyName { get; set; }
    }

    public class ClientResponseDTO : UserResponseDTO
    {
        public string CompanyName { get; set; }
        public IEnumerable<ProjectDetailsDTO>? Projects { get; set; }
    }

    public class ClientUpdateDTO
    {
        [StringLength(64, ErrorMessage = "The Name cannot exceed 64 characters.")]
        public string Name { get; set; }

        [StringLength(128, ErrorMessage = "The CompanyName cannot exceed 128 characters.")]
        public string CompanyName { get; set; }
    }
}
