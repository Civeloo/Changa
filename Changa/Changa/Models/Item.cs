using System;
using System.Collections.Generic;
using System.ComponentModel;
using Plugin.CloudFirestore.Attributes;

namespace Changa.Models
{
    public class Item : INotifyPropertyChanged
    {
        public static string CollectionPath = "items";        

        public event PropertyChangedEventHandler PropertyChanged;

        [Id]
        public string Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string OwnerId { get; set; }
        public int BidCount { get; set; }
        public string Comment { get; set; }
        public long Timestamp { get; set; }
        public double Price { get; set; }        
        public string Location { get; set; }
        public string PostalCode { get; set; }
        public int Hours { get; set; }
        public string Status { get; set; }
        public string Assigned { get; set; }
        
        internal object Bids;

        public void CopyTo(Item item)
        {
            item.Id = Id;
            item.Title = Title;
            item.Image = Image;
            item.OwnerId = OwnerId;
            item.BidCount = BidCount;
            item.Comment = Comment;
            item.Timestamp = Timestamp;
            item.Location = Location;
            item.PostalCode = PostalCode;
            item.Hours = Hours;
            item.Price = Price;
            item.Status = Status;
            item.Bids = Bids;
            item.Assigned = Assigned;
        }
    }
}
