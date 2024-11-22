using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace RaceElement.Core.Jobs.Timer;

internal class TimerData
{
    /// <summary>
    /// Worker identifier. Used to access it from outside.
    /// </summary>
    public long Id;

    /// <summary>
    /// Function to execute.
    /// </summary>
    public IJob Callback;

    /// <summary>
    /// Point in time when to execute the function.
    /// </summary>
    public DateTime TimePoint;
}

internal class ConcurrentListTimerData
{
    /// <summary>
    /// List of TimerData objects.
    /// </summary>
    private List<TimerData> _list = new();

    /// <summary>
    /// Use to avoid concurrent access to the list.
    /// </summary>
    private readonly Lock _lockObj = new();

    /// <summary>
    /// Try to get the first element of the list without removing it from it.
    /// </summary>
    /// <param name="data">Out data.</param>
    /// <returns>True if element peeked, else otherwise.</returns>
    public bool TryPeekFront(out TimerData data)
    {
        lock (_lockObj)
        {
            if (_list.Count == 0)
            {
                data = null;
                return false;
            }

            data = _list[0];
            return true;
        }
    }

    /// <summary>
    /// Try to get the first element of the list. This method removes the element from it.
    /// </summary>
    /// <param name="data">Out data.</param>
    /// <returns>True if element is removed, else otherwise.</returns>
    public bool TryFront(out TimerData data)
    {
        lock (_lockObj)
        {
            if (_list.Count == 0)
            {
                data = null;
                return false;
            }

            data = _list[0];
            _list.RemoveAt(0);

            return true;
        }
    }

    /// <summary>
    /// Add an element to the list and sort it by "TimePoint".
    /// </summary>
    /// <param name="data">Element to add.</param>
    /// <returns>True if added, false otherwise.</returns>
    public bool Add(TimerData data)
    {
       lock (_lockObj)
        {
            _list.Add(data);
            _list.Sort((a, b) => a.TimePoint.CompareTo(b.TimePoint));
        }

        return true;
    }

    /// <summary>
    /// Remove the element with the given ID from the list.
    /// </summary>
    /// <param name="id">Element identifier.</param>
    /// <returns>True if the element has been removed, false otherwise.</returns>
    public bool Remove(long id)
    {
        lock (_lockObj)
        {
            var item = _list.SingleOrDefault(r => r.Id == id);
            if (item == null) return false;

            _list.Remove(item);
            return true;
        }
    }

    /// <summary>
    /// Remove all elements from the list.
    /// </summary>
    public void Clear()
    {
        lock (_lockObj)
        {
            _list.Clear();
        }
    }
}
