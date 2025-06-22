// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Runtime.CompilerServices;
using Valve.VR;
using VRCOSC.App.OVR;
using VRCOSC.App.SDK.OVR.Device;

namespace VRCOSC.App.SDK.OVR;

public class OVRInput
{
    private readonly OVRClient client;

    private static readonly uint vractiveactonset_t_size = (uint)Unsafe.SizeOf<VRActiveActionSet_t>();

    private ulong mainActionSetHandle;
    private ulong hapticActionSetHandle;
    private readonly ulong[] leftControllerActions = new ulong[64];
    private readonly ulong[] rightControllerActions = new ulong[64];
    private readonly ulong[] hapticActions = new ulong[64];

    internal OVRInput(OVRClient client)
    {
        this.client = client;
    }

    internal void Init()
    {
        OpenVR.Input.SetActionManifestPath(client.Metadata!.ActionManifest);
        getActionHandles();
    }

    public ulong GetHapticActionHandle(DeviceRole device) => device == DeviceRole.Unset ? OpenVR.k_ulInvalidActionHandle : hapticActions[(int)device];

    private void getActionHandles()
    {
        var i = 0;
        OpenVR.Input.GetActionHandle("/actions/main/in/left_stick_position", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_stick_touch", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_stick_click", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_trigger_pull", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_trigger_touch", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_trigger_click", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_primary_touch", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_primary_click", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_secondary_touch", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_secondary_click", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_grip_pull", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_grip_click", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_pad_position", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_pad_touch", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_pad_click", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_finger_index", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_finger_middle", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_finger_ring", ref leftControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/left_finger_pinky", ref leftControllerActions[i++]);

        i = 0;
        OpenVR.Input.GetActionHandle("/actions/main/in/right_stick_position", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_stick_touch", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_stick_click", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_trigger_pull", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_trigger_touch", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_trigger_click", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_primary_touch", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_primary_click", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_secondary_touch", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_secondary_click", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_grip_pull", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_grip_click", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_pad_position", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_pad_touch", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_pad_click", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_finger_index", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_finger_middle", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_finger_ring", ref rightControllerActions[i++]);
        OpenVR.Input.GetActionHandle("/actions/main/in/right_finger_pinky", ref rightControllerActions[i++]);

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
        var leftController = (Controller?)OVRDeviceManager.GetInstance().GetTrackedDevice(DeviceRole.LeftHand);

        if (leftController is not null)
            leftController.Input = populateInputState(leftControllerActions);

        var rightController = (Controller?)OVRDeviceManager.GetInstance().GetTrackedDevice(DeviceRole.RightHand);

        if (rightController is not null)
            rightController.Input = populateInputState(rightControllerActions);
    }

    private InputState populateInputState(ulong[] actions)
    {
        var i = 0;
        var state = new InputState();

        var stickX = OVRHelper.GetAnalogueInput(actions[i]).x;
        var stickY = OVRHelper.GetAnalogueInput(actions[i++]).y;
        var stickPosition = new Vector2(stickX, stickY);
        var stickTouch = OVRHelper.GetDigitalInput(actions[i++]).bState;
        var stickClick = OVRHelper.GetDigitalInput(actions[i++]).bState;
        state.Stick = new Stick(stickPosition, stickTouch, stickClick);

        var triggerPull = OVRHelper.GetAnalogueInput(actions[i++]).x;
        var triggerTouch = OVRHelper.GetDigitalInput(actions[i++]).bState;
        var triggerClick = OVRHelper.GetDigitalInput(actions[i++]).bState;
        state.Trigger = new Trigger(triggerPull, triggerTouch, triggerClick);

        var primaryTouch = OVRHelper.GetDigitalInput(actions[i++]).bState;
        var primaryClick = OVRHelper.GetDigitalInput(actions[i++]).bState;
        state.Primary = new Button(primaryTouch, primaryClick);

        var secondaryTouch = OVRHelper.GetDigitalInput(actions[i++]).bState;
        var secondaryClick = OVRHelper.GetDigitalInput(actions[i++]).bState;
        state.Secondary = new Button(secondaryTouch, secondaryClick);

        var gripPull = OVRHelper.GetAnalogueInput(actions[i++]).x;
        var gripClick = OVRHelper.GetDigitalInput(actions[i++]).bState;
        state.Grip = new Grip(gripPull, gripClick);

        var padX = OVRHelper.GetAnalogueInput(actions[i]).x;
        var padY = OVRHelper.GetAnalogueInput(actions[i++]).y;
        var padPosition = new Vector2(padX, padY);
        var padTouch = OVRHelper.GetDigitalInput(actions[i++]).bState;
        var padClick = OVRHelper.GetDigitalInput(actions[i++]).bState;
        state.Pad = new Pad(padPosition, padTouch, padClick);

        var index = OVRHelper.GetAnalogueInput(actions[i++]).x;
        var middle = OVRHelper.GetAnalogueInput(actions[i++]).x;
        var ring = OVRHelper.GetAnalogueInput(actions[i++]).x;
        var pinky = OVRHelper.GetAnalogueInput(actions[i++]).x;
        state.Skeleton = new Skeleton(index, middle, ring, pinky);

        return state;
    }
}