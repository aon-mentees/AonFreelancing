namespace AonFreelancing.Commons
{
    public class ValidationResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Creates a success validation result
        /// </summary>
        public ValidationResult()
        {
            IsSuccess = true;
        }
        /// <summary>
        /// Creates a fail validation result
        /// </summary>
        /// <param name="errorMessage"></param>
        public ValidationResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
