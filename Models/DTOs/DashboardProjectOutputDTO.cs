namespace AonFreelancing.Models.DTOs
{
    public class DashboardProjectOutputDTO
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string ClientName { get; set; }
        public string Status { get; set; }
        public string Percentage { get; set; }

        public DashboardProjectOutputDTO(Project project, string percentage)
        {
            Id = project.Id;
            Title = project.Title;
            if (project.Client != null)
                ClientName = project.Client.Name;
            Status = project.Status;
            Percentage = percentage;
        }
    }
}
