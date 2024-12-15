namespace AonFreelancing.Models.DTOs;
public class ClientActivitiesResponseDTO
{
    public int FreelancersWorkedWith { get; set; }
    public int ProjectPosted { get; set; }
    public int GivenLikes { get; set; }

    public ClientActivitiesResponseDTO(int freelancersWorkedWith, int projectPosted, int givenLikes)
    {
        FreelancersWorkedWith = freelancersWorkedWith;
        ProjectPosted = projectPosted;
        GivenLikes = givenLikes;
    }
}

