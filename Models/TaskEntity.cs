using AonFreelancing.Models.DTOs;

namespace AonFreelancing.Models
{
    public class TaskEntity
    {
        // StartredAt
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public Project? Project { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime? DeadlineAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; }
        public DateTime? StartedAt { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public TaskEntity() { }
        TaskEntity(TaskInputDTO inputDTO,long projectId)
        {
            ProjectId = projectId;
            Name = inputDTO.Name;
            DeadlineAt = inputDTO.DeadlineAt;
            Notes = inputDTO.Notes;
        }
        public static TaskEntity FromInputDTO(TaskInputDTO inputDTO, long projectId) => new TaskEntity(inputDTO, projectId);
        public void UpdateFromInputDTO(TaskInputDTO inputDTO)
        {
            Name = inputDTO.Name;
            Notes = inputDTO.Notes;
            DeadlineAt = inputDTO.DeadlineAt;
        }
    }
}
