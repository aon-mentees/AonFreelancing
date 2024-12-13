using AonFreelancing.Utilities;

namespace AonFreelancing.Models.DTOs
{
   
    public class FreelancerResponseDTO : UserResponseDTO
    {
        public List<SkillOutDTO> Skills { get; set; }
        FreelancerResponseDTO(Freelancer freelancer)
        {
            Id = freelancer.Id;
            Name = freelancer.Name;
            Username = freelancer.UserName;
            PhoneNumber = freelancer.PhoneNumber ?? string.Empty;
            Email = freelancer.Email;
            About = freelancer.About;
            PhoneNumber = freelancer.PhoneNumber;
            UserType = Constants.USER_TYPE_FREELANCER;
            IsPhoneNumberVerified = freelancer.PhoneNumberConfirmed;
            Skills = freelancer.Skills.Select(s => SkillOutDTO.FromSkill(s)).ToList();
            Role = new RoleResponseDTO { Name = Constants.USER_TYPE_FREELANCER };
        }
        public static FreelancerResponseDTO FromFreelancer(Freelancer freelancer)=> new FreelancerResponseDTO(freelancer);
    }

    public class FreelancerShortOutDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string QualificationName { get; set; }

        FreelancerShortOutDTO(Freelancer freelancer)
        {
            Id = freelancer.Id;
            Name = freelancer.Name;
        }
        public static FreelancerShortOutDTO FromFreelancer(Freelancer freelancer) => new FreelancerShortOutDTO(freelancer);
    }
}
