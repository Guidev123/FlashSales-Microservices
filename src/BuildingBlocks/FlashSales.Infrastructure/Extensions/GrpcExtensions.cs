using FlashSales.Domain.Results;
using Grpc.Core;

namespace FlashSales.Infrastructure.Extensions
{
    public static class GrpcExtensions
    {
        public static async Task<Result<T>> ExecuteAsync<T>(this AsyncUnaryCall<T> grpcCall, string operationName, CancellationToken cancellation = default)
        {
            try
            {
                var result = await grpcCall.ResponseAsync.WaitAsync(cancellation);
                return Result.Success(result);
            }
            catch (RpcException ex)
            {
                return ex.StatusCode switch
                {
                    StatusCode.NotFound => Result.Failure<T>(Error.NotFound(
                        $"{operationName}.NotFound",
                        $"The requested resource for {operationName} was not found.")),

                    StatusCode.PermissionDenied => Result.Failure<T>(Error.Problem(
                        $"{operationName}.PermissionDenied",
                        $"You do not have permission to perform {operationName}.")),

                    StatusCode.Unavailable => Result.Failure<T>(Error.Problem(
                        $"{operationName}.Unavailable",
                        $"The service is currently unavailable for {operationName}.")),

                    StatusCode.AlreadyExists => Result.Failure<T>(Error.Conflict(
                        $"{operationName}.AlreadyExists",
                        $"The resource for {operationName} already exists.")),

                    StatusCode.InvalidArgument => Result.Failure<T>(Error.Invalid(
                        ex.Trailers.GetValue(GrpcMetadataKeys.ErrorCode) ?? $"{operationName}.InvalidArgument",
                        ex.Status.Detail)),

                    StatusCode.FailedPrecondition => Result.Failure<T>(Error.Problem(
                        ex.Trailers.GetValue(GrpcMetadataKeys.ErrorCode) ?? $"{operationName}.FailedPrecondition",
                        ex.Status.Detail)),

                    _ => Result.Failure<T>(Error.Problem(
                        $"{operationName}.UnknownError",
                        $"An unknown error occurred while performing {operationName}: {ex.Message}"))
                };
            }
        }
    }
}