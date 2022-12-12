// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.OSC;

public class ConcurrentList<T> : List<T>
{
    private readonly object lockObject = new();

    public new void Add(T item)
    {
        lock (lockObject)
            base.Add(item);
    }

    public new void Remove(T item)
    {
        lock (lockObject)
            base.Remove(item);
    }

    public new void ForEach(Action<T> action)
    {
        lock (lockObject)
            base.ForEach(action);
    }
}
