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
        private List<ToDoItem> _tasksForDoing = new List<ToDoItem>();
        private int _maxTaskCount;
        private int _maxTaskLength;

        public ToDoService(int maxTaskCount, int maxTaskLength)
        {
            _maxTaskCount = maxTaskCount;
            _maxTaskLength = maxTaskLength;
        }
        public ToDoItem Add(ToDoUser user, string name)
        {
            if (_tasksForDoing.Count >= _maxTaskCount)
                throw new TaskCountLimitException(_maxTaskCount);

            if (name.Length > _maxTaskLength)
                throw new TaskLengthLimitException(name.Length, _maxTaskLength);

            foreach (var task in _tasksForDoing)
            {
                if (task.Name == name)
                    throw new DuplicateTaskException(name);
            }

            var newTask = new ToDoItem
            {
                UserId = user.UserId,
                Name = name

            };
            

            _tasksForDoing.Add(newTask);
            return newTask;
        }

        public void Delete(Guid id)
        {
            for (int i = 0; i < _tasksForDoing.Count; i++)
            {
                if (_tasksForDoing[i].Id == id)
                {
                    _tasksForDoing.RemoveAt(i); // Удаляем по индексу
                    return; // Выходим из метода
                }
            }
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            List<ToDoItem> result = new List<ToDoItem>();

            foreach (var task in _tasksForDoing)
            {
                if (task.UserId == userId && task.State == ToDoItemState.Active)
                {
                    result.Add(task);
                }
            }
            return result;
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            var result = new List<ToDoItem>();
            foreach (var task in _tasksForDoing)
            {
                if (task.UserId == userId)
                {
                    result.Add(task);
                }
            }
            return result;
        }

        public void MarkCompleted(Guid id)
        {
            foreach (var task in _tasksForDoing)
            {
                if (task.Id == id) // Если нашли задачу с нужным Id
                {
                    task.State = ToDoItemState.Completed;
                    task.StateChangedAt = DateTime.UtcNow;
                    break; // Прерываем цикл (больше искать не нужно)
                }
            }
        }
    }
}
