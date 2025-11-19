// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using Velopack;
using VRCOSC.App;
using VRCOSC.App.UI.Windows;

namespace VRCOSC;

public static class Program
{
    private static MainApp app = null!;
    private static MainWindow window = null!;

    [STAThread]
    public static void Main(string[] args)
    {
        const string mutex_name = @"Global\VRCOSC_SingleInstanceMutex";
        using var mutex = new Mutex(true, mutex_name, out bool isFirstInstance);

        if (!isFirstInstance)
        {
            sendShowCommand();
            return;
        }

        VelopackApp.Build().Run();

        startPipeServer();

        app = new MainApp();
        window = new MainWindow(args);
        app.Run(window);
    }

    private static void sendShowCommand()
    {
        try
        {
            using var client = new NamedPipeClientStream(".", "VRCOSC_IPC", PipeDirection.Out);
            client.Connect(500);
            using var writer = new StreamWriter(client);
            writer.AutoFlush = true;
            writer.WriteLine("SHOW");
        }
        catch
        {
        }
    }

    private static void startPipeServer()
    {
        var thread = new Thread(() =>
        {
            while (true)
            {
                try
                {
                    using var server = new NamedPipeServerStream("VRCOSC_IPC", PipeDirection.In);
                    server.WaitForConnection();

                    using var reader = new StreamReader(server);
                    var command = reader.ReadLine();

                    if (string.Equals(command, "SHOW", StringComparison.OrdinalIgnoreCase))
                    {
                        app.Dispatcher.BeginInvoke(() => window.TransitionTray(false));
                    }
                }
                catch
                {
                }
            }
        })
        {
            IsBackground = true
        };

        thread.Start();
    }
}