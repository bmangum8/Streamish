using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Streamish.Models;
using Streamish.Utils;


namespace Streamish.Repositories
{
    public class UserProfileRepository : BaseRepository, IUserProfileRepository
    {
        public UserProfileRepository(IConfiguration configuration) : base(configuration) { }
        public List<UserProfile> GetAll()
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                            SELECT * FROM UserProfile
                            ORDER BY DateCreated
                            ";

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        var profiles = new List<UserProfile>();
                        while (reader.Read())
                        {
                            profiles.Add(new UserProfile()
                            {
                                Id = DbUtils.GetInt(reader, "Id"),
                                Name = DbUtils.GetString(reader, "Name"),
                                Email = DbUtils.GetString(reader, "Email"),
                                ImageUrl = DbUtils.GetString(reader, "ImageUrl"),
                                DateCreated = DbUtils.GetDateTime(reader, "DateCreated")
                            });
                        }
                        return profiles;
                    }
                }
            }
        }

        public UserProfile GetById(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                    SELECT Id, Name, Email, ImageUrl, DateCreated
                                    FROM UserProfile
                                    WHERE Id = @Id";

                    DbUtils.AddParameter(cmd, "@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        UserProfile profile = null;
                        if (reader.Read())
                        {
                            profile = new UserProfile()
                            {
                                Id = id,
                                Name = DbUtils.GetString(reader, "Name"),
                                Email = DbUtils.GetString(reader, "Email"),
                                ImageUrl = DbUtils.GetString(reader, "ImageUrl"),
                                DateCreated = DbUtils.GetDateTime(reader, "DateCreated")
                            };
                        }
                        return profile;
                    }
                }
            }
        }

        public UserProfile GetByFirebaseUserId(string firebaseUserId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT 
                                        Id, FirebaseUserId, Name, Email, ImageUrl, DateCreated
                              
                                        FROM UserProfile
                                        WHERE FirebaseUserId = @FirebaseUserId
                                        ";
                    DbUtils.AddParameter(cmd, "@FirebaseUserId", firebaseUserId);

                    UserProfile userProfile = null;

                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        userProfile = new UserProfile()
                        {
                            Id = DbUtils.GetInt(reader, "Id"),
                            FirebaseUserId = DbUtils.GetString(reader, "FirebaseUserId"),
                            Name = DbUtils.GetString(reader, "Name"),
                            Email = DbUtils.GetString(reader, "Email"),
                            ImageUrl = DbUtils.GetString(reader, "ImageUrl"),
                            DateCreated = DbUtils.GetDateTime(reader, "DateCreated")
                        };
                    }
                    reader.Close();

                    return userProfile;
                }
            }
        }

        public void Add(UserProfile userProfile)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                INSERT INTO UserProfile (Name, Email, ImageUrl, DateCreated)
                                OUTPUT INSERTED.ID
                                VALUES (@Name, @Email, @ImageUrl, @DateCreated)";

                    DbUtils.AddParameter(cmd, "@Name", userProfile.Name);
                    DbUtils.AddParameter(cmd, "@Email", userProfile.Email);
                    DbUtils.AddParameter(cmd, "@ImageUrl", userProfile.ImageUrl);
                    DbUtils.AddParameter(cmd, "@DateCreated", userProfile.DateCreated);

                    userProfile.Id = (int)cmd.ExecuteScalar();
                }
            }
        }

        public void Update(UserProfile userProfile)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        UPDATE UserProfile
                        SET Name = @Name,
                            Email = @Email,
                            ImageUrl = @ImageUrl,
                            DateCreated = @DateCreated
                        WHERE Id = @Id";

                    DbUtils.AddParameter(cmd, "@Name", userProfile.Name);
                    DbUtils.AddParameter(cmd, "@Email", userProfile.Email);
                    DbUtils.AddParameter(cmd, "@ImageUrl", userProfile.ImageUrl);
                    DbUtils.AddParameter(cmd, "@DateCreated", userProfile.DateCreated);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM UserProfile WHERE Id = @Id";

                    DbUtils.AddParameter(cmd, "@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public UserProfile GetUserProfileByIdWithVideosAndComments(int id)
        {
            using (var conn = Connection)
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                    SELECT up.Id AS UserProfileId, up.Name AS UserProfileName, up.Email, up.ImageUrl, up.DateCreated AS UserProfileDateCreated,

                                        v.Id AS VideoId, v.Title AS VideoTitle, v.Description AS VideoDescription, v.Url, v.DateCreated AS VideoDateCreated, v.UserProfileId AS VideoUserProfileId,

                                        c.Id AS CommentId, c.Message, c.UserProfileId AS CommentUserProfileId
                                    
                                    FROM UserProfile up
                                    LEFT JOIN Video v ON v.UserProfileId = up.Id
                                    LEFT JOIN Comment c ON c.VideoId = v.Id
                                    WHERE up.Id = @Id
                                        ";
                    DbUtils.AddParameter(cmd, "@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<UserProfile> profiles = new List<UserProfile>();
                        List<Comment> comments = new List<Comment>();
                        Comment comment = null;
                        Video video = null;
                        UserProfile userProfile = null;

                        while (reader.Read())
                        {
                            userProfile = profiles.FirstOrDefault(p => p.Id == id);

                            if (userProfile == null)
                            {
                                userProfile = new UserProfile()
                                {
                                    Id = id,
                                    Name = DbUtils.GetString(reader, "UserProfileName"),
                                    Email = DbUtils.GetString(reader, "Email"),
                                    ImageUrl = DbUtils.GetString(reader, "ImageUrl"),
                                    DateCreated = DbUtils.GetDateTime(reader, "UserProfileDateCreated"),
                                    Videos = new List<Video>()
                                };
                                profiles.Add(userProfile);
                            }



                            if (DbUtils.IsNotDbNull(reader, "VideoId"))
                            {
                                int currentVideoId = DbUtils.GetInt(reader, "VideoId");
                                if (!userProfile.Videos.Any(x => x.Id == currentVideoId))
                                {

                                    video = new Video()
                                    {
                                        Id = DbUtils.GetInt(reader, "VideoId"),
                                        Title = DbUtils.GetString(reader, "VideoTitle"),
                                        Description = DbUtils.GetString(reader, "VideoDescription"),
                                        Url = DbUtils.GetString(reader, "Url"),
                                        DateCreated = DbUtils.GetDateTime(reader, "VideoDateCreated"),
                                        UserProfileId = DbUtils.GetInt(reader, "VideoUserProfileId"),
                                        Comments = comments
                                    };

                                    userProfile.Videos.Add(video);
                                }

                            }

                            if (DbUtils.IsNotDbNull(reader, "CommentId"))
                            {
                                comment = new Comment()
                                {
                                    Id = DbUtils.GetInt(reader, "CommentId"),
                                    Message = DbUtils.GetString(reader, "Message"),
                                    VideoId = DbUtils.GetInt(reader, "VideoId"),
                                    UserProfileId = DbUtils.GetInt(reader, "CommentUserProfileId")
                                };
                                video.Comments.Add(comment);
                            };

                        }
                        return userProfile;
                    }
                }
            }
        }
    }
}
