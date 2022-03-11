using System;
using Changa.Models;
using Reactive.Bindings;
using System.Reactive.Disposables;
using Reactive.Bindings.Extensions;
using Changa.Services;
using System.Reactive.Linq;

namespace Changa.ViewModels
{
    public class NoticeViewModel : IDisposable
    {
        private readonly INoticeService _noticeService;
        private readonly Notice _notice;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ReadOnlyReactivePropertySlim<string> Id { get; }
        public ReadOnlyReactivePropertySlim<string> UserId { get; }
        public ReadOnlyReactivePropertySlim<string> ItemId { get; }
        public ReadOnlyReactivePropertySlim<string> Action { get; }
        public ReadOnlyReactivePropertySlim<string> Title { get; }
        public ReadOnlyReactivePropertySlim<string> Comment { get; }
        public ReadOnlyReactivePropertySlim<long> Timestamp { get; }
        public ReadOnlyReactivePropertySlim<User> User { get; }

        public NoticeViewModel(Notice notice)
        {
            _noticeService = new NoticeService();
            _notice = notice;

            Id = _notice.ObserveProperty(x => x.Id)
                      .ToReadOnlyReactivePropertySlim()
                      .AddTo(_disposables);

            ItemId = _notice.ObserveProperty(x => x.ItemId)
                         .ToReadOnlyReactivePropertySlim()
                         .AddTo(_disposables);

            Action = _notice.ObserveProperty(x => x.Action)
                             .ToReadOnlyReactivePropertySlim()
                             .AddTo(_disposables);

            Title = _notice.ObserveProperty(x => x.Title)
                         .ToReadOnlyReactivePropertySlim()
                         .AddTo(_disposables);

            Comment = _notice.ObserveProperty(x => x.Comment)
                         .ToReadOnlyReactivePropertySlim()
                         .AddTo(_disposables);
            Timestamp = _notice.ObserveProperty(x => x.Timestamp)
                         .ToReadOnlyReactivePropertySlim()
                         .AddTo(_disposables);

            UserId = _notice.ObserveProperty(x => x.UserId)
                         .ToReadOnlyReactivePropertySlim()
                         .AddTo(_disposables);
            //User = _notice.ObserveProperty(x => x.User)
            //             .ToReadOnlyReactivePropertySlim()
            //             .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

    }
}