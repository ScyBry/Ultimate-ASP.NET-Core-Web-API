using Microsoft.AspNetCore.Identity;
using Shared.DataTransferObjects;

namespace Contracts;

public interface IAuthenticationService
{
    Task<IdentityResult> RegisterUser(UserForRegistrationDto userForRegistration);
}