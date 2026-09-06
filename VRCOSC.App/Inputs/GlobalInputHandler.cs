// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using SDL3;
using SharpHook;
using SharpHook.Simulation;
using VRCOSC.App.SDK.Utils;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Inputs;

public class GlobalInputHandler
{
    private EventLoopGlobalHook? _hook;
    private EventSimulator? _eventSimulator;
    private Repeater? _updater;

    private readonly Dictionary<Key, bool> _keyStates = [];
    private readonly Dictionary<MouseButton, bool> _mouseStates = [];
    private readonly Dictionary<uint, Gamepad> _gamepads = [];

    private Vector2 _mousePos = Vector2.Zero;
    private int _mouseWheel;

    internal Task<Result> Start()
    {
        if (!SDL.Init(SDL.InitFlags.Gamepad | SDL.InitFlags.Haptic))
            return Task.FromResult(Result.Error(SDL.GetError()));

        _hook = new EventLoopGlobalHook(useBackgroundThreadForEventLoop: true);
        _hook.KeyPressed += hookOnKeyPressed;
        _hook.KeyReleased += hookOnKeyReleased;
        _hook.MousePressed += hookOnMousePressed;
        _hook.MouseReleased += hookOnMouseReleased;
        _hook.MouseMoved += hookOnMouseMoved;
        _hook.MouseWheel += hookOnMouseWheel;

        _eventSimulator = EventSimulator.Create(AppManager.APP_NAME);

        _hook.RunAsync(useBackgroundThread: true).FireAndForget(onError: e => Logger.Error(e, "Unable to run event hook"));
        _updater = new Repeater(nameof(GlobalInputHandler) + " - " + nameof(Update), Update);
        _updater.Start(TimeSpan.FromMilliseconds(10));
        return Task.FromResult(Result.Success());
    }

    internal Task Update()
    {
        updateSdl();
        updateGamepads();
        return Task.CompletedTask;
    }

    internal async Task Stop()
    {
        if (_updater is not null)
            await _updater.StopAsync();

        if (_hook is not null && !_hook.IsDisposed)
            _hook.Dispose();

        if (_eventSimulator is not null && !_eventSimulator.IsDisposed)
            _eventSimulator.Dispose();

        _hook = null;
        _eventSimulator = null;
    }

    public async Task PressKeybind(Keybind keybind, TimeSpan delay)
    {
        if (_eventSimulator is null) return;

        foreach (var key in keybind.AllKeys)
        {
            _eventSimulator.SimulateKeyPress(key.ToSharpHook());
        }

        await Task.Delay(delay);

        foreach (var key in keybind.AllKeys.Reverse())
        {
            _eventSimulator.SimulateKeyRelease(key.ToSharpHook());
        }
    }

    public void HoldKeybind(Keybind keybind)
    {
        if (_eventSimulator is null) return;

        foreach (var key in keybind.AllKeys)
        {
            _eventSimulator.SimulateKeyPress(key.ToSharpHook());
        }
    }

    public void ReleaseKeybind(Keybind keybind)
    {
        if (_eventSimulator is null) return;

        foreach (var key in keybind.AllKeys)
        {
            _eventSimulator.SimulateKeyRelease(key.ToSharpHook());
        }
    }

    public void SetMousePosition(Vector2 vector, bool isRelative)
    {
        if (_eventSimulator is null) return;

        if (isRelative)
            _eventSimulator.SimulateMouseMovementRelative((short)vector.X, (short)vector.Y);
        else
            _eventSimulator.SimulateMouseMovement((short)vector.X, (short)vector.Y);
    }

    public async Task PressMouseButton(MouseButton button, TimeSpan delay)
    {
        if (_eventSimulator is null) return;

        _eventSimulator.SimulateMousePress(button.ToSharpHook());
        await Task.Delay(delay);
        _eventSimulator.SimulateMouseRelease(button.ToSharpHook());
    }

