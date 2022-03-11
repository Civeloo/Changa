using System;
using System.Collections.Generic;
using System.ComponentModel;
using Plugin.CloudFirestore.Attributes;

namespace Changa.Models
{
    public class Bid : INotifyPropertyChanged
    {
        public static string CollectionPath = "bids";

        public event PropertyChangedEventHandler PropertyChanged;

        [Id]
        public string Id { get; set; }
        public virtual string ItemId { get; set; }

        public long Timestamp { get; set; }
        public virtual string Status { get; set; }
        public virtual string BidderId { get; set; }
        public virtual string BidderName { get; set; }
        public virtual int BidderReputation { get; set; }
        public virtual string BidderImage { get; set; }
        public virtual object Bidder { get; set; }

        public void CopyTo(Bid bid)
        {
            bid.Id = Id;
            bid.ItemId = ItemId;
            bid.Timestamp = Timestamp;
            bid.Status = Status;
            bid.BidderId = BidderId;
            bid.BidderName = BidderName;
            bid.BidderReputation = BidderReputation;
            bid.Bidder = Bidder;
        }
    }
}
