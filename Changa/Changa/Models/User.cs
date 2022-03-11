using System;
using System.ComponentModel;
using Plugin.CloudFirestore.Attributes;
namespace Changa.Models
{
    public class User : INotifyPropertyChanged
    {
        public static string CollectionPath = "users";

        public event PropertyChangedEventHandler PropertyChanged;

        [Id]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public int ContributionCount { get; set; }
        public int Reputation { get; set; }
        public int Works { get; set; }
        public string Location { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        internal object Notices;

        public void CopyTo(User user)
        {
            user.Id = Id;
            user.Name = Name;
            user.Image = Image;
            user.Reputation = Reputation;
            user.Location = Location;
            user.PostalCode = PostalCode;
            user.Notices = Notices;
            user.PhoneNumber = PhoneNumber;
            user.Email = Email;
        }
    }
}
