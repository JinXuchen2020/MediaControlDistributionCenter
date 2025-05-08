namespace MediaControlDistributionCenter.Services.DTO.Models
{
    public class ResultResponse<T>
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public T? Data { get; set; }

        public Pagination? Pagination { get; set; }

        public static ResultResponse<T> ErrorInstance(string message)
        {
            return new ResultResponse<T>
            {
                Code = -1,
                Message = message
            };
        }
    }
}
