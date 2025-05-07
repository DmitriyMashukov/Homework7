using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyOtusProject
{
    internal class ToDoService : IToDoService
    {
        private readonly IToDoRepository _repository;
        private readonly int _maxTaskCount;
        private readonly int _maxTaskLength;

        public ToDoService(IToDoRepository repository, int maxTaskCount, int maxTaskLength)
        {
            _repository = repository;
            _maxTaskCount = maxTaskCount;
            _maxTaskLength = maxTaskLength;
        }
        public ToDoItem Add(ToDoUser user, string name)
        {
            if (_repository.CountActive(user.UserId) >= _maxTaskCount)
                throw new TaskCountLimitException(_maxTaskCount);

            if (name.Length > _maxTaskLength)
                throw new TaskLengthLimitException(name.Length, _maxTaskLength);

            if (_repository.ExistsByName(user.UserId, name))
                throw new DuplicateTaskException(name);

            var newTask = new ToDoItem
            {
                User = user,
                Name = name,
                State = ToDoItemState.Active,
                CreatedAt = DateTime.UtcNow
            };


            _repository.Add(newTask);
            return newTask;
        }

        public void Delete(Guid id)
        {
            _repository.Delete(id);
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _repository.GetActiveByUserId(userId);
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _repository.GetAllByUserId(userId);
        }

        public void MarkCompleted(Guid id)
        {
            var task = _repository.Get(id);
            if (task != null)
            {
                task.State = ToDoItemState.Completed;
                task.StateChangedAt = DateTime.UtcNow;
                _repository.Update(task);
            }
        }
    }
}
