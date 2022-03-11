using Reactive.Bindings;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Changa.Models;

namespace Changa.Services
{
    public interface INoticeListService
    {
        ReadOnlyObservableCollection<Notice> Notices { get; }
        Task LoadAsync(string id);
        void Close();
    }
}