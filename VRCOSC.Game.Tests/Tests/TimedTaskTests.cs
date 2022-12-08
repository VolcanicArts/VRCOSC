// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Tests.Tests;

public class TimedTaskTests
{
    [Test]
    public void TestStart()
    {
        var actionInvoked = false;

        Task action()
        {
            actionInvoked = true;
            return Task.CompletedTask;
        }

        var task = new TimedTask(action, 100);
        task.Start().Wait();

        Thread.Sleep(500);

        Assert.IsTrue(actionInvoked);
    }

    [Test]
    public void TestStop()
    {
        var actionInvoked = false;

        Task action()
        {
            actionInvoked = true;
            return Task.CompletedTask;
        }

        var task = new TimedTask(action, 100);
        task.Start().Wait();

        Thread.Sleep(500);
        Assert.IsTrue(actionInvoked);

        task.Stop().Wait();

        actionInvoked = false;

        Thread.Sleep(500);
        Assert.IsFalse(actionInvoked);
    }

    [Test]
    public void TestExecuteOnceImmediately()
    {
        var actionInvoked = false;

        Task action()
        {
            actionInvoked = true;
            return Task.CompletedTask;
        }

        var task = new TimedTask(action, 100, true);
        task.Start().Wait();

        Assert.IsTrue(actionInvoked);

        Thread.Sleep(500);
        Assert.IsTrue(actionInvoked);
    }

    [Test]
    public void TestStartMultipleTimes()
    {
        var actionInvoked = false;

        Task action()
        {
            actionInvoked = true;
            return Task.CompletedTask;
        }

        var task = new TimedTask(action, 100);
        task.Start().Wait();
        task.Start().Wait();

        Thread.Sleep(500);

        Assert.IsTrue(actionInvoked);
    }

    [Test]
    public void TestStopWithoutStart()
    {
        var actionInvoked = false;

        Task action()
        {
            actionInvoked = true;
            return Task.CompletedTask;
        }

        var task = new TimedTask(action, 100);

        task.Stop().Wait();

        Thread.Sleep(500);
        Assert.IsFalse(actionInvoked);
    }
}
