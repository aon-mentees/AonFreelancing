namespace AonFreelancing.Models
{
    public class ProfileVisitNotification : Notification
    {
        public long VisitorId { get; set; }
        public string VisitorName { get; set; }

        public ProfileVisitNotification() { }

        public ProfileVisitNotification(string title, string message, long receiverId, long visitorId, string visitorName)
            : base(title, message, receiverId)
        {
            VisitorId = visitorId;
            VisitorName = visitorName;
        }
    }
}
