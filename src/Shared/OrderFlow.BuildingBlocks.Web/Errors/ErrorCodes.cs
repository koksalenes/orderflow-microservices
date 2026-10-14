using Microsoft.AspNetCore.Http;

namespace OrderFlow.BuildingBlocks.Web.Errors;

public static class ErrorCodes
{
    public const string BadRequest = "common.bad_request";
    public const string Unauthorized = "auth.unauthorized";
    public const string Forbidden = "auth.forbidden";
    public const string NotFound = "common.not_found";
    public const string MethodNotAllowed = "common.method_not_allowed";
    public const string RequestTimeout = "common.request_timeout";
    public const string PayloadTooLarge = "common.payload_too_large";
    public const string UnsupportedMediaType = "common.unsupported_media_type";
    public const string UnsupportedApiVersion = "common.unsupported_api_version";
    public const string TooManyRequests = "common.too_many_requests";
    public const string Unexpected = "common.unexpected";
    public const string BadGateway = "gateway.bad_gateway";
    public const string ServiceUnavailable = "gateway.service_unavailable";
    public const string GatewayTimeout = "gateway.timeout";
    public const string CsrfHeaderMissing = "bff.csrf_header_missing";
    public const string InvalidLogoutRequest = "bff.invalid_logout_request";

    public static string FromStatusCode(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status401Unauthorized => Unauthorized,
            StatusCodes.Status403Forbidden => Forbidden,
            StatusCodes.Status404NotFound => NotFound,
            StatusCodes.Status405MethodNotAllowed => MethodNotAllowed,
            StatusCodes.Status408RequestTimeout => RequestTimeout,
            StatusCodes.Status413PayloadTooLarge => PayloadTooLarge,
            StatusCodes.Status415UnsupportedMediaType => UnsupportedMediaType,
            StatusCodes.Status429TooManyRequests => TooManyRequests,
            StatusCodes.Status502BadGateway => BadGateway,
            StatusCodes.Status503ServiceUnavailable => ServiceUnavailable,
            StatusCodes.Status504GatewayTimeout => GatewayTimeout,
            >= 400 and < 500 => BadRequest,
            _ => Unexpected,
        };
}
