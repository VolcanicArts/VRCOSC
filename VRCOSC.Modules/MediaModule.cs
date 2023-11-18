// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules.SDK;
using VRCOSC.Game.Modules.SDK.Parameters;
using VRCOSC.Game.Modules.SDK.Providers.Media;

namespace VRCOSC.Modules;

[ModuleTitle("Media")]
[ModuleDescription("Integration with Windows Media")]
[ModuleType(ModuleType.Integrations)]
[ModulePrefab("VRCOSC-Media", "https://github.com/VolcanicArts/VRCOSC/releases/download/latest/VRCOSC-Media.unitypackage")]
public class MediaModule : Module
{
    private readonly MediaProvider mediaProvider = new WindowsMediaProvider();
    private bool currentlySeeking;
    private TimeSpan targetPosition;

    public MediaModule()
    {
        mediaProvider.OnPlaybackStateChange += onPlaybackStateChange;
        mediaProvider.OnLog += Log;
    }

    protected override void OnLoad()
    {
        RegisterParameter<bool>(MediaParameter.Play, "VRCOSC/Media/Play", ParameterMode.ReadWrite, "Play/Pause", "True for playing. False for paused");
        RegisterParameter<float>(MediaParameter.Volume, "VRCOSC/Media/Volume", ParameterMode.ReadWrite, "Volume", "The volume of the process that is controlling the media");
        RegisterParameter<int>(MediaParameter.Repeat, "VRCOSC/Media/Repeat", ParameterMode.ReadWrite, "Repeat", "0 for disabled. 1 for single. 2 for list");
        RegisterParameter<bool>(MediaParameter.Shuffle, "VRCOSC/Media/Shuffle", ParameterMode.ReadWrite, "Shuffle", "True for enabled. False for disabled");
        RegisterParameter<bool>(MediaParameter.Next, "VRCOSC/Media/Next", ParameterMode.Read, "Next", "Becoming true causes the next track to play");
        RegisterParameter<bool>(MediaParameter.Previous, "VRCOSC/Media/Previous", ParameterMode.Read, "Previous", "Becoming true causes the previous track to play");
        RegisterParameter<bool>(MediaParameter.Seeking, "VRCOSC/Media/Seeking", ParameterMode.Read, "Seeking", "Whether the user is currently seeking");
        RegisterParameter<float>(MediaParameter.Position, "VRCOSC/Media/Position", ParameterMode.ReadWrite, "Position", "The position of the song as a percentage");
    }

    protected override async Task<bool> OnModuleStart()
    {
        return await hookIntoMedia();
    }

    private async Task<bool> hookIntoMedia()
    {
        if (await mediaProvider.InitialiseAsync()) return true;

        Log("Could not hook into Windows media");
        Log("Try restarting the module\nIf this persists you will need to restart your PC as Windows has not initialised the media service correctly");
        return false;
    }

    protected override async Task OnModuleStop()
    {
        await mediaProvider.TerminateAsync();
    }

    protected override void OnAvatarChange()
    {
        sendUpdatableParameters();
        sendMediaParameters();
    }

    [ModuleUpdate(ModuleUpdateMode.Custom)]
    private void fixedUpdate()
    {
        if (mediaProvider.State.IsPlaying)
        {
            // Hack to allow browsers to have time info
            mediaProvider.Update(TimeSpan.FromMilliseconds(50));
        }
    }

    [ModuleUpdate(ModuleUpdateMode.Custom, true, 1000)]
    private void sendUpdatableParameters()
    {
        SendParameter(MediaParameter.Volume, mediaProvider.TryGetVolume());

        if (!currentlySeeking)
        {
            SendParameter(MediaParameter.Position, mediaProvider.State.Timeline.Progress);
        }
    }

    private void onPlaybackStateChange()
    {
        sendMediaParameters();
    }

    private void sendMediaParameters()
    {
        SendParameter(MediaParameter.Play, mediaProvider.State.IsPlaying);
        SendParameter(MediaParameter.Shuffle, mediaProvider.State.IsShuffle);
        SendParameter(MediaParameter.Repeat, (int)mediaProvider.State.RepeatMode);
    }

    protected override void OnRegisteredParameterReceived(RegisteredParameter parameter)
    {
        switch (parameter.Lookup)
        {
            case MediaParameter.Volume:
                mediaProvider.TryChangeVolume(parameter.GetValue<float>());
                break;

            case MediaParameter.Position:
                if (!currentlySeeking) return;

                var position = mediaProvider.State.Timeline;
                targetPosition = (position.End - position.Start) * parameter.GetValue<float>();
                break;

            case MediaParameter.Repeat:
                mediaProvider.ChangeRepeatMode((MediaRepeatMode)parameter.GetValue<int>());
                break;

            case MediaParameter.Play:
                if (parameter.GetValue<bool>())
                    mediaProvider.Play();
                else
                    mediaProvider.Pause();
                break;

            case MediaParameter.Shuffle:
                mediaProvider.ChangeShuffle(parameter.GetValue<bool>());
                break;

            case MediaParameter.Next when parameter.GetValue<bool>():
                mediaProvider.SkipNext();
                break;

            case MediaParameter.Previous when parameter.GetValue<bool>():
                mediaProvider.SkipPrevious();
                break;

            case MediaParameter.Seeking:
                currentlySeeking = parameter.GetValue<bool>();
                if (!currentlySeeking) mediaProvider.ChangePlaybackPosition(targetPosition);
                break;
        }
    }

    private enum MediaParameter
    {
        Play,
        Next,
        Previous,
        Shuffle,
        Repeat,
        Volume,
        Seeking,
        Position
    }
}
