using System;
using System.Collections.Generic;
using System.ComponentModel;
using Plugin.CloudFirestore.Attributes;

namespace Changa.Models
{
    public class Notice : INotifyPropertyChanged
    {
        public static string CollectionPath = "notices";

        public event PropertyChangedEventHandler PropertyChanged;

        [Id]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string ItemId { get; set; }
        public string Action { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public long Timestamp { get; set; }

        public virtual object User { get; set; }


        public void CopyTo(Notice notice)
        {
            notice.Id = Id;
            notice.UserId = UserId;
            notice.ItemId = ItemId;
            notice.Action = Action;
            notice.Title = Title;
            notice.Comment = Comment;
            notice.Timestamp = Timestamp;

            notice.User = User;
        }
    }
}
