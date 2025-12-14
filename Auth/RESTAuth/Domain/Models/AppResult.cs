namespace RESTAuth.Domain.Models;

public class AppResult<T>: AppResult
{
    public T? Value { get; }

    private AppResult(bool isSuccess, AppError appError, T? value) : base(isSuccess, appError)
    {
        Value = value;
    }
    
    private AppResult(bool isSuccess, SuccessType error, T? value) : base(isSuccess, error)
    {
        Value = value;
    }
    public static AppResult<T> Success(SuccessType successType, T value) =>
        new(true, successType, value);
    public static AppResult<T> Failure(AppError appError, T? value = default) => 
        new(false, appError, value);
}

public class AppResult
{
    public bool IsSuccess { get; }
    public AppError? AppError { get; }
    public SuccessType? SuccessType { get; set; }

    protected AppResult(bool isSuccess, AppError appError)
    {
        IsSuccess = isSuccess;
        AppError = appError;
    }

    protected AppResult(bool isSuccess, SuccessType? successType)
    {
        IsSuccess = isSuccess;
        SuccessType = successType;
    }
    public static AppResult Success(SuccessType successType) => new(true, successType);
    public static AppResult Failure(AppError appError) => new(false, appError);
}