// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Modules;
using VRCOSC.OSC.VRChat;

namespace VRCOSC.Game.Graphics.ModuleRun;

public sealed partial class ParameterContainer : Container
{
    [Resolved]
    private GameManager gameManager { get; set; } = null!;

    private readonly ParameterSubContainer outgoingParameterDisplay;
    private readonly ParameterSubContainer incomingParameterDisplay;

    public ParameterContainer()
    {
        RelativeSizeAxes = Axes.Both;
        Padding = new MarginPadding
        {
            Left = 15 / 2f
        };

        Child = new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            RowDimensions = new[]
            {
                new Dimension(),
                new Dimension()
            },
            Content = new[]
            {
                new Drawable[]
                {
                    outgoingParameterDisplay = new ParameterSubContainer
                    {
                        Title = "Outgoing",
                        Padding = new MarginPadding
                        {
                            Bottom = 15 / 2f
                        }
                    }
                },
                new Drawable[]
                {
                    incomingParameterDisplay = new ParameterSubContainer
                    {
                        Title = "Incoming",
                        Padding = new MarginPadding
                        {
                            Top = 15 / 2f
                        }
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        gameManager.OscClient.OnParameterSent += onParameterSent;
        gameManager.OscClient.OnParameterReceived += onParameterReceived;
    }

    private void onParameterSent(VRChatOscData data)
    {
        outgoingParameterDisplay.AddEntry(data.Address, data.ParameterValue);
    }

    private void onParameterReceived(VRChatOscData data)
    {
        incomingParameterDisplay.AddEntry(data.Address, data.ParameterValue);
    }

    public void ClearParameters()
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

            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                BorderThickness = 3,
                Masking = true,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ThemeManager.Current[ThemeAttribute.Darker]
                    },
                    parameterDisplay = new ParameterDisplay
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Vertical = 1.5f,
                            Horizontal = 3
                        },
                        Title = Title
                    }
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
        gameManager.OscClient.OnParameterSent -= onParameterSent;
        gameManager.OscClient.OnParameterReceived -= onParameterReceived;
    }
}
