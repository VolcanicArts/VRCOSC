using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Modules;

public abstract class Module : Container
{
    public abstract void Start();
    public abstract void Stop();
}
