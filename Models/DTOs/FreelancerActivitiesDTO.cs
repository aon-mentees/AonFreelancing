namespace AonFreelancing.Models.DTOs;
public class FreelancerActivitiesResponseDTO
{
    public int ClientsWorkedWith { get; set; }
    public int GivenLikes { get; set; }
    public int ProjectYouApplied { get; set; }
    public int ProjectYouWorkingOn { get; set; }
    public int InReview { get; set; }
    public int ToDo { get; set; }
    public int CompletedProjects { get; set; }
    public int InProgressProjects { get; set; }

    public FreelancerActivitiesResponseDTO(int inProgressProjects, int completedProjects, 
                                            int toDo, int inReview, int projectYouWorkingOn, 
                                            int projectYouApplied, int clientsWorkedWith, int givenLikes)
    {
        ClientsWorkedWith = clientsWorkedWith;
        ProjectYouApplied = projectYouApplied;
        ProjectYouWorkingOn = projectYouWorkingOn;
        InReview =inReview;
        ToDo = toDo;
        CompletedProjects = completedProjects;
        InProgressProjects = inProgressProjects;
        GivenLikes = givenLikes;
    }
}

