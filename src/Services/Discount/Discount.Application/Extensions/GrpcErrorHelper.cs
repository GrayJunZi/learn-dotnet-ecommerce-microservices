using System.Net;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;

namespace Discount.Application.Extensions;

public class GrpcErrorHelper
{
    public static RpcException CreateValidationException(Dictionary<string, string> fieldErrors)
    {
        var fieldViolations = new List<BadRequest.Types.FieldViolation>();
        foreach (var error in fieldErrors)
        {
            fieldViolations.Add(new BadRequest.Types.FieldViolation
            {
                Field = error.Key,
                Description = error.Value
            });
        }

        var badRequest = new BadRequest();
        badRequest.FieldViolations.AddRange(fieldViolations);

        var status = new Google.Rpc.Status
        {
            Code = (int)StatusCode.InvalidArgument,
            Message = "Validation Failed",
            Details = { Any.Pack(badRequest) }
        };

        var trailers = new Metadata
        {
            { "grpc-status-details-bin", status.ToByteArray() }
        };

        return new RpcException(new global::Grpc.Core.Status(StatusCode.InvalidArgument, "Validation Errors"),
            trailers);
    }
}