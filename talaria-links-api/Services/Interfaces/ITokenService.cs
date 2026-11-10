using talaria_links_api.DTOs.Tokens;
using talaria_links_api.Entities;

namespace talaria_links_api.Services.Interfaces
{
    public interface ITokenService
    {
        TokenResult CreateToken(Account account); 
    }
}
