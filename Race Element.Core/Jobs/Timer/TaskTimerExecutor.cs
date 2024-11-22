using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaceElement.Core.Jobs.Timer;

public class TaskTimerExecutor
{
    /// <summary>
    /// Singleton instance so it can be accessed from anywhere.
    /// This should be thread-safe but don't know if there
    /// are any other major issues doing the initialization
    /// this way.
    /// </summary>
    private static TaskTimerExecutor _instance = new();

    /// <summary>
    /// Queue that will store the jobs to execute.
    /// </summary>
    private ConcurrentListTimerData _queue = new();

    /// <summary>
    /// Thread that will loop over all the task on the queue
    /// and run them when it is needed.
    /// </summary>
    private Thread _thread = null;

    /// <summary>
    /// Is worker thread running.
    /// </summary>
    private bool _running = false;

    /// <summary>
    /// Value used to identify the tasks added to the queue.
    /// This is incremental.
    /// </summary>
    private long _identifier = 0;

    /// <summary>
    /// Used by worker thread to sleep until next tick or wake up on cancel.
    /// </summary>
    private readonly EventWaitHandle _jobSleepEvent = new(false, EventResetMode.AutoReset);

    /// <summary>
    /// Get the timer instance.
    /// </summary>
    /// <returns>The Timer instance</returns>
    public static TaskTimerExecutor Instance()
    {
        return _instance;
    }

    /// <summary>
    /// Stop que working thread and clear the queue. Call dispose only when the
    /// application has to be closed as it stops the working thread.
    /// </summary>
    public void Dispose()
    {
        _running = false;
        _jobSleepEvent.Set();

        _thread.Join();
        _queue.Clear();
    }

    /// <summary>
    /// Add a new element to the execution list. The list is ordered by the
    /// execution time point. If "TimePoint" is lower the "DateTime.now()"
    /// the element will not be added.
    /// </summary>
    /// <param name="callback">User callback to execute.</param>
    /// <param name="timePoint">Point in time when to execute.</param>
    /// <param name="identifier">Identifier given by the system to the task.</param>
    /// <returns>True on success, false otherwise.</returns>
    public bool Add(IJob callback, DateTime timePoint, out long identifier)
    {
        if (DateTime.Now > timePoint)
        {
            identifier = 0;
            return false;
        }

        TimerData timerData = new();
        identifier = Interlocked.Increment(ref _identifier);

        timerData.Id = identifier;
        timerData.Callback = callback;
        timerData.TimePoint = timePoint;

        _queue.Add(timerData);
        _jobSleepEvent.Set();

        return true;
    }

    /// <summary>
    /// Remove element from the execution queue. Will return false if the
    /// element is not found.
    /// </summary>
    /// <param name="identifier">Identifier of the task to remove.</param>
    /// <returns>True on success, false otherwise.</returns>
    public bool RemoveTimer(long identifier)
    {
        var result = _queue.Remove(identifier);
        _jobSleepEvent.Set();
        return result;
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Default constructor. Creates the worker threads.
    /// </summary>
    private TaskTimerExecutor()
    {
        _thread = new Thread(Worker);
        _running = true;
        _thread.Start();
    }

    /// <summary>
    /// Hack to run execute function callback inside a C# Task.
    /// </summary>
    /// <param name="job">Jos to execute.</param>
    private void Callback(IJob job)
    {
        job.Run();
    }

    /// <summary>
    /// Peeks the first task in the queue and sees if it has to be executed.
    /// If the queue is empty or there is no task to execute the worker it
    /// will sleep until a task is added or the first task has to be
    /// executed. Tasks have to be ordered by execution time point.
    /// </summary>
    private void Worker()
    {
        while (_running)
        {
            if (_queue.TryPeekFront(out TimerData timerData))
            {
                var diff = timerData.TimePoint - DateTime.Now;
                if (diff.TotalMilliseconds <= 0)
                {
                    Task.Factory.StartNew(() => { Callback(timerData.Callback); });
                    _queue.TryFront(out TimerData _);
                } else _jobSleepEvent.WaitOne((int)diff.TotalMilliseconds);
            } else _jobSleepEvent.WaitOne();
        }
    }
}
