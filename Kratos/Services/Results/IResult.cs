namespace Kratos.Services.Results
{
    public interface IResult
    {
        ResultType Type { get; set; }

        string Message { get; set; }
    }
}
