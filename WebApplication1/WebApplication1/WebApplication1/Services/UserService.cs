using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.System)?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : null;
        }

        public string? GetUserRole()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;
        }

        public byte[]? GetUserAvatar()
        {
            int? userId = GetUserId();
            if (userId == null) return null;

            return _context.Users.Where(user => user.Id == userId)
                                 .Select(user => user.Avatar)
                                 .FirstOrDefault();
        }

        public async Task<bool> GetAccess()
        {
            int? userId = GetUserId();
            if (userId == null) return false;

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (user.BanTime == null) return true;

            if (user.BanTime < DateTime.Now)
            {
                user.BanTime = null;
                _context.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
