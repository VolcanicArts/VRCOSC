// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Dolly;

public class Dolly
{
    public Guid Id { get; }
    public Observable<string> Name { get; } = new("New Dolly");

    public Dolly()
    {
        Id = Guid.NewGuid();
    }

    public Dolly(Guid id)
    {
        Id = id;
    }
}