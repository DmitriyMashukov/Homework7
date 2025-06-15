using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyOtusProject.Project.Core.DataAccess;
using MyOtusProject.Project.Core.Entities;

namespace MyOtusProject.Project.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _items = new List<ToDoItem>();
        public void Add(ToDoItem item)
        {
            _items.Add(item);
        }

        public int CountActive(Guid userId)
        {
            return _items.Count(x => x.User.UserId == userId && x.State == ToDoItemState.Active);
        }

        public void Delete(Guid id)
        {
            var item = _items.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                _items.Remove(item);
            }
        }

        public bool ExistsByName(Guid userId, string name)
        {
            return _items.Any(x => x.User.UserId == userId &&
                              x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public ToDoItem? Get(Guid id)
        {
            return _items.FirstOrDefault(x => x.Id == id);
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _items.Where(x => x.User.UserId == userId &&
                               x.State == ToDoItemState.Active).ToList().AsReadOnly();
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _items.Where(x => x.User.UserId == userId).ToList().AsReadOnly();
        }

        public void Update(ToDoItem item)
        {
            var existingItem = _items.FirstOrDefault(x => x.Id == item.Id);
            if (existingItem != null)
            {
                _items.Remove(existingItem);
                _items.Add(item);
            }
        }

        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            return _items.Where(x => x.User.UserId == userId).Where(predicate).ToList().AsReadOnly();
        }
    }
}
