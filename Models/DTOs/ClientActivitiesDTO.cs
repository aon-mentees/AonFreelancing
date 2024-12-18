namespace AonFreelancing.Models.DTOs;
public class ClientActivitiesResponseDTO
{
    public int FreelancersWorkedWith { get; set; }
    public int ProjectPosted { get; set; }
    public int GivenLikes { get; set; }
    public int ProjectsPending { get; set; }
    public int ProjectsInProgress {  get; set; }
    public int ProjectsCompleted {  get; set; }

    public ClientActivitiesResponseDTO(int freelancersWorkedWith, int projectPosted, int givenLikes, int projectsPending,
                                       int projectsInProgress, int projectsCompleted)
    {
        FreelancersWorkedWith = freelancersWorkedWith;
        ProjectPosted = projectPosted;
        GivenLikes = givenLikes;
        ProjectsPending = projectsPending;
        ProjectsInProgress = projectsInProgress;
        ProjectsCompleted = projectsCompleted;
    }
}

