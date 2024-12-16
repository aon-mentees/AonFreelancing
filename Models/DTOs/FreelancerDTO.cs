using AonFreelancing.Utilities;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
   
    public class FreelancerResponseDTO : UserResponseDTO
    {
        public List<SkillOutputDTO>Skills { get; set; }
        FreelancerResponseDTO(Freelancer freelancer, string imageBaseUrl)
        {
            Id = freelancer.Id;
            Name = freelancer.Name;
            Username = freelancer.UserName;
            Email = freelancer.Email;
            About = freelancer.About;
            PhoneNumber = freelancer.PhoneNumber;
            UserType = Constants.USER_TYPE_FREELANCER;
            IsPhoneNumberVerified = freelancer.PhoneNumberConfirmed;
            Skills = freelancer.Skills.Select(s => SkillOutputDTO.FromSkill(s)).ToList();
            //Role = new RoleResponseDTO { Name = Constants.USER_TYPE_FREELANCER };
            if(freelancer.ProfilePicture != null )
                ProfilePicture = $"{imageBaseUrl}/{freelancer.ProfilePicture}";
        }
        public static FreelancerResponseDTO FromFreelancer(Freelancer freelancer, string imageBaseUrl)=> new FreelancerResponseDTO(freelancer, imageBaseUrl);
    }

    public class FreelancerShortOutDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string QualificationName { get; set; }
        public string ProfilePicture { get; set; }
        FreelancerShortOutDTO(Freelancer freelancer, string imageBaseUrl)
        {
            Id = freelancer.Id;
            Name = freelancer.Name;
            if(freelancer.ProfilePicture != null)
            ProfilePicture = $"{imageBaseUrl}/{freelancer.ProfilePicture}";
            
        }
        public static FreelancerShortOutDTO FromFreelancer(Freelancer freelancer, string imageBaseUrl) => new FreelancerShortOutDTO(freelancer, imageBaseUrl);
    }

    public class FreelancerUpdateDTO
    {
        [StringLength(64, ErrorMessage = "The Name cannot exceed 64 characters")]
        public string Name { get; set; }

        [StringLength(128, ErrorMessage = "The SpecializationName cannot exceed 128 characters")]
        public string QualificationName { get; set; }
    }
}
