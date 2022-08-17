namespace AnkiConverter.Models
{
    public class ResultModel<T>
    {
        public T Data { get; set; }
        public bool Result { get; set; } = false;
        public string Message { get; set; } = "";
        public int Code { get; set; } = 500;
    }
}
