namespace AonFreelancing.Models.DTOs
{
    public class SkillOutputDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        SkillOutputDTO(Skill skill)
        {
            Id = skill.Id;
            Name = skill.Name;
        }
        public static SkillOutputDTO FromSkill(Skill skill) => new SkillOutputDTO(skill);
    }
}