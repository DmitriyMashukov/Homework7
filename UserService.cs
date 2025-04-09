using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyOtusProject
{
    internal class UserService : IUserService
    {
        private List<ToDoUser> _users = new List<ToDoUser>(); 
        public ToDoUser? GetUser(long telegramUserId)
        {
            foreach (var user in _users)
            {
                if (user.TelegramUserId == telegramUserId)
                    return user;
            }
            return null;
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var newUser = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.UtcNow
            };
            _users.Add(newUser);
            return newUser;
        }
    }
}
