// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Windows.Threading;

namespace VRCOSC.App.Utils;

// ReSharper disable InconsistentNaming
public class DTWrapper
{
    private readonly string name;
    private readonly bool runOnceImmediately;
    private readonly Action callback;
    private readonly DispatcherTimer dt;

    public DTWrapper(string name, TimeSpan interval, bool runOnceImmediately, Action callback)
    {
        this.name = name;
        this.runOnceImmediately = runOnceImmediately;
        this.callback = callback;

        dt = new DispatcherTimer
        {
            Interval = interval
        };

        dt.Tick += (_, _) => invokeCallback();
    }

    private void invokeCallback()
    {
        try
        {
            callback();
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{nameof(DTWrapper)}:{name} has experienced an exception");
        }
    }

    public void Start()
    {
        if (runOnceImmediately)
        {
            invokeCallback();
        }

        dt.Start();
    }

    public void Stop()
    {
        dt.Stop();
    }
}