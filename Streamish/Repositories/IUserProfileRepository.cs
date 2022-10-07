using Streamish.Models;
using System.Collections.Generic;

namespace Streamish.Repositories
{
    public interface IUserProfileRepository
    {
        void Add(UserProfile userProfile);
        List<UserProfile> GetAll();
        UserProfile GetById(int id);
        void Update(UserProfile userProfile);
        void Delete(int id);
        UserProfile GetUserProfileByIdWithVideosAndComments(int id);
        UserProfile GetByFirebaseUserId (string firebaseUserId);

    }
}