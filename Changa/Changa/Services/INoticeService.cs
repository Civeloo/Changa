using System.Collections.Generic;
using System.Threading.Tasks;
using Changa.Models;

namespace Changa.Services
{
    public interface INoticeService
    {
        Task<Notice> GetNoticeAsync(string UserId,string id);
        Task<List<Notice>> GetAllNoticeAsync(string UserId);
        Task SetNotice(Notice notice, string userId, string id);
        Task DeleteAsync(string UserId, string id);
    }
}