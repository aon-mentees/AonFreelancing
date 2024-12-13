using AonFreelancing.Models.DTOs;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("Skills")]
    public class Skill
    {
        public long Id { get; set; }
        public long FreelancerId { get; set; }
        public string Name { get; set; }
        public Skill() { }
        Skill(SkillInputDTO inputDTO, long userId)
        {
            Name = inputDTO.Name;
            FreelancerId = userId;
        }
        public static Skill FromInputDTO(SkillInputDTO inputDTO, long userId) => new Skill(inputDTO, userId);
    }
}
