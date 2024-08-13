// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.CompilerServices;
using Valve.VR;
using VRCOSC.App.SDK.OVR.Device;
using VRCOSC.App.SDK.OVR.Input;

namespace VRCOSC.App.SDK.OVR;

public class OVRInput
{
    private readonly OVRClient client;

    private static readonly uint vractiveactonset_t_size = (uint)Unsafe.SizeOf<VRActiveActionSet_t>();

    private ulong mainActionSetHandle;
    private ulong hapticActionSetHandle;
    private readonly ulong[] leftControllerActions = new ulong[8];
    private readonly ulong[] rightControllerActions = new ulong[8];
    private readonly ulong[] hapticActions = new ulong[11];

    internal OVRInput(OVRClient client)
    {
        this.client = client;
    }

    internal void Init()
    {
        OpenVR.Input.SetActionManifestPath(client.Metadata!.ActionManifest);
        getActionHandles();
    }

    public ulong GetHapticActionHandle(DeviceRole device) => hapticActions[(int)device];

    private void getActionHandles()
    {
        OpenVR.Input.GetActionHandle("/actions/main/in/lefta", ref leftControllerActions[0]);
        OpenVR.Input.GetActionHandle("/actions/main/in/leftb", ref leftControllerActions[1]);
        OpenVR.Input.GetActionHandle("/actions/main/in/leftpad", ref leftControllerActions[2]);
        OpenVR.Input.GetActionHandle("/actions/main/in/leftstick", ref leftControllerActions[3]);
        OpenVR.Input.GetActionHandle("/actions/main/in/leftfingerindex", ref leftControllerActions[4]);
        OpenVR.Input.GetActionHandle("/actions/main/in/leftfingermiddle", ref leftControllerActions[5]);
        OpenVR.Input.GetActionHandle("/actions/main/in/leftfingerring", ref leftControllerActions[6]);
        OpenVR.Input.GetActionHandle("/actions/main/in/leftfingerpinky", ref leftControllerActions[7]);

        OpenVR.Input.GetActionHandle("/actions/main/in/righta", ref rightControllerActions[0]);
        OpenVR.Input.GetActionHandle("/actions/main/in/rightb", ref rightControllerActions[1]);
        OpenVR.Input.GetActionHandle("/actions/main/in/rightpad", ref rightControllerActions[2]);
        OpenVR.Input.GetActionHandle("/actions/main/in/rightstick", ref rightControllerActions[3]);
        OpenVR.Input.GetActionHandle("/actions/main/in/rightfingerindex", ref rightControllerActions[4]);
        OpenVR.Input.GetActionHandle("/actions/main/in/rightfingermiddle", ref rightControllerActions[5]);
        OpenVR.Input.GetActionHandle("/actions/main/in/rightfingerring", ref rightControllerActions[6]);
        OpenVR.Input.GetActionHandle("/actions/main/in/rightfingerpinky", ref rightControllerActions[7]);

        OpenVR.Input.GetActionHandle("/actions/haptic/out/head", ref hapticActions[0]);
        OpenVR.Input.GetActionHandle("/actions/haptic/out/chest", ref hapticActions[1]);
        OpenVR.Input.GetActionHandle("/actions/haptic/out/waist", ref hapticActions[2]);
        OpenVR.Input.GetActionHandle("/actions/haptic/out/lefthand", ref hapticActions[3]);
        OpenVR.Input.GetActionHandle("/actions/haptic/out/righthand", ref hapticActions[4]);
        OpenVR.Input.GetActionHandle("/actions/haptic/out/leftelbow", ref hapticActions[5]);
        OpenVR.Input.GetActionHandle("/actions/haptic/out/rightelbow", ref hapticActions[6]);
        OpenVR.Input.GetActionHandle("/actions/haptic/out/leftfoot", ref hapticActions[7]);
        OpenVR.Input.GetActionHandle("/actions/haptic/out/rightfood", ref hapticActions[8]);
        OpenVR.Input.GetActionHandle("/actions/haptic/out/leftknee", ref hapticActions[9]);
        OpenVR.Input.GetActionHandle("/actions/haptic/out/rightknee", ref hapticActions[10]);

        OpenVR.Input.GetActionSetHandle("/actions/main", ref mainActionSetHandle);
        OpenVR.Input.GetActionSetHandle("/actions/haptic", ref hapticActionSetHandle);
    }

    internal void Update()
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
                ulRestrictedToDevice = OpenVR.k_ulInvalidInputValueHandle,
                nPriority = 0
            },
            new()
            {
                ulActionSet = hapticActionSetHandle,
                ulRestrictedToDevice = OpenVR.k_ulInvalidInputValueHandle,
                nPriority = 0
            }
        };
        OpenVR.Input.UpdateActionState(activeActionSet, vractiveactonset_t_size);
    }

    private void updateDevices()
    {
        var leftControllerState = ((Controller?)OVRDeviceManager.GetTrackedDevice(DeviceRole.LeftHand))?.Input ?? new InputStates();

        leftControllerState.A.Touched = OVRHelper.GetDigitalInput(leftControllerActions[0]).bState;
        leftControllerState.B.Touched = OVRHelper.GetDigitalInput(leftControllerActions[1]).bState;
        leftControllerState.PadTouched = OVRHelper.GetDigitalInput(leftControllerActions[2]).bState;
        leftControllerState.StickTouched = OVRHelper.GetDigitalInput(leftControllerActions[3]).bState;
        leftControllerState.IndexFinger = OVRHelper.GetAnalogueInput(leftControllerActions[4]).x;
        leftControllerState.MiddleFinger = OVRHelper.GetAnalogueInput(leftControllerActions[5]).x;
        leftControllerState.RingFinger = OVRHelper.GetAnalogueInput(leftControllerActions[6]).x;
        leftControllerState.PinkyFinger = OVRHelper.GetAnalogueInput(leftControllerActions[7]).x;

        var rightControllerState = ((Controller?)OVRDeviceManager.GetTrackedDevice(DeviceRole.RightHand))?.Input ?? new InputStates();

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
