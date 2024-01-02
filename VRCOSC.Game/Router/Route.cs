// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Net;
using osu.Framework.Bindables;

namespace VRCOSC.Router;

public class Route
{
    /// <summary>
    /// The name of this <see cref="Route"/>
    /// </summary>
    public readonly Bindable<string> Name = new("My new route");

    /// <summary>
    /// The endpoint to forward VRChat's data to
    /// </summary>
    public readonly Bindable<IPEndPoint> Endpoint = new(IPEndPoint.Parse("127.0.0.1:1234"));
}
