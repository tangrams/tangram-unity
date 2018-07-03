using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Nextzen.Unity
{
    public class AsyncWorker
    {
        public delegate void Task();

        private Thread[] threads;
        private bool stopped;
        private Queue<Task> tasks;

        public AsyncWorker(int nThreads)
        {
            tasks = new Queue<Task>();
            threads = new Thread[nThreads];
            stopped = false;

            for (int i = 0; i < threads.Length; ++i)
            {
                threads[i] = new Thread(new ThreadStart(this.ThreadMain));
                threads[i].Start();
            }
        }

        // Immediately runs the task if no worker have been instantiated
        public void RunAsync(Task task)
        {
            if (stopped)
            {
                return;
            }

            if (threads.Length == 0)
            {
                task.Invoke();
                return;
            }

            lock (this)
            {
                tasks.Enqueue(task);

                // Awaken one waiting thread
                Monitor.Pulse(this);
            }
        }

        public int RemainingTasks()
        {
            if (threads.Length == 0)
            {
                return 0;
            }

            int tasksSize;

            lock (this)
            {
                tasksSize = tasks.Count;
            }

            return tasksSize;
        }

        public void JoinAll()
        {
            stopped = true;

            if (threads.Length == 0)
            {
                return;
            }

            lock (this)
            {
                // Awaken all waiting threads
                Monitor.PulseAll(this);
            }

            for (int i = 0; i < threads.Length; ++i)
            {
                threads[i].Join();
            }
        }

        public void ClearTasks()
        {
            lock (this)
            {
                tasks.Clear();
            }
        }

        private void ThreadMain()
        {
            while (!stopped)
            {
                Task task = null;

                lock (this)
                {
                    // If a thread calls Pulse when no other threads are waiting, the Pulse is lost,
                    // make sure that this threads keep consuming tasks unless the queue is empty.
                    if (tasks.Count == 0)
                    {
                        Monitor.Wait(this);
                    }

                    // This thread might be awaken from a JoinAll() call with an empty queue
                    if (tasks.Count > 0)
                    {
                        task = tasks.Dequeue();
                    }
                }

                if (task != null)
                {
                    try
                    {
                        task.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }
    }
}