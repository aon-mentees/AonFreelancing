namespace AonFreelancing.Models
{
    public class TaskApprovalNotification : Notification
    {
        public long ProjectId { get; set; }
        public long ApproverId { get; set; }
        public string ApproverName { get; set; }
        public long TaskId { get; set; } // spare

        public TaskApprovalNotification() { }
        public TaskApprovalNotification(string title, string message, long receiverId, long projectId, long approverId, string approverName, long taskId)
            : base(title, message, receiverId)
        {
            ProjectId = projectId;
            ApproverId = approverId;
            ApproverName = approverName;
            TaskId = taskId;
        }
    }
}
