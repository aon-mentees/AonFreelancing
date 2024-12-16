namespace AonFreelancing.Models.DTOs.NoftificationDTOs
{
    public class TaskApprovalNotificationOutputDTO : NotificationOutputDTO
    {
        public long ProjectId { get; set; }
        public long ApproverId { get; set; }
        public string ApproverName { get; set; }
        public long TaskId { get; set; } // spare

        TaskApprovalNotificationOutputDTO(TaskApprovalNotification taskApprovalNotification) : base(taskApprovalNotification.Id, taskApprovalNotification.Title, taskApprovalNotification.Message, taskApprovalNotification.IsRead, taskApprovalNotification.CreatedAt, taskApprovalNotification.Image = "lilo issue")
        {
            ProjectId = taskApprovalNotification.ProjectId;
            ApproverId = taskApprovalNotification.ApproverId;
            ApproverName = taskApprovalNotification.ApproverName;
            TaskId = taskApprovalNotification.TaskId;
        }
        public static TaskApprovalNotificationOutputDTO FromTaskApprovalNotification(TaskApprovalNotification taskApprovalNotification) => new TaskApprovalNotificationOutputDTO(taskApprovalNotification);

    }
}
