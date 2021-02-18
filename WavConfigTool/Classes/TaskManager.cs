using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WavConfigTool.ViewModels;

namespace WavConfigTool.Classes
{
    class TaskManager
    {
        public void RequestWaveFormImageForPage(IList<WavControlBaseViewModel> pageContent, int height)
        {
            var oldPageContent = currentPageContent;
            currentPageContent = new List<ProjectLineContainer>();
            foreach (var controller in pageContent)
            {
                var wavControl = controller as WavControlViewModel;
                if (wavControl == null)
                    continue;
                var container = (ProjectLineContainer)wavControl.PagerContent;
                if (oldPageContent.Contains(container))
                {
                    oldPageContent.Remove(container);
                }
                else
                {
                    if (container.IsLoadedImages || container.IsLoadingImages)
                        continue;
                    if (!taskQueue.ContainsKey(container) && !runningTasks.ContainsKey(container))
                        RequestWaveFormImages(container, height);
                }

                currentPageContent.Add(container);
            }

            foreach (var container in oldPageContent)
            {
                container.ReleaseImages();
            }

            Console.WriteLine("TaskManager: Start");
            StartTask();
        }

        public void CancelAll()
        {
            // TODO: add cancellation tokens
            taskQueue.Clear();
        }

        private const int MAX_TASK_COUNT = 10;

        private List<ProjectLineContainer> currentPageContent = new List<ProjectLineContainer>();
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
            if (container != null)
            {
                taskQueue.TryRemove(container, out var task);
                if (task != null)
                {
                    runningTasks.TryAdd(container, task);
                    try
                    {
                        task.Start();
                    }
                    catch (TaskCanceledException) { }
                }
            }

            if (runningTasks.Count < MAX_TASK_COUNT)
                StartTask();
        }

        private void RequestWaveFormImages(ProjectLineContainer container, int height)
        {
            var task = new Task(() => ExceptionCatcher.Current.CatchOnAction(() => container.LoadImages(height), $"Failed to load images for [{container}]"));
            task.ContinueWith(delegate
            {
                try
                {
                    App.MainDispatcher.Invoke(delegate
                    {
                        container.FinishImagesLoading(true);
                        HandleLoaded(container);
                    });
                }
                catch (TaskCanceledException) { }
            });
            taskQueue[container] = task;
        }

        private ProjectLineContainer TakeTaskContainer()
        {
            while (currentPageContent.Count() > 0)
            {
                var container = currentPageContent[0];
                currentPageContent.RemoveAt(0);
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
