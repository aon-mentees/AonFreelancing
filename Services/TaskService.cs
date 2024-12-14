using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Utilities;
using AonFreelancing.Services;
using System.Linq;
using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services;
public class TaskService(MainAppContext mainAppContext, UserService userService) : MainDbService(mainAppContext)
{
    public async Task<TaskEntity?> FindTaskByIdAsync(long id, bool includeProject = false)
    {
        if (includeProject) 
            return await mainAppContext.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == id);
        return await mainAppContext.Tasks.FindAsync(id);
    }
    public async Task<bool> AuthorizedToUpdateAsync(long userId, TaskEntity storedTask)
    {
        User? storedUser = await userService.FindByIdAsync(userId);
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
    public async Task ApproveTaskAsync(TaskEntity storedTask) 
    {
        await UpdateTaskStatusAsync(storedTask, Constants.TASK_STATUS_DONE);
    }
    public async Task RejectTaskAsync(TaskEntity storedTask) => await UpdateTaskStatusAsync(storedTask, Constants.TASK_STATUS_IN_PROGRESS);
    public async Task UpdateTaskStatusAsync(TaskEntity storedTask, string newStatus)
    {
        storedTask.Status = newStatus;
        if(newStatus == Constants.TASK_STATUS_DONE)
            storedTask.CompletedAt = DateTime.Now;
        await base.SaveChangesAsync();
    }
}