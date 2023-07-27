// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.App;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Graphics.Run;

public sealed partial class ParameterContainer : Container
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private readonly ParameterSubContainer outgoingParameterDisplay;
    private readonly ParameterSubContainer incomingParameterDisplay;

    public ParameterContainer()
    {
        Child = new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            RowDimensions = new[]
            {
                new Dimension(),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension()
            },
            Content = new[]
            {
                new Drawable[]
                {
                    outgoingParameterDisplay = new ParameterSubContainer
                    {
                        Title = "Outgoing"
                    }
                },
                null,
                new Drawable[]
                {
                    incomingParameterDisplay = new ParameterSubContainer
                    {
                        Title = "Incoming"
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        appManager.OSCClient.OnParameterSent += onParameterSent;
        appManager.OSCClient.OnParameterReceived += onParameterReceived;

        appManager.State.BindValueChanged(e =>
        {
            if (e.NewValue == AppManagerState.Starting) clearParameters();
        });
    }

    private void onParameterSent(VRChatOscMessage message)
    {
        outgoingParameterDisplay.AddEntry(message.Address, message.ParameterValue);
    }

    private void onParameterReceived(VRChatOscMessage message)
    {
        incomingParameterDisplay.AddEntry(message.Address, message.ParameterValue);
    }

    private void clearParameters()
    {
        outgoingParameterDisplay.ClearContent();
        incomingParameterDisplay.ClearContent();
    }

    private sealed partial class ParameterSubContainer : Container
    {
        private ParameterDisplay parameterDisplay = null!;

        public string Title { get; init; } = string.Empty;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 10;
            BorderThickness = 2;
            BorderColour = ThemeManager.Current[ThemeAttribute.Border];

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ThemeManager.Current[ThemeAttribute.Dark]
                },
                parameterDisplay = new ParameterDisplay
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Vertical = 2,
                        Horizontal = 7
                    },
                    Title = Title
                }
            };
        }

        public void AddEntry(string key, object value)
            => parameterDisplay.AddEntry(key, value);

        public void ClearContent()
            => parameterDisplay.ClearContent();
    }

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);
        appManager.OSCClient.OnParameterSent -= onParameterSent;
        appManager.OSCClient.OnParameterReceived -= onParameterReceived;
    }
}
