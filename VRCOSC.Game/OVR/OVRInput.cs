// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.CompilerServices;
using Valve.VR;

namespace VRCOSC.Game.OVR;

public class OVRInput
{
    private readonly OVRClient client;

    private static readonly uint vractiveactonset_t_size = (uint)Unsafe.SizeOf<VRActiveActionSet_t>();

    private ulong mainActionSetHandle;
    private ulong hapticActionSetHandle;
    private readonly ulong[] leftControllerActions = new ulong[8];
    private readonly ulong[] rightControllerActions = new ulong[8];
    private readonly ulong[] hapticActions = new ulong[2];

    public ulong LeftControllerHapticActionHandle => hapticActions[0];
    public ulong RightControllerHapticActionHandle => hapticActions[1];

    public OVRInput(OVRClient client)
    {
        this.client = client;
    }

    public void Init()
    {
        Valve.VR.OpenVR.Input.SetActionManifestPath(client.Metadata.ActionManifest);
        getActionHandles();
    }

    private void getActionHandles()
    {
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/lefta", ref leftControllerActions[0]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/leftb", ref leftControllerActions[1]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/leftpad", ref leftControllerActions[2]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/leftstick", ref leftControllerActions[3]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/leftfingerindex", ref leftControllerActions[4]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/leftfingermiddle", ref leftControllerActions[5]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/leftfingerring", ref leftControllerActions[6]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/leftfingerpinky", ref leftControllerActions[7]);

        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/righta", ref rightControllerActions[0]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/rightb", ref rightControllerActions[1]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/rightpad", ref rightControllerActions[2]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/rightstick", ref rightControllerActions[3]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/rightfingerindex", ref rightControllerActions[4]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/rightfingermiddle", ref rightControllerActions[5]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/rightfingerring", ref rightControllerActions[6]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/rightfingerpinky", ref rightControllerActions[7]);

        Valve.VR.OpenVR.Input.GetActionHandle("/actions/haptic/out/hapticsleft", ref hapticActions[0]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/haptic/out/hapticsright", ref hapticActions[1]);

        Valve.VR.OpenVR.Input.GetActionSetHandle("/actions/main", ref mainActionSetHandle);
        Valve.VR.OpenVR.Input.GetActionSetHandle("/actions/haptic", ref hapticActionSetHandle);
    }

    public void Update()
    {
        updateActionSet();
        updateDevices();
    }

    private void updateActionSet()
    {
        var activeActionSet = new VRActiveActionSet_t[]
        {
            new()
            {
                ulActionSet = mainActionSetHandle,
                ulRestrictedToDevice = Valve.VR.OpenVR.k_ulInvalidInputValueHandle,
                nPriority = 0
            },
            new()
            {
                ulActionSet = hapticActionSetHandle,
                ulRestrictedToDevice = Valve.VR.OpenVR.k_ulInvalidInputValueHandle,
                nPriority = 0
            }
        };
        Valve.VR.OpenVR.Input.UpdateActionState(activeActionSet, vractiveactonset_t_size);
    }

    private void updateDevices()
    {
        var leftControllerState = client.LeftController.Input;

        leftControllerState.A.Touched = OVRHelper.GetDigitalInput(leftControllerActions[0]).bState;
        leftControllerState.B.Touched = OVRHelper.GetDigitalInput(leftControllerActions[1]).bState;
        leftControllerState.PadTouched = OVRHelper.GetDigitalInput(leftControllerActions[2]).bState;
        leftControllerState.StickTouched = OVRHelper.GetDigitalInput(leftControllerActions[3]).bState;
        leftControllerState.IndexFinger = OVRHelper.GetAnalogueInput(leftControllerActions[4]).x;
        leftControllerState.MiddleFinger = OVRHelper.GetAnalogueInput(leftControllerActions[5]).x;
        leftControllerState.RingFinger = OVRHelper.GetAnalogueInput(leftControllerActions[6]).x;
        leftControllerState.PinkyFinger = OVRHelper.GetAnalogueInput(leftControllerActions[7]).x;

        var rightControllerState = client.RightController.Input;

        rightControllerState.A.Touched = OVRHelper.GetDigitalInput(rightControllerActions[0]).bState;
        rightControllerState.B.Touched = OVRHelper.GetDigitalInput(rightControllerActions[1]).bState;
        rightControllerState.PadTouched = OVRHelper.GetDigitalInput(rightControllerActions[2]).bState;
        rightControllerState.StickTouched = OVRHelper.GetDigitalInput(rightControllerActions[3]).bState;
        rightControllerState.IndexFinger = OVRHelper.GetAnalogueInput(rightControllerActions[4]).x;
        rightControllerState.MiddleFinger = OVRHelper.GetAnalogueInput(rightControllerActions[5]).x;
        rightControllerState.RingFinger = OVRHelper.GetAnalogueInput(rightControllerActions[6]).x;
        rightControllerState.PinkyFinger = OVRHelper.GetAnalogueInput(rightControllerActions[7]).x;
    }
}
