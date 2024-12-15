namespace AonFreelancing.Models.DTOs.NoftificationDTOs
{
    public class BidRejectionNotificationOutputDTO : NotificationOutputDTO
    {
        public long ProjectId { get; set; }
        public long RejectorId { get; set; }
        public string RejectorName { get; set; }
        public long BidId { get; set; } // spare

        BidRejectionNotificationOutputDTO(BidRejectionNotification bidRejectionNotification) : base(bidRejectionNotification.Id, bidRejectionNotification.Title, bidRejectionNotification.Message, bidRejectionNotification.IsRead, bidRejectionNotification.CreatedAt, bidRejectionNotification.Image)
        {
            ProjectId = bidRejectionNotification.ProjectId;
            RejectorId = bidRejectionNotification.RejectorId;
            RejectorName = bidRejectionNotification.RejectorName;
            BidId = bidRejectionNotification.BidId;
        }
        public static BidRejectionNotificationOutputDTO FromBidRejectionNotification(BidRejectionNotification bidRejectionNotification) => new BidRejectionNotificationOutputDTO(bidRejectionNotification);

    }
}
