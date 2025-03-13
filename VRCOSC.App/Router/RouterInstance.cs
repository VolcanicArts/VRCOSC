// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Router;

public class RouterInstance
{
    public Observable<string> Name { get; } = new("My Router Instance");
    public Observable<bool> ReceiveEnabled { get; } = new();
    public Observable<string> ReceiveEndpoint { get; } = new($"{IPAddress.Loopback}:9000");
    public Observable<bool> SendEnabled { get; } = new(true);
    public Observable<string> SendEndpoint { get; } = new($"{IPAddress.Loopback}:9000");
}