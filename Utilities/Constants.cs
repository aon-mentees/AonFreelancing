namespace AonFreelancing.Utilities
{
    public class Constants
    {
        public const string USER_TYPE_FREELANCER = "freelancer";
        public const string USER_TYPE_CLIENT = "client";


        public const string ENV_SIT = "SIT";

        public const string PROJECT_PRICETYPE_FIXED = "fixed";
        public const string PROJECT_PRICETYPE_PERHOUR = "per-hour";

        public const string PROJECT_STATUS_AVAILABLE = "available";
        public const string PROJECT_STATUS_CLOSED = "closed";


        public const string BIDS_STATUS_PENDING = "pending";
        public const string BIDS_STATUS_APPROVED = "approved";
        public const string BIDS_STATUS_REJECTED = "rejected";


        public const string TASK_STATUS_TO_DO = "to-do";
        public const string TASK_STATUS_IN_PROGRESS = "in-progress";
        public const string TASK_STATUS_IN_REVIEW = "in-review";
        public const string TASK_STATUS_DONE = "done";

        public const string PROJECT_LIKE_ACTION = "like";
        public const string PROJECT_UNLIKE_ACTION = "unlike";

        public const string PROJECT_QUALIFICATION_BACKEND = "backend";
        public const string PROJECT_QUALIFICATION_FULLSTACK = "fullstack";
        public const string PROJECT_QUALIFICATION_FRONTEND = "frontend";
        public const string PROJECT_QUALIFICATION_MOBILE = "mobile";
        public const string PROJECT_QUALIFICATION_UIUX = "uiux";

        public const string LIKE_NOTIFICATION_MESSAGE_FORMAT = "{0} liked your project {1}";
        public const string BID_APPROVAL_NOTIFICATION_MESSAGE_FORMAT = "{0} approved your bid on project {1}";
        public const string BID_REJECTION_NOTIFICATION_MESSAGE_FORMAT = "{0} rejected your bid on project {1}";

        public const string BID_APPROVAL_NOTIFICATION_TITLE = "You’ve Got a bid approved";
        public const string BID_REJECTION_NOTIFICATION_TITLE = "You’ve Got a bid rejected";
        public const string LIKE_NOTIFICATION_TITLE = "You’ve Got a New Like";

        public const string SUBMIT_BID_NOTIFICATION_MESSAGE_FORMAT = "{0} Applied a bid on your project {1}";
        public const string SUBMIT_BID_NOTIFICATION_TITLE = "You’ve Got a New bid apply";
    }
}
