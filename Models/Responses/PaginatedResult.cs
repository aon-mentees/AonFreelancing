namespace AonFreelancing.Models.Responses
{
    public class PaginatedResult<T>
    {
        public long Total { get; set; }
        public List<T> Result { get; set; }

        public PaginatedResult()
        {
            Result = new List<T>();
        }
        public PaginatedResult(long total, List<T> result)
        {
            Total = total;
            Result = result;
        }
    }
}
