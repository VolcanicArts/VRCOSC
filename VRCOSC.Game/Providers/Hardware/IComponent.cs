// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.Game.Providers.Hardware;

public interface IComponent
{
}

public class CPU : IComponent
{
    public float Usage { get; internal set; }
    public int Temperature { get; internal set; }
}

public class GPU : IComponent
{
    public float Usage { get; internal set; }
    public int Temperature { get; internal set; }
}

public class RAM : IComponent
{
    public float Usage { get; internal set; }
    public float Used { get; internal set; }
    public float Available { get; internal set; }
    public float Total => Used + Available;
}
