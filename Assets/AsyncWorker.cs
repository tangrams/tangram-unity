using System;
using System.Threading;
using System.Collections.Generic;

public class AsyncWorker {
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

	public void RunAsync(Task task)
	{
		if (stopped)
		{
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
				task.Invoke();
			}
		}
	}
}