    public void MouseHoldButton(MouseButton button)
    {
        if (_eventSimulator is null) return;

        _eventSimulator.SimulateMousePress(button.ToSharpHook());
    }

    public void MouseReleaseButton(MouseButton button)
    {
        if (_eventSimulator is null) return;

        _eventSimulator.SimulateMouseRelease(button.ToSharpHook());
    }

    public void MouseScroll(int amount)
    {
        if (_eventSimulator is null) return;

        var value = (short)(amount * 120);
        _eventSimulator.SimulateMouseWheel(value);
    }

    private void updateSdl()
    {
        while (SDL.PollEvent(out var @event))
        {
            var type = (SDL.EventType)@event.Type;

            switch (type)
            {
                case SDL.EventType.GamepadAdded:
                {
                    var gamepadId = @event.GDevice.Which;
                    var gamepadInstanceId = SDL.OpenGamepad(gamepadId);
                    Logger.Log($"Registered gamepad {gamepadId}");
                    _gamepads.Add(gamepadId, new Gamepad(gamepadId, gamepadInstanceId));
                    break;
                }

                case SDL.EventType.GamepadRemapped:
                {
                    var gamepadId = @event.GDevice.Which;
                    SDL.CloseGamepad(_gamepads[gamepadId].InstanceId);

                    var gamepadInstanceId = SDL.OpenGamepad(gamepadId);
                    Logger.Log($"Remapped gamepad {gamepadId}");
                    _gamepads.Add(gamepadId, new Gamepad(gamepadId, gamepadInstanceId));
                    break;
                }

                case SDL.EventType.GamepadRemoved:
                {
                    var gamepadId = @event.GDevice.Which;
                    SDL.CloseGamepad(_gamepads[gamepadId].InstanceId);
                    Logger.Log($"Removed gamepad {gamepadId}");
                    _gamepads.Remove(gamepadId);
                    break;
                }
            }
        }
    }

    private void updateGamepads()
    {
        foreach (var (_, gamepad) in _gamepads)
        {
            var instanceId = gamepad.InstanceId;

            var leftStickX = getGamepadAxis(instanceId, SDL.GamepadAxis.LeftX);
            var leftStickY = getGamepadAxis(instanceId, SDL.GamepadAxis.LeftY);
            var leftStickClick = getGamepadButton(instanceId, SDL.GamepadButton.LeftStick);
            gamepad.LeftStick.Position = new Vector2(leftStickX, leftStickY);
            gamepad.LeftStick.Click = leftStickClick;

            var rightStickX = getGamepadAxis(instanceId, SDL.GamepadAxis.RightX);
            var rightStickY = getGamepadAxis(instanceId, SDL.GamepadAxis.RightY);
            var rightStickClick = getGamepadButton(instanceId, SDL.GamepadButton.RightStick);
            gamepad.RightStick.Position = new Vector2(rightStickX, rightStickY);
            gamepad.RightStick.Click = rightStickClick;

            var leftTrigger = getGamepadAxis(instanceId, SDL.GamepadAxis.LeftTrigger);
            var rightTrigger = getGamepadAxis(instanceId, SDL.GamepadAxis.RightTrigger);
            gamepad.LeftTrigger = leftTrigger;
            gamepad.RightTrigger = rightTrigger;

            var leftShoulder = getGamepadButton(instanceId, SDL.GamepadButton.LeftShoulder);
            var rightShoulder = getGamepadButton(instanceId, SDL.GamepadButton.RightShoulder);
            gamepad.LeftShoulder = leftShoulder;
            gamepad.RightShoulder = rightShoulder;

            var leftPaddle1 = getGamepadButton(instanceId, SDL.GamepadButton.LeftPaddle1);
            var leftPaddle2 = getGamepadButton(instanceId, SDL.GamepadButton.LeftPaddle2);
            var rightPaddle1 = getGamepadButton(instanceId, SDL.GamepadButton.RightPaddle1);
            var rightPaddle2 = getGamepadButton(instanceId, SDL.GamepadButton.RightPaddle2);
            gamepad.LeftPaddle1 = leftPaddle1;
            gamepad.LeftPaddle2 = leftPaddle2;
            gamepad.RightPaddle1 = rightPaddle1;
            gamepad.RightPaddle2 = rightPaddle2;

            var dPadUp = getGamepadButton(instanceId, SDL.GamepadButton.DPadUp);
            var dPadRight = getGamepadButton(instanceId, SDL.GamepadButton.DPadRight);
            var dPadDown = getGamepadButton(instanceId, SDL.GamepadButton.DPadDown);
            var dPadLeft = getGamepadButton(instanceId, SDL.GamepadButton.DPadLeft);
            gamepad.DPadUp = dPadUp;
            gamepad.DPadRight = dPadRight;
            gamepad.DPadDown = dPadDown;
            gamepad.DPadLeft = dPadLeft;

            var start = getGamepadButton(instanceId, SDL.GamepadButton.Start);
            var back = getGamepadButton(instanceId, SDL.GamepadButton.Back);
            var guide = getGamepadButton(instanceId, SDL.GamepadButton.Guide);
            gamepad.Start = start;
            gamepad.Back = back;
            gamepad.Guide = guide;

            var north = getGamepadButton(instanceId, SDL.GamepadButton.North);
            var east = getGamepadButton(instanceId, SDL.GamepadButton.East);
            var south = getGamepadButton(instanceId, SDL.GamepadButton.South);
            var west = getGamepadButton(instanceId, SDL.GamepadButton.West);
            gamepad.North = north;
            gamepad.East = east;
            gamepad.South = south;
            gamepad.West = west;
        }
    }

