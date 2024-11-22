using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaceElement.Core.Jobs.Timer;

public class TimerTask
{
    /// <summary>
    /// Singleton instance so it can be accessed from anywhere.
    /// This should be thread-safe but don't know if there
    /// are any other major issues doing the initialization
    /// this way.
    /// </summary>
    private static TimerTask _instance = new();

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
    /// At what interval tasks have to be checked.
    /// </summary>
    private int _intervalMs = 250;

    /// <summary>
    ///
    /// </summary>
    private long _identifier = 0;

    /// <summary>
    /// Get the timer instance.
    /// </summary>
    /// <returns>The Timer instance</returns>
    public static TimerTask Instance()
    {
        return _instance;
    }

    /// <summary>
    /// Stop que working thread and clear the queue.
    /// </summary>
    public void Dispose()
    {
        _running = false;
        _thread.Join();
        _queue.Clear();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="job"></param>
    /// <param name="timePoint"></param>
    /// <param name="identifier"></param>
    /// <returns></returns>
    public bool AddTimer(IJob job, DateTime timePoint, out long identifier)
    {
        if (DateTime.Now > timePoint)
        {
            identifier = 0;
            return false;
        }

        TimerData timerData = new();
        identifier = ++_identifier;

        timerData.Callback = job;
        timerData.Id = identifier;
        timerData.TimePoint = timePoint;

        _queue.Add(timerData);
        return true;
    }

    public bool RemoveTimer(long identifier)
    {
        return _queue.Remove(identifier);
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Default constructor. Creates the worker threads.
    /// </summary>
    private TimerTask()
    {
        _running = true;
        _thread = new Thread(Worker);
        _thread.Start();
    }

    /// <summary>
    /// Hack to run user function call back inside a task.
    /// </summary>
    /// <param name="job">Jos to execute.</param>
    private void Callback(IJob job)
    {
        job.Run();
    }

    /// <summary>
    /// Peeks the first task on the queue and sees if it has to be executed.
    /// If the queue is empty or there is no task to execute the worker it
    /// will sleep "_intervalMs". Tasks have to be ordered by execution
    /// time point.
    /// </summary>
    private void Worker()
    {
        while (_running)
        {
            if (_queue.TryPeekFront(out TimerData timerData))
            {
                if (DateTime.Now > timerData.TimePoint)
                {
                    Task.Factory.StartNew(() => { Callback(timerData.Callback); });
                    _queue.TryFront(out TimerData _);
                } else Thread.Sleep(_intervalMs);
            } else Thread.Sleep(_intervalMs);
        }
    }
}
