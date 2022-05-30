using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;

namespace VRCOSC.Game.Modules.Modules.Windows;

public class WindowsModule : Module
{
    public override string Title => "Windows Shortcuts";
    public override string Description => "Allows Windows shortcuts to be run from VR.";
    public override string Author => "Buckminsterfullerene";
    public override Colour4 Colour => Color4Extensions.FromHex(@"00c3ff").Darken(0.5f);
    public override ModuleType Type => ModuleType.Integrations;
}

public enum WindowsInputParameters
{
    FullScreenshot,
    VRChatScreenshot,
    OnScreenKeyboard,

}
