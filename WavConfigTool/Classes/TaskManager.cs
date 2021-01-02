using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WavConfigTool.Classes
{
    class TaskManager
    {
        public void RequestWaveFormImageForPage(ProjectLineContainer container, int height)
        {
            if (container.IsLoadedImages || container.IsLoadingImages)
                return;
            if (!taskQueue.ContainsKey(container) && !runningTasks.ContainsKey(container))
                RequestWaveFormImages(container, height);
            prioritiesedTasks.Add(container);
        }

        public void RequestWaveFormImages(ProjectLineContainer container, int height)
        {
            var task = new Task(() => container.LoadImages(height));
            task.ContinueWith(delegate { HandleLoaded(container); });
            taskQueue[container] = task;
        }

        public void Start()
        {
            Console.WriteLine("TaskManager: Started");
            StartTask();
        }

        private const int MAX_TASK_COUNT = 10;

        private List<ProjectLineContainer> prioritiesedTasks = new List<ProjectLineContainer>();
        private ConcurrentDictionary<ProjectLineContainer, Task> taskQueue = new ConcurrentDictionary<ProjectLineContainer, Task>();
        private ConcurrentDictionary<ProjectLineContainer, Task> runningTasks = new ConcurrentDictionary<ProjectLineContainer, Task>();

        private void StartTask()
        {
            if (taskQueue.Count == 0)
            {
                Console.WriteLine("TaskManager: Completed");
                return;
            }

            var container = TakeTaskContainer();
            if (container == null)
            {
                throw new Exception();
            }

            taskQueue.TryRemove(container, out var task);
            if (task != null)
            {
                runningTasks.TryAdd(container, task);
                task.Start();
            }

            if (runningTasks.Count < MAX_TASK_COUNT)
                StartTask();
        }

        private ProjectLineContainer TakeTaskContainer()
        {
            while (prioritiesedTasks.Count() > 0)
            {
                var container = prioritiesedTasks[0];
                prioritiesedTasks.RemoveAt(0);
                if (taskQueue.ContainsKey(container))
                {
                    Console.WriteLine($"TaskManager [{runningTasks.Count}]: Loading with priority " + container.ToString());
                    return container;
                }
            }

            foreach (var pair in taskQueue)
            {
                var container = pair.Key;
                Console.WriteLine($"TaskManager [{runningTasks.Count}]: Loading " + container.ToString());
                return container;
            }
            return null;
        }

        private void HandleLoaded(ProjectLineContainer container)
        {
            Console.WriteLine("TaskManager: " + container.ToString() + " loaded");
            runningTasks.TryRemove(container, out _);
            StartTask();
        }
    }
}
