using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RaceElement.Core.Jobs.Timer;

public sealed class JobTimerExecutor
{
    /// <summary>
    /// Singleton instance so it can be accessed from anywhere.
    /// This should be thread-safe but don't know if there
    /// are any other major issues doing the initialization
    /// this way.
    /// </summary>
    private static readonly JobTimerExecutor _instance = new();

    /// <summary>
    /// Queue that will store the jobs to execute.
    /// </summary>
    //private readonly ConcurrentListTimerData _queue = new();

    private readonly ConcurrentDictionary<Guid, TimerData> _dic = [];

    /// <summary>
    /// Thread that will loop over all the task on the queue
    /// and run them when it is needed.
    /// </summary>
    private readonly Thread _thread = null;

    /// <summary>
    /// Is worker thread running.
    /// </summary>
    private bool _running = false;

    /// <summary>
    /// Used by worker thread to sleep until next clock, wake up because of add/remove or cancel.
    /// </summary>
    private readonly EventWaitHandle _jobWaitEvent = new(false, EventResetMode.AutoReset);

    /// <summary>
    /// Get the timer instance.
    /// </summary>
    /// <returns>The Timer instance</returns>
    public static JobTimerExecutor Instance() => _instance;

    /// <summary>
    /// Stop que working thread and clear the queue. Call dispose only when the
    /// application has to be closed as it stops the working thread.
    /// </summary>
    public void Dispose()
    {
        _running = false;
        _jobWaitEvent.Set();

        _thread?.Join();
        _dic.Clear();
    }

    /// <summary>
    /// Add a new element to the execution list. The list is ordered by the
    /// execution time point.
    /// <br/> 
    /// <br/> The element will not be added if:
    /// <br/> - <paramref name="timePoint"/> is earlier in time than <see cref="DateTime.UtcNow"/>
    /// <br/> - the job id of the given job is <see cref="Guid.Empty"/>
    /// <br/> - the job <see cref="IJob.IsRunning"/>
    /// </summary>
    /// <param name="job">User callback to execute.</param>
    /// <param name="timePoint">UTC Point in time when to execute.</param>
    /// <param name="jobId">Guid of the job, if the timepoint is in the past, it will be <see cref="Guid.Empty"/></param>
    /// <returns>True on success, false otherwise.</returns>
    public bool Add(IJob job, DateTime timePoint, out Guid jobId)
    {
        if (DateTime.UtcNow > timePoint || job.JobId == Guid.Empty || job.IsRunning)
        {
            jobId = Guid.Empty;
            return false;
        }

        TimerData timerData = new()
        {
            Job = job,
            TimePoint = timePoint
        };

        if (_dic.TryAdd(job.JobId, timerData))
        {
            jobId = job.JobId;

            _jobWaitEvent.Set();
            return true;
        }

        jobId = Guid.Empty;
        return false;
    }

    /// <summary>
    /// Remove element from the execution queue. Will return false if the
    /// element is not found.
    /// </summary>
    /// <param name="identifier">Identifier of the task to remove.</param>
    /// <returns>True on success, false otherwise.</returns>
    public bool Remove(Guid identifier)
    {
        if (_dic.TryRemove(identifier, out TimerData _))
        {
            _jobWaitEvent.Set();
            return true;
        }

        return false;
    }

    ///////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Default constructor. Creates the worker threads.
    /// </summary>
    private JobTimerExecutor()
    {
        _thread = new Thread(Worker);
        _running = true;
        _thread.Start();
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
            Parallel.ForEach(_dic, (KeyValuePair<Guid, TimerData> kv) =>
            {
                if (kv.Value.Job.IsRunning) return;

                if (!kv.Value.WasStarted && DateTime.UtcNow > kv.Value.TimePoint)
                {
                    kv.Value.WasStarted = true;
                    Task.Factory.StartNew(kv.Value.Job.Run);
                }
            });

            var earliest = _dic.Where(x => !x.Value.WasStarted && x.Value.TimePoint > DateTime.UtcNow)
                 .OrderByDescending(x => x.Value.TimePoint)
                 .LastOrDefault();
            if (earliest.Key != Guid.Empty)
            {
                var timeDifference = (earliest.Value.TimePoint - DateTime.UtcNow);
                if (timeDifference > TimeSpan.FromMilliseconds(1))
                {
                    _jobWaitEvent.WaitOne(timeDifference);
                    continue;
                }
            }

            _jobWaitEvent.WaitOne(TimeSpan.FromMinutes(1));
        }
    }

    private sealed class TimerData
    {
        /// <summary>
        /// Function to execute.
        /// </summary>
        public IJob Job { get; init; }

        /// <summary>
        /// Point in time when to execute the function.
        /// </summary>
        public DateTime TimePoint { get; init; }

        /// <summary>
        /// Determines whether the job was started by the <see cref="JobTimerExecutor"/>. 
        /// </summary>
        public bool WasStarted { get; internal set; } = false;
    }
}
