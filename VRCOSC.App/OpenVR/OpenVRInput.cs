// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Runtime.CompilerServices;
using Valve.VR;
using VRCOSC.App.OpenVR.Device;

namespace VRCOSC.App.OpenVR;

public class OpenVRInput
{
    private static readonly uint vractiveactonset_t_size = (uint)Unsafe.SizeOf<VRActiveActionSet_t>();

    private readonly ulong[] leftControllerActions = new ulong[64];
    private readonly ulong[] rightControllerActions = new ulong[64];
    private readonly ulong[] hapticActions = new ulong[64];
    private readonly OpenVRManager manager;

    private ulong mainActionSetHandle;
    private ulong hapticActionSetHandle;

    internal OpenVRInput(OpenVRManager manager)
    {
        this.manager = manager;
    }

    internal void Init()
    {
        getActionHandles();
    }

    public ulong GetHapticActionHandle(DeviceRole device) => device == DeviceRole.Unset ? Valve.VR.OpenVR.k_ulInvalidActionHandle : hapticActions[(int)device];

    private void getActionHandles()
    {
        var i = 0;
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_stick_position", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_stick_touch", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_stick_click", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_trigger_pull", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_trigger_touch", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_trigger_click", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_primary_touch", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_primary_click", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_secondary_touch", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_secondary_click", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_system_touch", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_system_click", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_grip_pull", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_grip_click", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_pad_position", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_pad_touch", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_pad_click", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_finger_index", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_finger_middle", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_finger_ring", ref leftControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/left_finger_pinky", ref leftControllerActions[i++]);

        i = 0;
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_stick_position", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_stick_touch", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_stick_click", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_trigger_pull", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_trigger_touch", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_trigger_click", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_primary_touch", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_primary_click", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_secondary_touch", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_secondary_click", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_system_touch", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_system_click", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_grip_pull", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_grip_click", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_pad_position", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_pad_touch", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_pad_click", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_finger_index", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_finger_middle", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_finger_ring", ref rightControllerActions[i++]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/main/in/right_finger_pinky", ref rightControllerActions[i++]);

        Valve.VR.OpenVR.Input.GetActionHandle("/actions/haptic/out/head", ref hapticActions[0]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/haptic/out/chest", ref hapticActions[1]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/haptic/out/waist", ref hapticActions[2]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/haptic/out/lefthand", ref hapticActions[3]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/haptic/out/righthand", ref hapticActions[4]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/haptic/out/leftelbow", ref hapticActions[5]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/haptic/out/rightelbow", ref hapticActions[6]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/haptic/out/leftfoot", ref hapticActions[7]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/haptic/out/rightfoot", ref hapticActions[8]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/haptic/out/leftknee", ref hapticActions[9]);
        Valve.VR.OpenVR.Input.GetActionHandle("/actions/haptic/out/rightknee", ref hapticActions[10]);

        Valve.VR.OpenVR.Input.GetActionSetHandle("/actions/main", ref mainActionSetHandle);
        Valve.VR.OpenVR.Input.GetActionSetHandle("/actions/haptic", ref hapticActionSetHandle);
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
        var leftController = manager.GetLeftController();
        leftController?.Input = populateInputState(leftControllerActions);

        var rightController = manager.GetRightController();
        rightController?.Input = populateInputState(rightControllerActions);
    }

    private InputState populateInputState(ulong[] actions)
    {
        var i = 0;
        var state = new InputState();

        var stickX = OpenVRHelper.GetAnalogueInput(actions[i]).x;
        var stickY = OpenVRHelper.GetAnalogueInput(actions[i++]).y;
        var stickPosition = new Vector2(stickX, stickY);
        var stickTouch = OpenVRHelper.GetDigitalInput(actions[i++]).bState;
        var stickClick = OpenVRHelper.GetDigitalInput(actions[i++]).bState;
        state.Stick = new Stick(stickPosition, stickTouch, stickClick);

        var triggerPull = OpenVRHelper.GetAnalogueInput(actions[i++]).x;
        var triggerTouch = OpenVRHelper.GetDigitalInput(actions[i++]).bState;
        var triggerClick = OpenVRHelper.GetDigitalInput(actions[i++]).bState;
        state.Trigger = new Trigger(triggerPull, triggerTouch, triggerClick);

        var primaryTouch = OpenVRHelper.GetDigitalInput(actions[i++]).bState;
        var primaryClick = OpenVRHelper.GetDigitalInput(actions[i++]).bState;
        state.Primary = new Button(primaryTouch, primaryClick);

        var secondaryTouch = OpenVRHelper.GetDigitalInput(actions[i++]).bState;
        var secondaryClick = OpenVRHelper.GetDigitalInput(actions[i++]).bState;
        state.Secondary = new Button(secondaryTouch, secondaryClick);

        var systemTouch = OpenVRHelper.GetDigitalInput(actions[i++]).bState;
        var systemClick = OpenVRHelper.GetDigitalInput(actions[i++]).bState;
        state.System = new Button(systemTouch, systemClick);

        var gripPull = OpenVRHelper.GetAnalogueInput(actions[i++]).x;
        var gripClick = OpenVRHelper.GetDigitalInput(actions[i++]).bState;
        state.Grip = new Grip(gripPull, gripClick);

        var padX = OpenVRHelper.GetAnalogueInput(actions[i]).x;
        var padY = OpenVRHelper.GetAnalogueInput(actions[i++]).y;
        var padPosition = new Vector2(padX, padY);
        var padTouch = OpenVRHelper.GetDigitalInput(actions[i++]).bState;
        var padClick = OpenVRHelper.GetDigitalInput(actions[i++]).bState;
        state.Pad = new Pad(padPosition, padTouch, padClick);

        var index = OpenVRHelper.GetAnalogueInput(actions[i++]).x;
        var middle = OpenVRHelper.GetAnalogueInput(actions[i++]).x;
        var ring = OpenVRHelper.GetAnalogueInput(actions[i++]).x;
        var pinky = OpenVRHelper.GetAnalogueInput(actions[i++]).x;
        state.Skeleton = new Skeleton(index, middle, ring, pinky);

        return state;
    }
}