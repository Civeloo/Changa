using Plugin.CloudFirestore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Changa.Models;

namespace Changa.Services
{
    public class UserService : IUserService
    {
        private readonly IFirestore _firestore;

        public UserService()
        {
            _firestore = CrossCloudFirestore.Current.Instance;            
        }

        public async Task<User> GetUserAsync(string id)
        {
            
            var u = new User();
            try
            {
                var document = await _firestore.GetCollection(User.CollectionPath)
                                                    .GetDocument(id)
                                                    .GetDocumentAsync()
                                                    .ConfigureAwait(false)
                                                    ;
                ;
                u = document.ToObject<User>();

                
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }            

            return u;
        }

        public User GetUser(string id)
        {
            var u = GetUserAsync(id).Result;
            return u;
        }

            public async Task<bool> CalificateUserAync(string userId, bool good)
        {
            return await _firestore.RunTransactionAsync(transaction =>
                {
                    var document = _firestore.GetCollection(User.CollectionPath).GetDocument(userId);
                    var user = transaction.GetDocument(document).ToObject<User>();
                    if (user == null) return false;
                    if (good) user.Reputation++;
                    else user.Reputation--;
                    user.Works++;
                    transaction.UpdateData(document, user);
                    return true;
                }).ConfigureAwait(false);         
        }

    }
}
