using AonFreelancing.Utilities;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectsStatisticsDTO
    {
        public int Total {  get; set; }
        public int Pending { get; set; }
        public int InProgress {  get; set; }
        public int Completed { get; set; }

        ProjectsStatisticsDTO(List<Project>projects)
        {
            Total = projects.Count; ;
            Pending = projects.Where(p => p.Status == Constants.PROJECT_STATUS_PENDING).Count();
            InProgress = projects.Where(p => p.Status == Constants.PROJECT_STATUS_IN_PROGRESS).Count();
            Completed = projects.Where(p => p.Status == Constants.PROJECT_STATUS_COMPLETED).Count();
        }

        public static ProjectsStatisticsDTO FromProjects(List<Project> projects) => new ProjectsStatisticsDTO(projects);
    }
}