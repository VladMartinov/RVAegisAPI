﻿using RVAegis.Models.UserModels;

namespace RVAegis.DTOs.UserDTOs
{
    public class UserUDto
    {
        public ushort UserRoleId { get; set; }
        public ushort UserStatusId { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Photo { get; set; }

        public UserUDto(User user)
        {
            UserRoleId = user.UserRoleId;
            UserStatusId = user.UserStatusId;

            FullName = user.FullName;
            Email = user.Email;
            Photo = user.Photo is not null ? Convert.ToBase64String(user.Photo) : null;
        }

        public UserUDto()
        {
            // Empty constructor for deserialization
            FullName = string.Empty;
            Email = string.Empty;
        }
    }
}
