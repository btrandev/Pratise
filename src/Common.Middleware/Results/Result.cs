namespace Common.Middleware.Results;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public IEnumerable<Error> Errors { get; private set; } = Array.Empty<Error>();
    
    public bool HasErrors => Errors.Any();
    
    private Result(bool isSuccess, T? data, IEnumerable<Error> errors)
    {
        IsSuccess = isSuccess;
        Data = data;
        Errors = errors;
    }

    public static Result<T> Success(T data) => new(true, data, Array.Empty<Error>());

    public static Result<T> Failure(Error error) => new(false, default, new[] { error });
    
    public static Result<T> Failure(string errorMessage) => new(false, default, new[] { new Error(errorMessage) });
    
    public static Result<T> Failure(string code, string errorMessage) => new(false, default, new[] { new Error(code, errorMessage) });
    
    public static Result<T> Failure(IEnumerable<Error> errors) => new(false, default, errors);
    
    public static Result<T> Failure(IEnumerable<string> errorMessages) => 
        new(false, default, errorMessages.Select(msg => new Error(msg)));
    
    public static implicit operator Result(Result<T> result) => 
        result.IsSuccess ? Result.Success() : Result.Failure(result.Errors);
        
    public Result<U> Map<U>(Func<T, U> mapper) where U : class =>
        IsSuccess && Data != null 
            ? Result<U>.Success(mapper(Data)) 
            : Result<U>.Failure(Errors);
            
    public async Task<Result<U>> MapAsync<U>(Func<T, Task<U>> mapper) where U : class =>
        IsSuccess && Data != null 
            ? Result<U>.Success(await mapper(Data)) 
            : Result<U>.Failure(Errors);
}

public class Result
{
    public bool IsSuccess { get; private set; }
    public IEnumerable<Error> Errors { get; private set; } = Array.Empty<Error>();
    
    public bool HasErrors => Errors.Any();

    private Result(bool isSuccess, IEnumerable<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success() => new(true, Array.Empty<Error>());

    public static Result Failure(Error error) => new(false, new[] { error });
    
    public static Result Failure(string errorMessage) => new(false, new[] { new Error(errorMessage) });
    
    public static Result Failure(string code, string errorMessage) => new(false, new[] { new Error(code, errorMessage) });
    
    public static Result Failure(IEnumerable<Error> errors) => new(false, errors);
    
    public static Result Failure(IEnumerable<string> errorMessages) => 
        new(false, errorMessages.Select(msg => new Error(msg)));
    
    public static Result Combine(params Result[] results)
    {
        var failedResults = results.Where(r => !r.IsSuccess).ToArray();
        
        return failedResults.Length == 0
            ? Success()
            : Failure(failedResults.SelectMany(r => r.Errors).ToArray());
    }
    
    public Result<T> WithData<T>(T data) where T : class => 
        IsSuccess ? Result<T>.Success(data) : Result<T>.Failure(Errors);
}

