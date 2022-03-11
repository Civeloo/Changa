using System.Threading.Tasks;
using Changa.Models;

namespace Changa.Services
{
    public interface IUserService
    {
        Task<User> GetUserAsync(string id);
        User GetUser(string id);
        Task<bool> CalificateUserAync(string userId, bool good);
    }
}