using AonFreelancing.Utilities;

namespace AonFreelancing.Models.DTOs
{
    public class ProjectsStatisticsDTO
    {
        public int Total {  get; set; }
        public int Available { get; set; }
        public int Closed {  get; set; }

        ProjectsStatisticsDTO(List<Project>projects)
        {
            Total = projects.Count; ;
            Available = projects.Where(p => p.Status == Constants.PROJECT_STATUS_PENDING).Count();
            Closed = projects.Where(p => p.Status == Constants.PROJECT_STATUS_IN_PROGRESS).Count();
        }

        public static ProjectsStatisticsDTO FromProjects(List<Project> projects) => new ProjectsStatisticsDTO(projects);
    }
}