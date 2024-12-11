using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Utilities;
using AonFreelancing.Services;
using System.Linq;
using Microsoft.AspNetCore.Routing.Tree;

namespace AonFreelancing.Services;
public class TaskService(MainAppContext mainAppContext, UserService userService) : MainDbService(mainAppContext)
{
    public async Task<TaskEntity?> FindTaskByIdAsync(long id) => await mainAppContext.Tasks.FindAsync(id);
    public async Task<bool> AuthorizedToUpdateAsync(long userId, TaskEntity storedTask)
    {
        User? storedUser = await userService.GetByIdAsync(userId);
        if(storedUser is null ||
            !(storedUser.GetType() is Freelancer) ||
            storedUser.Id != storedTask.Project.FreelancerId)
            return false;
        return true;   
    }
    public async Task StartTaskAsync(TaskEntity storedTask)
    {
        storedTask.StartedAt = DateTime.Now;
        await UpdateTaskStatusAsync(storedTask, Constants.TASK_STATUS_IN_PROGRESS);
    }
    public async Task SubmitTaskAsync(TaskEntity storedTask) => await UpdateTaskStatusAsync(storedTask, Constants.TASK_STATUS_IN_REVIEW);
    public async Task ApproveTaskAsync(TaskEntity storedTask) => await UpdateTaskStatusAsync(storedTask, Constants.TASK_STATUS_DONE);
    public async Task RejectTaskAsync(TaskEntity storedTask) => await UpdateTaskStatusAsync(storedTask, Constants.TASK_STATUS_IN_PROGRESS);
    public async Task UpdateTaskStatusAsync(TaskEntity storedTask, string newStatus)
    {
        storedTask.Status = newStatus;
        await base.SaveChangesAsync();
    }
}