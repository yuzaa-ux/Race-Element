using System;
using System.Threading;

namespace RaceElement.Core.Jobs;

public interface IJob
{
    public bool IsRunning { get; }
    public abstract void Run();
    public abstract void Cancel();
}

public class ReferenceProperty<T>
{
    private T[] _typeReference;

    public ReferenceProperty(T value)
    {
        _typeReference = new T[] { value };
    }

    public T PropertyAsValue
    {
        get { return _typeReference[0]; }
        set { _typeReference[0] = value; }
    }
    public T[] PropertyAsReference
    {
        get { return _typeReference; }
    }
}

public static class IJobExtensions
{
    /// <summary>
    /// Returns when the job has stopped running.
    /// </summary>
    /// <param name="job"></param>
    /// <param name="checkIntervalMilliseconds"></param>
    public static void WaitForCompletion(this IJob job, int checkIntervalMilliseconds = 100)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(checkIntervalMilliseconds, 1);

        while (job.IsRunning)
            Thread.Sleep(checkIntervalMilliseconds);
    }

    /// <summary>
    /// Returns when the job has stopped running, whilst executing the action based on the check interval.
    /// </summary>
    /// <param name="job"></param>
    /// <param name="action"></param>
    /// <param name="checkIntervalMilliseconds"></param>
    public static void WaitForCompletionAndDo(this IJob job, Action action, int checkIntervalMilliseconds = 100)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(checkIntervalMilliseconds, 1);

        while (job.IsRunning)
        {
            action();
            Thread.Sleep(checkIntervalMilliseconds);
        }
    }
}
