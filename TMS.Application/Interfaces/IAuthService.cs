using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMS.Application.DTOs.Auth;

namespace TMS.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
        Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default);
        Task RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken ct = default);
        Task RevokeTokenAsync(string userId, CancellationToken ct = default);
    }
}
