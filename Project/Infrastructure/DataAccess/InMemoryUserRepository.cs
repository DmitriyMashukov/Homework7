using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyOtusProject.Project.Core.DataAccess;
using MyOtusProject.Project.Core.Entities;

namespace MyOtusProject.Project.Infrastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _users = new List<ToDoUser>();
        public void Add(ToDoUser user)
        {
            _users.Add(user);
        }

        public ToDoUser? GetUser(Guid userId)
        {
            return _users.FirstOrDefault(x => x.UserId == userId);
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            return _users.FirstOrDefault(x => x.TelegramUserId == telegramUserId);
        }
    }
}
