using Reactive.Bindings;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Changa.Models;

namespace Changa.Services
{
    public interface IBidListService
    {
        ReadOnlyObservableCollection<Bid> Bids { get; }
        Task LoadAsync(string itemId);
        void Close();
    }
}
