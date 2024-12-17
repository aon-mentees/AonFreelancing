using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.DTOs;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services;

public class ActivitiesService(MainAppContext mainAppContext) : MainDbService(mainAppContext)
{
    public ClientActivitiesResponseDTO ClientActivities(long id)
    {
        var clientProjects = mainAppContext.Projects.Where(p=> p.ClientId == id && !p.IsDeleted);

        int projectPosted = clientProjects.Count();
        var freelancersWorkedWith = clientProjects.Where(p => p.FreelancerId != null).Select(p => p.FreelancerId).Distinct().Count();
        int givenLikes = mainAppContext.ProjectLikes.Where(p => p.LikerId == id).Count();
        int projectsInProgress = clientProjects.Count(p => p.Status == Constants.PROJECT_STATUS_CLOSED);
        int projectsPending = clientProjects.Count(p => p.Status == Constants.PROJECT_STATUS_AVAILABLE);
        int projectsCompleted = clientProjects.Count(p => p.Status == Constants.PROJECT_STATUS_COMPLETED);

        return new ClientActivitiesResponseDTO(freelancersWorkedWith, projectPosted, givenLikes, projectsPending,
                                               projectsInProgress, projectsCompleted);
    }

    public FreelancerActivitiesResponseDTO FreelancerActivities(long id)
    {
        var freelancerProjects = mainAppContext.Projects.Where(p=> p.FreelancerId == id && !p.IsDeleted);
        var freelancerTasks = mainAppContext.Tasks.Include(t=> t.Project).Where(t=> t.Project.FreelancerId == id);

        // Project info
        int clientsWorkedWith = freelancerProjects.Select(f=> f.ClientId).Distinct().Count();        
        int completedProjects = freelancerProjects.Where(p=> p.EndDate != null).Count();
        // UNDONE (DUE TO UNCLARITY IN THE UI DESIGN)
        int projectYouWorkingOn = freelancerProjects.Where(p=> p.EndDate == null).Count();
        int inProgressProjects = freelancerProjects.Where(p=> p.EndDate == null).Count();
        // Likes info
        int givenLikes = mainAppContext.ProjectLikes.Where(p=> p.LikerId == id).Count();    
        
        // Tasks Info
        int inReview = freelancerTasks.Where(t=> t.Status == Constants.TASK_STATUS_IN_REVIEW).Count();
        int toDo = freelancerTasks.Where(t=> t.Status == Constants.TASK_STATUS_TO_DO).Count();
        // Bids info
        int projectYouApplied = mainAppContext.Bids.Where(b=> b.Status == Constants.BIDS_STATUS_PENDING).Count();
        

        return new FreelancerActivitiesResponseDTO(inProgressProjects, completedProjects, 
                                            toDo, inReview, projectYouWorkingOn, 
                                            projectYouApplied, clientsWorkedWith, givenLikes);
    }
}