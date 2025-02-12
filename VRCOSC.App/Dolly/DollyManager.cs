// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.Dolly.Serialisation;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Profiles;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Dolly;

public class DollyManager
{
    private static DollyManager? instance;
    internal static DollyManager GetInstance() => instance ??= new DollyManager();

    private VRChatOscClient oscClient => AppManager.GetInstance().VRChatOscClient;
    private Storage dollyStorage => AppManager.GetInstance().Storage.GetStorageForDirectory(Path.Join("profiles", ProfileManager.GetInstance().ActiveProfile.Value.ID.ToString(), "dollies"));
    private string vrchatDollyDirectoryPath => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VRChat", "CameraPaths");

    public ObservableCollection<Dolly> Dollies { get; } = [];

    private readonly SerialisationManager serialisationManager;
    private bool isLoaded;

    public Action? OnPlay;
    public Action? OnStop;

    private DollyManager()
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new DollyManagerSerialiser(AppManager.GetInstance().Storage, this));

        Dollies.OnCollectionChanged((newItems, oldItems) =>
        {
            foreach (var newDolly in newItems)
            {
                newDolly.Name.Subscribe(_ => serialisationManager.Serialise());
            }

            if (!isLoaded) return;

            foreach (var oldDolly in oldItems)
            {
                var filePath = dollyStorage.GetFullPath($"{oldDolly.Id.ToString()}.json");
                File.Delete(filePath);
            }
        }, true);

        Dollies.OnCollectionChanged((_, _) =>
        {
            if (!isLoaded) return;

            serialisationManager.Serialise();
        });
    }

    public void Unload()
    {
        serialisationManager.Serialise();
        isLoaded = false;
    }

    public void Load()
    {
        Dollies.Clear();
        serialisationManager.Deserialise();
        isLoaded = true;
    }

    public void Play()
    {
        oscClient.Send(VRChatOscConstants.ADDRESS_DOLLY_PLAY, true);
    }

    public void Stop()
    {
        oscClient.Send(VRChatOscConstants.ADDRESS_DOLLY_PLAY, false);
    }

    public void PlayDelayed(int secondsDelay)
    {
        oscClient.Send(VRChatOscConstants.ADDRESS_DOLLY_PLAYDELAYED, secondsDelay);
    }

    /// <summary>
    /// Imports the dolly file into VRChat
    /// </summary>
    public async void Import(Dolly dolly)
    {
        // not stopping causes vrchat to combine the paths and keep playing
        Stop();
        await Task.Delay(50);

        var filePath = dollyStorage.GetFullPath($"{dolly.Id.ToString()}.json");
        var contents = await File.ReadAllTextAsync(filePath);
        oscClient.Send(VRChatOscConstants.ADDRESS_DOLLY_IMPORT, contents);
    }

    /// <summary>
    /// Exports the current dolly out of VRChat
    /// </summary>
    public async Task Export()
    {
        var newDolly = new Dolly();
        var destinationFilePath = dollyStorage.GetFullPath($"{newDolly.Id.ToString()}.json");
        var currentDateTime = DateTime.Now;

        oscClient.Send(VRChatOscConstants.ADDRESS_DOLLY_EXPORT, [null]);
        await Task.Delay(100);

        var vrchatDollyDirectory = new DirectoryInfo(vrchatDollyDirectoryPath);
        var latestFile = vrchatDollyDirectory.GetFiles().Where(f => f.LastWriteTime >= currentDateTime).OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
        if (latestFile is null) return;

        File.Copy(Path.Join(vrchatDollyDirectoryPath, latestFile.Name), destinationFilePath);

        Dollies.Add(newDolly);
    }

    /// <summary>
    /// Imports a dolly file from a file path to be managed
    /// </summary>
    public void ImportFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        var newDolly = new Dolly();
        var destinationFilePath = dollyStorage.GetFullPath($"{newDolly.Id.ToString()}.json");

        File.Copy(filePath, destinationFilePath);

        Dollies.Add(newDolly);
    }

    public void HandleDollyEvent(VRChatOscMessage message)
    {
        if (message.Address == VRChatOscConstants.ADDRESS_DOLLY_PLAY)
        {
            var playing = (bool)message.Arguments[0]!;

            if (playing)
                OnPlay?.Invoke();
            else
                OnStop?.Invoke();
        }
    }
}