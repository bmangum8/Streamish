 SELECT up.Id AS UserProfileId, up.Name AS UserProfileName, up.Email, up.ImageUrl, up.DateCreated AS UserProfileDateCreated,

                                        v.Id AS VideoId, v.Title AS VideoTitle, v.Description AS VideoDescription, v.Url, v.DateCreated AS VideoDateCreated, v.UserProfileId AS VideoUserProfileId
                                    
                                    FROM UserProfile up
                                    LEFT JOIN Video v ON v.UserProfileId = up.Id
                                    WHERE up.Id = 2


  SELECT up.Id AS UserProfileId, up.Name AS UserProfileName, up.Email, up.ImageUrl, up.DateCreated AS UserProfileDateCreated,

                                        v.Id AS VideoId, v.Title AS VideoTitle, v.Description AS VideoDescription, v.Url, v.DateCreated AS VideoDateCreated, v.UserProfileId AS VideoUserProfileId,

                                        c.Id AS CommentId, c.Message, c.UserProfileId AS CommentUserProfileId
                                    
                                    FROM UserProfile up
                                    LEFT JOIN Video v ON v.UserProfileId = up.Id
                                    LEFT JOIN Comment c ON c.VideoId = v.Id
                                    WHERE up.Id = 1