    private static bool getGamepadButton(nint instanceId, SDL.GamepadButton button) => SDL.GetGamepadButton(instanceId, button);
    private static float getGamepadAxis(nint instanceId, SDL.GamepadAxis axis) => SDL.GetGamepadAxis(instanceId, axis) / (float)short.MaxValue;

    private void hookOnKeyPressed(object? sender, KeyboardHookEventArgs e) => _keyStates[e.Data.KeyCode.ToWpf()] = true;
    private void hookOnKeyReleased(object? sender, KeyboardHookEventArgs e) => _keyStates[e.Data.KeyCode.ToWpf()] = false;
    private void hookOnMousePressed(object? sender, MouseHookEventArgs e) => _mouseStates[e.Data.Button.ToWpf()] = true;
    private void hookOnMouseReleased(object? sender, MouseHookEventArgs e) => _mouseStates[e.Data.Button.ToWpf()] = false;
    private void hookOnMouseMoved(object? sender, MouseHookEventArgs e) => _mousePos = new Vector2(e.Data.X, e.Data.Y);
    private void hookOnMouseWheel(object? sender, MouseWheelHookEventArgs e) => _mouseWheel = (int)(e.Data.Rotation / 360f);

    public bool GetKeyState(Key key) => _keyStates.GetValueOrDefault(key, false);
    public bool GetKeybindState(Keybind keybind) => keybind.AllKeys.Length != 0 && keybind.AllKeys.All(GetKeyState);

    public bool GetMouseState(MouseButton button) => _mouseStates.GetValueOrDefault(button, false);
    public Vector2 GetMousePosition() => _mousePos;

    public int GetMouseWheel()
    {
        var value = _mouseWheel;
        if (value == 0) return 0;

        _mouseWheel = 0;
        return value;
    }

    public Gamepad GetGamepad(uint id) => _gamepads.GetValueOrDefault(id, new Gamepad(0, nint.Zero));
    public void RumbleGamepad(Gamepad gamepad, ushort lowFrequency, ushort highFrequency, uint durationMs) => SDL.RumbleGamepad(gamepad.InstanceId, lowFrequency, highFrequency, durationMs);
}