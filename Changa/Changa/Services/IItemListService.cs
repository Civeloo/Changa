using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Changa.Models;

namespace Changa.Services
{
    public interface IItemListService
    {
        ReadOnlyObservableCollection<Item> Items { get; }
        Task LoadAsync();
        void Close();
    }
}
