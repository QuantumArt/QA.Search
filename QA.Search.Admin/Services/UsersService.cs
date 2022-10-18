using System;
using QA.Search.Admin.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using QA.Search.Admin.Errors;
using Microsoft.Extensions.Options;
using QA.Search.Common.Extensions;
using QA.Search.Data;
using QA.Search.Data.Models;

namespace QA.Search.Admin.Services
{
    public class UsersService
    {
        const int RESET_PASSWORD_REQUEST_VALID_DAYS = 5;

        readonly SearchDbContext _dbContext;
        readonly SmtpService _smtpService;
        readonly Settings _settings;

        public UsersService(
            SearchDbContext dbContext,
            SmtpService smtpService,
            IOptions<Settings> settings)
        {
            _dbContext = dbContext;
            _smtpService = smtpService;
            _settings = settings.Value;
        }

        public async Task CreateUser(string email, UserRole role)
        {
            if (await GetUser(email) != null)
            {
                throw new BusinessError("Пользователь с указанным адресом электронной почты уже существует");
            }

            var user = new User(email, Guid.NewGuid().ToString(), role);
            _dbContext.Users.Add(user);

            var resetPasswordRequest = new Data.Models.ResetPasswordRequest(user);
            _dbContext.ResetPasswordRequests.Add(resetPasswordRequest);

            await _smtpService.Send(
                user.Email,
                _settings.InviteUserMessageSubject,
                _settings.InviteUserMessageBodyTemplate
                    .Replace("{host}", _settings.AdminAppUrl.RemoveTrailingSlash())
                    .Replace("{id}", resetPasswordRequest.Id.ToString()));

            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserResponse> GetUser(string email)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.Equals(email.ToLower()));

            if (user == null)
            {
                return null;
            }

            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task DeleteUser(int id)
        {
            var user = await _dbContext.Users
              .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new BusinessError("Пользователь не найден");
            }

            _dbContext.Users.Remove(user);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<UsersListResponse> ListUsers(UsersListRequest request)
        {
            var query = _dbContext.Users.AsQueryable();

            if (request.Email != null)
            {
                query = query.Where(u => EF.Functions.Like(u.Email, $"{request.Email.ToLower()}%"));
            }

            if (request.Role != null)
            {
                query = query.Where(u => u.Role == request.Role);
            }

            return new UsersListResponse
            {
                TotalCount = await query.CountAsync(),
                Data = (await query.Skip(request.Offset)
                    .Take(request.Limit).ToListAsync())
                    .Select(u => new UserResponse
                    {
                        Id = u.Id,
                        Email = u.Email,
                        Role = u.Role
                    })
                    .ToList()
            };
        }

        public async Task SendResetPasswordLink(string email)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.Equals(email.ToLower()));

            if (user == null)
            {
                throw new BusinessError("Пользователь с указанным адресом электронной почты не найден");
            }

            var resetPasswordRequest = new Data.Models.ResetPasswordRequest(user);
            _dbContext.ResetPasswordRequests.Add(resetPasswordRequest);

            await _smtpService.Send(
                user.Email,
                _settings.ResetPasswordMessageSubject,
                _settings.ResetPasswordMessageBodyTemplate
                    .Replace("{host}", _settings.AdminAppUrl.RemoveTrailingSlash())
                    .Replace("{id}", resetPasswordRequest.Id.ToString()));

            await _dbContext.SaveChangesAsync();
        }

        public async Task<UserResponse> GetUserByResetPasswordLinkId(Guid id)
        {
            var request = await GetValidResetPasswordRequest(id);

            return new UserResponse
            {
                Id = request.User.Id,
                Email = request.User.Email,
                Role = request.User.Role
            };
        }

        public async Task ChangePassword(Guid requestId, string password)
        {
            var request = await GetValidResetPasswordRequest(requestId);

            request.User.SetPassword(password);
            request.Invalidate();

            await _dbContext.SaveChangesAsync();
        }

        async Task<QA.Search.Data.Models.ResetPasswordRequest> GetValidResetPasswordRequest(Guid requestId)
        {
            var request = await _dbContext.ResetPasswordRequests
                .Include(e => e.User)
                .SingleOrDefaultAsync(e => e.Id == requestId);

            if (request == null)
            {
                throw new BusinessError("Неверная ссылка для восстановления пароля");
            }

            if (!request.IsActive || request.Timestamp.AddDays(RESET_PASSWORD_REQUEST_VALID_DAYS) < DateTime.UtcNow)
            {
                throw new BusinessError("Cсылка для восстановления пароля не действительна");
            }

            var lastRequest = await _dbContext.ResetPasswordRequests
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync(r => r.User == request.User);

            if (lastRequest != null && lastRequest.Id != request.Id)
            {
                throw new BusinessError("Cсылка для восстановления пароля не действительна");
            }

            return request;
        }
    }
}
