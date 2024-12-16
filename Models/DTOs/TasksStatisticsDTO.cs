using AonFreelancing.Utilities;

namespace AonFreelancing.Models.DTOs
{
    public class TasksStatisticsDTO
    {

        public int Total { get; set; }
        public string ToDo { get; set; }
        public string InReview { get; set; }
        public string InProgress { get; set; }
        public string Done { get; set; }

        TasksStatisticsDTO(List<TaskEntity> tasks)
        {
            Total = tasks.Count;
            if (Total == 0)
                return;
            ToDo = GetPercentString(tasks.Where(t => t.Status == Constants.TASK_STATUS_TO_DO).Count());
            InReview =GetPercentString(tasks.Where(t => t.Status == Constants.TASK_STATUS_IN_REVIEW).Count());
            InProgress = GetPercentString(tasks.Where(t => t.Status == Constants.TASK_STATUS_IN_PROGRESS).Count());
            Done = GetPercentString(tasks.Where(t => t.Status == Constants.TASK_STATUS_DONE).Count());
        }

        public static TasksStatisticsDTO FromTasks(List<TaskEntity>tasks)=>new TasksStatisticsDTO(tasks);

        string GetPercentString(decimal count) => $"{(count / Total) * 100:F2}%";


    }
}