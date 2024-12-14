namespace AonFreelancing.Models.DTOs.NoftificationDTOs
{
    public class TaskRejectionNotificationOutputDTO : NotificationOutputDTO
    {
        public long ProjectId { get; set; }
        public long RejectorId { get; set; }
        public string RejectorName { get; set; }
        public long TaskId { get; set; } // spare

        TaskRejectionNotificationOutputDTO(TaskRejectionNotification taskRejectionNotification) : base(taskRejectionNotification.Id, taskRejectionNotification.Title, taskRejectionNotification.Message, taskRejectionNotification.IsRead, taskRejectionNotification.CreatedAt)
        {
            ProjectId = taskRejectionNotification.ProjectId;
            RejectorId = taskRejectionNotification.RejectorId;
            RejectorName = taskRejectionNotification.RejectorName;
            TaskId = taskRejectionNotification.TaskId;
        }
        public static TaskRejectionNotificationOutputDTO FromTaskRejectionNotification(TaskRejectionNotification taskRejectionNotification) => new TaskRejectionNotificationOutputDTO(taskRejectionNotification);

    }
}
