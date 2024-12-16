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
        public const string PROJECT_STATUS_COMPLETED = "completed";

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

        public const string EMPLOYMENTTYPE_FULLTIME = "full-time";
        public const string EMPLOYMENTTYPE_PARTTIME = "part-time";
        public const string EMPLOYMENTTYPE_CONTRACT = "contract";
        public const string EMPLOYMENTTYPE_INTERNSHIP = "internship";
        
        public const string LIKE_NOTIFICATION_MESSAGE_FORMAT = "{0} liked your project {1}";
        public const string BID_APPROVAL_NOTIFICATION_MESSAGE_FORMAT = "{0} approved your bid on project {1}";
        public const string BID_REJECTION_NOTIFICATION_MESSAGE_FORMAT = "{0} rejected your bid on project {1}";
        public const string SUBMIT_BID_NOTIFICATION_MESSAGE_FORMAT = "{0} Applied a bid on your project {1}";
        public const string TASK_APPROVAL_NOTIFICATION_MESSAGE_FORMAT = "{0} approved your task on project {1}";
        public const string TASK_REJECTION_NOTIFICATION_MESSAGE_FORMAT = "{0} rejected your task on project {1}";
        public const string COMMENT_NOTIFICATION_MESSAGE_FORMAT = "{0} commented on your project {1}";

        public const string LIKE_NOTIFICATION_TITLE = "You’ve Got a New Like";
        public const string BID_APPROVAL_NOTIFICATION_TITLE = "You’ve Got a bid approved";
        public const string BID_REJECTION_NOTIFICATION_TITLE = "You’ve Got a bid rejected";
        public const string SUBMIT_BID_NOTIFICATION_TITLE = "You’ve Got a New bid apply";
        public const string TASK_APPROVAL_NOTIFICATION_TITLE = "You’ve Got a task approved";
        public const string TASK_REJECTION_NOTIFICATION_TITLE = "You’ve Got a task rejected";
        public const string LIKE_NOTIFICATION_TITLE = "You’ve Got a New Like";
        public const string SUBMIT_BID_NOTIFICATION_TITLE = "You’ve Got a New bid apply";
        public const string COMMENT_NOTIFICATION_TITLE = "You’ve Got a New Comment";

        public const int LIKES_DEFAULT_PAGE_SIZE = 10;
        public const int PROJECTS_DEFAULT_PAGE_SIZE = 10;
        public const int RECENT_PROJECTS_DEFAULT_PAGE_SIZE = 6;
        public const int SKILLS_DEFAULT_PAGE_SIZE = 4;
        public const int BIDS_DEFAULT_PAGE_SIZE = 10;

        public const int CLIENT_ACTIVITY_DEFAULT_PAGE_SIZE = 2;

        public const int WORK_EXPERIENCES_DEFAULT_PAGE_SIZE = 4;
        public const int CERTIFICATION_DEFAULT_PAGE_SIZE = 4;
        public const int EDUCATION_DEFAULT_PAGE_SIZE = 4;
        public const int COMMENTS_DEFAULT_PAGE_SIZE = 10;

        public const int MAX_FILE_SIZE = 1024 * 1024 * 5;
        public const string JPG = ".jpg";
        public const string JPEG = ".jpeg";
        public const string PNG = ".png";
        public const string GIF = ".gif";


        public const string DEFAULT_USER_PROFILE_PICTURE = "default-user-profile-picture.jpg";

        public const int RATING_DEFAULT_PAGE_SIZE = 5;
        public const int FREELANCERS_WORKED_WITH_DEFAULT_PAGE_SIZE = 5;
    }
}
