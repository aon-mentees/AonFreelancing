namespace AonFreelancing.Models
{
    public class TaskRejectionNotification : Notification
    {
        public long ProjectId { get; set; }
        public long RejectorId { get; set; }
        public string RejectorName { get; set; }
        public long TaskId { get; set; } // spare

        public TaskRejectionNotification() { }

        public TaskRejectionNotification(string title, string message, long receiverId, long projectId, long rejectorId, string rejectorName, long taskId, string? image = null)
            : base(title, message, receiverId, image)
        {
            ProjectId = projectId;
            RejectorId = rejectorId;
            RejectorName = rejectorName;
            TaskId = taskId;
        }
    }
}
