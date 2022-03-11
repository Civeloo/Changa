using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Changa.Models;

namespace Changa.Services
{
    public interface IUserItemListService
    {
        ReadOnlyObservableCollection<Item> Items { get; }
        void Close();
        void SetUserId(string userId);
        Task LoadAsync();
    }
}