using Plugin.CloudFirestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Changa.Models;

namespace Changa.Services
{
    public class NoticeService : INoticeService
    {
        private readonly IFirestore _firestore;
        public NoticeService()
        {
            _firestore = CrossCloudFirestore.Current.Instance;
        }

        public ICollectionReference GetNoticeCollectionAsync(string UserId)
        {
            return _firestore.GetCollection(User.CollectionPath)
                                .GetDocument(UserId)
                                .GetCollection(Models.Notice.CollectionPath);
        }

        public async Task<Notice> GetNoticeAsync(string userId, string id)
        {  
            var document = await GetNoticeCollectionAsync(userId)
                                .GetDocument(id)
                                .GetDocumentAsync()
                                .ConfigureAwait(false);
            return document.ToObject<Notice>();            
        }

        public async Task<List<Notice>> GetAllNoticeAsync(string UserId)
        {
            var documents = await GetNoticeCollectionAsync(UserId)
                                //.OrderBy(nameof(Bid.Timestamp), true)
                                //.LimitTo(Count)
                                //.StartAfter(_lastTimestamp)
                                .GetDocumentsAsync()
                                .ConfigureAwait(false);
            return (List<Notice>) documents.ToObjects<Notice>();                                       
        }

        public async Task SetNotice(Notice notice, string UserId, string id)
        {
            notice.Timestamp = DateTime.Now.Ticks;
            await GetNoticeCollectionAsync(UserId)
                        .GetDocument(id)
                        .SetDataAsync(notice)
                        .ConfigureAwait(false);
        }

        public async Task DeleteAsync(string UserId, string id)
        {
            await GetNoticeCollectionAsync(UserId)
                  .GetDocument(id)
                  .DeleteDocumentAsync()
                  .ConfigureAwait(false);
        }

    }
}
