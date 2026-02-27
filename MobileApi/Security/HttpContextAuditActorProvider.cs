using System.Security.Claims;
using Data.Auditing;

namespace MobileApi.Security
{
    public class HttpContextAuditActorProvider : IAuditActorProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextAuditActorProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var raw = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}
