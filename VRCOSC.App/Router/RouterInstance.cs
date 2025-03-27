// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Router;

public class RouterInstance
{
    public Observable<string> Name { get; } = new("My Router Instance");
    public Observable<RouterMode> Mode { get; } = new(RouterMode.Send);
    public Observable<string> Endpoint { get; } = new($"{IPAddress.Loopback}:9000");
}

public enum RouterMode
{
    Send,
    Receive
}