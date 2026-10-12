using FlashSales.Domain.Results;
using Grpc.Core;

namespace FlashSales.Infrastructure.Extensions
{
    public static class GrpcMetadataKeys
    {
        public const string ErrorCode = "error-code";
    }

    public static class GrpcServerExtensions
    {
        public static RpcException ToRpcException(this Error error)
        {
            var statusCode = error.Type switch
            {
                ErrorType.NotFound => StatusCode.NotFound,
                ErrorType.Conflict => StatusCode.AlreadyExists,
                ErrorType.Validation => StatusCode.InvalidArgument,
                _ => StatusCode.FailedPrecondition
            };

            return new RpcException(
                new Status(statusCode, error.Description),
                [new Metadata.Entry(GrpcMetadataKeys.ErrorCode, error.Code)]);
        }

        public static void ThrowIfFailure(this Result result)
        {
            if (result.IsFailure)
                throw result.Error!.ToRpcException();
        }

        public static T ThrowIfFailure<T>(this Result<T> result)
        {
            result.ThrowIfFailure();

            return result.Value;
        }

        public static Guid ToGuid(this string value, string field)
            => Guid.TryParse(value, out var id)
                ? id
                : throw new RpcException(new Status(StatusCode.InvalidArgument, $"{field} must be a valid GUID."));
    }
}
