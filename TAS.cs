using System;
using System.Threading;
using UnityEngine;
namespace KalimbaTAS {
    [Flags]
    public enum TASState {
        None = 0,
        Enable = 1,
        Record = 2,
        Reload = 4,
        FrameStep = 8,
        CheckpointNext = 16,
        CheckpointPrevious = 32,
        Disable = 64
    }
    public class TAS {
        private static TASState tasStateNext, tasState;
        private static string filePath = "Kalimba.tas";
        private static InputController player1 = new InputController(1, filePath);
        private static InputController player2 = new InputController(2, filePath);
        private const float deltaTime = 1f / 60f;
        private static float triggerThreshholdRelease = 0.1f, triggerThreshholdPressed = 0.7f;
        public static int frameRate = 0;
        private static GUIStyle style;
        private static PlatformManagerImplementation.Player p1, p2;
        public static bool isRunning = false;
        public static string TASOutput;
        public static bool showOutput = true;
        private static bool lastBack = false, shouldUpdateInfo;
        private static Vector2 lastPos1, lastPos2, lastPos3, lastPos4;
        public static float TotemTime;

        static TAS() {
            NGUIDebug.Log("");
            long temp = 0x56AF3C93E17D2F8B;
            TASOutput = temp.ToString("X");
        }
        public static void UpdateTAS(BaseController controller) {
            if (p1 == null || p2 == null) {
                PlatformManagerImplementation imp = PlatformManager.Instance.imp;
                p1 = imp.players[0];
                p2 = imp.players[1];
            }

            if (controller is SteamController && !(controller is SteamKeyboardController) && ((SteamController)controller).controllerIndex == 0) {
                TotemGamePadPlugin.UpdateGamepads();
                TotemGamePadPlugin.GamepadState gamepad;
                TotemGamePadPlugin.GetGamepadState(0, out gamepad);

                HandleFrameRates(gamepad);
                CheckControls(gamepad);
                FrameStepping(gamepad);
            }

            if (p1.gameController != controller && p2.gameController != controller) {
                return;
            }

            if (HasFlag(tasState, TASState.Enable)) {
                isRunning = true;
                if (HasFlag(tasState, TASState.Record)) {
                    if (p1.gameController == controller) {
                        player1.RecordPlayer(player2, controller);
                    } else {
                        player2.RecordPlayer(player1, controller);
                    }
                } else {
                    if (p1.gameController == controller) {
                        player1.PlaybackPlayer(controller);
                    } else {
                        player2.PlaybackPlayer(controller);
                    }

                    if (player1.CurrentInput != null && player1.CurrentInput.HasActions(Actions.Time)) {
                        TotemTime = 0;
                    }

                    if (player1.FastForwarding) {
                        SetFrameRate(6000);
                    } else if (player1.EndFastForwarding) {
                        tasState |= TASState.FrameStep;
                        SetFrameRate(60);
                    }

                    if (!player1.CanPlayback && !player2.CanPlayback) {
                        DisableRun();
                    }
                }
            } else {
                if (controller is SteamKeyboardController) {
                    TotemGamePadPlugin.UpdateGamepads();
                    TotemGamePadPlugin.GamepadState gamepad;
                    TotemGamePadPlugin.GetGamepadState(0, out gamepad);

                    InputRecord input = new InputRecord();
                    input.Frames = 1;
                    input.Line = 1;
                    input.Player = 1;
                    bool backPressed = gamepad.IsBPressed || gamepad.IsMenuPressed;
                    if (backPressed && !lastBack) {
                        lastBack = true;
                        input.Actions |= Actions.Back;
                    } else if (!backPressed) {
                        lastBack = false;
                    }

                    input.Actions |= gamepad.IsDPadUpPressed ? Actions.Up : Actions.None;
                    input.Actions |= gamepad.IsDPadDownPressed ? Actions.Down : Actions.None;
                    input.Actions |= gamepad.IsXPressed ? Actions.Swap : Actions.None;

                    if (p1.gameController == controller) {
                        input.Actions |= (gamepad.IsLeftShoulderPressed || gamepad.IsAPressed) ? Actions.Jump : Actions.None;
                        input.Actions |= (gamepad.IsDPadLeftPressed || gamepad.LeftThumbstickX <= -0.5f) ? Actions.Left : Actions.None;
                        input.Actions |= (gamepad.IsDPadRightPressed || gamepad.LeftThumbstickX >= 0.5f) ? Actions.Right : Actions.None;
                    } else {
                        input.Actions |= gamepad.IsRightShoulderPressed ? Actions.Jump : Actions.None;
                        input.Actions |= gamepad.RightThumbstickX <= -0.5f ? Actions.Left : Actions.None;
                        input.Actions |= gamepad.RightThumbstickX >= 0.5f ? Actions.Right : Actions.None;
                    }

                    input.UpdateInput(controller);
                }
                isRunning = false;
                TASOutput = null;
            }

            TotemTime += deltaTime * Time.timeScale;
            try {
                if (GlobalGameManager.Instance != null && GlobalGameManager.Instance.currentSession != null && GlobalGameManager.Instance.currentSession.activeSessionHolder != null) {
                    GameManager gm = GlobalGameManager.Instance.currentSession.activeSessionHolder.gameManager;
                    Controller c = gm.controllers[0];
                    lastPos1 = c.controlledPlayers[0].transform.position;
                    lastPos2 = c.controlledPlayers[1].transform.position;
                    if (gm.controllers.Length > 1) {
                        c = gm.controllers[1];
                        lastPos3 = c.controlledPlayers[0].transform.position;
                        lastPos4 = c.controlledPlayers[1].transform.position;
                    }
                } else {
                    lastPos1 = Vector2.zero;
                    lastPos2 = Vector2.zero;
                    lastPos3 = Vector2.zero;
                    lastPos4 = Vector2.zero;
                }
            } catch {
            }
            shouldUpdateInfo = true;
        }
        private static void HandleFrameRates(TotemGamePadPlugin.GamepadState gamepad) {
            if (HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.FrameStep) && !HasFlag(tasState, TASState.Record)) {
                if (gamepad.RightThumbstickX <= 0.3) {
                    SetFrameRate();
                } else if (gamepad.RightThumbstickX <= 0.4) {
                    SetFrameRate(120);
                } else if (gamepad.RightThumbstickX <= 0.5) {
                    SetFrameRate(150);
                } else if (gamepad.RightThumbstickX <= 0.6) {
                    SetFrameRate(180);
                } else if (gamepad.RightThumbstickX <= 0.7) {
                    SetFrameRate(210);
                } else if (gamepad.RightThumbstickX <= 0.8) {
                    SetFrameRate(240);
                } else if (gamepad.RightThumbstickX <= 0.9) {
                    SetFrameRate(300);
                } else {
                    SetFrameRate(6000);
                }
            } else {
                SetFrameRate();
            }
        }
        private static void SetFrameRate(int newFrameRate = 60) {
            if (GlobalGameManager.Instance != null && GlobalGameManager.Instance.currentSession != null && GlobalGameManager.Instance.currentSession.activeSessionHolder != null && GlobalGameManager.Instance.currentSession.activeSessionHolder.cameraController != null) {
                if (newFrameRate > 300) {
                    GlobalGameManager.Instance.currentSession.activeSessionHolder.gameManager.cameraController.gameplayCamera.enabled = false;
                } else {
                    GlobalGameManager.Instance.currentSession.activeSessionHolder.gameManager.cameraController.gameplayCamera.enabled = true;
                }
            }
            if (frameRate == newFrameRate) { return; }

            frameRate = newFrameRate;
            UnityEngine.Time.captureFramerate = 60;
            Application.targetFrameRate = frameRate;
            UnityEngine.Time.fixedDeltaTime = deltaTime;
            UnityEngine.Time.maximumDeltaTime = deltaTime;
            QualitySettings.vSyncCount = 0;// newFrameRate == 60 ? 1 : 0;
        }
        private static void FrameStepping(TotemGamePadPlugin.GamepadState gamepad) {
            if (HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.Record) && (HasFlag(tasState, TASState.FrameStep) || (gamepad.IsDPadUpPressed && gamepad.LeftTrigger <= triggerThreshholdRelease && gamepad.RightTrigger <= triggerThreshholdRelease))) {
                bool ap = gamepad.IsDPadUpPressed;
                while (HasFlag(tasState, TASState.Enable)) {
                    TotemGamePadPlugin.UpdateGamepads();
                    TotemGamePadPlugin.GetGamepadState(0, out gamepad);

                    CheckControls(gamepad);
                    bool triggerReleased = gamepad.LeftTrigger <= triggerThreshholdRelease && gamepad.RightTrigger <= triggerThreshholdRelease;
                    if (!ap && gamepad.IsDPadUpPressed && triggerReleased) {
                        tasState |= TASState.FrameStep;
                        break;
                    } else if (gamepad.IsDPadDownPressed && triggerReleased) {
                        tasState &= ~TASState.FrameStep;
                        break;
                    } else if (gamepad.RightThumbstickX >= 0.2) {
                        tasState |= TASState.FrameStep;
                        int sleepTime = 0;
                        if (gamepad.RightThumbstickX <= 0.3) {
                            sleepTime = 200;
                        } else if (gamepad.RightThumbstickX <= 0.4) {
                            sleepTime = 100;
                        } else if (gamepad.RightThumbstickX <= 0.5) {
                            sleepTime = 80;
                        } else if (gamepad.RightThumbstickX <= 0.6) {
                            sleepTime = 64;
                        } else if (gamepad.RightThumbstickX <= 0.7) {
                            sleepTime = 48;
                        } else if (gamepad.RightThumbstickX <= 0.8) {
                            sleepTime = 32;
                        } else if (gamepad.RightThumbstickX <= 0.9) {
                            sleepTime = 16;
                        }
                        Thread.Sleep(sleepTime);
                        break;
                    }
                    ap = gamepad.IsDPadUpPressed;
                    Thread.Sleep(1);
                }
                ReloadRun();
            }
        }
        private static void DisableRun() {
            tasStateNext &= ~TASState.Disable;
            tasState &= ~TASState.Enable;
            tasState &= ~TASState.FrameStep;
            tasState &= ~TASState.Record;
            SetFrameRate(60);
        }
        private static void CheckControls(TotemGamePadPlugin.GamepadState gamepad) {
            if (!gamepad.IsDPadLeftPressed && HasFlag(tasStateNext, TASState.CheckpointPrevious)) {
                tasStateNext &= ~TASState.CheckpointPrevious;
                SelectCheckPoint(-1);
            } else if (!gamepad.IsDPadRightPressed && HasFlag(tasStateNext, TASState.CheckpointNext)) {
                tasStateNext &= ~TASState.CheckpointNext;
                SelectCheckPoint(1);
            }

            if (gamepad.RightTrigger >= triggerThreshholdPressed && gamepad.LeftTrigger >= triggerThreshholdPressed) {
                if (!HasFlag(tasState, TASState.Record) && !HasFlag(tasState, TASState.Enable) && gamepad.IsDPadLeftPressed) {
                    tasStateNext |= TASState.CheckpointPrevious;
                } else if (!HasFlag(tasState, TASState.Record) && !HasFlag(tasState, TASState.Enable) && gamepad.IsDPadRightPressed) {
                    tasStateNext |= TASState.CheckpointNext;
                } else if (!HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.Record) && gamepad.IsLeftThumbstickPressed) {
                    tasStateNext |= TASState.Record;
                }
            }

            if (!HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.Record) && gamepad.IsRightThumbstickPressed) {
                tasStateNext |= TASState.Enable;
            } else if ((HasFlag(tasState, TASState.Enable) || !HasFlag(tasState, TASState.Record)) && (gamepad.IsRightThumbstickPressed || gamepad.IsLeftThumbstickPressed)) {
                tasStateNext |= TASState.Disable;
            }

            if (!gamepad.IsRightThumbstickPressed && !gamepad.IsLeftThumbstickPressed) {
                if (HasFlag(tasStateNext, TASState.Enable)) {
                    EnableRun();
                } else if (HasFlag(tasStateNext, TASState.Record)) {
                    RecordRun();
                } else if (HasFlag(tasStateNext, TASState.Disable)) {
                    DisableRun();
                }
            }
        }
        private static void SelectCheckPoint(int direction) {
            GameManager gm = GlobalGameManager.Instance.currentSession.activeSessionHolder.gameManager;
            int num = gm.checkPointManager.checkPoints.Length;
            int num2 = (gm.controllers[0].currentCheckpoint + direction + num) % num;
            GlobalGameManager.Instance.RestartLevel();
            gm = GlobalGameManager.Instance.currentSession.activeSessionHolder.gameManager;
            gm.cameraController.ReturnToCheckpoint(gm.checkPointManager.GetCheckPoint(num2), true);
            gm.sessionHolder.checkpointManager.SetCheckpoint(num2);
        }
        private static void EnableRun() {
            tasStateNext &= ~TASState.Enable;

            UpdateVariables(false);
        }
        private static void RecordRun() {
            tasStateNext &= ~TASState.Record;

            UpdateVariables(true);
        }
        private static void UpdateVariables(bool recording) {
            tasState |= TASState.Enable;
            tasState &= ~TASState.FrameStep;
            if (recording) {
                tasState |= TASState.Record;
                player1.InitializeRecording();
                player2.InitializeRecording();
            } else {
                tasState &= ~TASState.Record;
                player1.InitializePlayback();
                player2.InitializePlayback();
            }
        }
        private static void ReloadRun() {
            tasStateNext &= ~TASState.Reload;

            player1.ReloadPlayback();
            player2.ReloadPlayback();
        }
        private static bool HasFlag(TASState state, TASState flag) {
            return (state & flag) == flag;
        }
        public static void UpdateText() {
            if (!HasFlag(tasState, TASState.Enable)) { return; }

            string msg = player1.ToString() + (player2.CurrentFrame != 0 ? " " + player2.ToString() : "");
            string next = player1.NextInput() + (player2.CurrentFrame != 0 ? " " + player2.NextInput() : "");
            if (next.Trim() != string.Empty) {
                msg += " " + next;
            }

            try {
                if (GlobalGameManager.Instance != null && GlobalGameManager.Instance.currentSession != null && GlobalGameManager.Instance.currentSession.activeSessionHolder != null) {
                    GameManager gm = GlobalGameManager.Instance.currentSession.activeSessionHolder.gameManager;
                    Controller c = gm.controllers[0];
                    bool t1CJ = c.controlledPlayers[0].CanJump();
                    bool t2CJ = c.controlledPlayers[1].CanJump();
                    Vector2 pos = c.controlledPlayers[0].transform.position;
                    Vector2 p1V = (pos - lastPos1) * 60;
                    pos = c.controlledPlayers[1].transform.position;
                    Vector2 p2V = (pos - lastPos2) * 60;
                    bool loading = GlobalGameManager.Instance.levelIsLoading;
                    if (gm.controllers.Length > 1) {
                        c = gm.controllers[1];
                        bool t3CJ = c.controlledPlayers[0].CanJump();
                        bool t4CJ = c.controlledPlayers[1].CanJump();
                        pos = c.controlledPlayers[0].transform.position;
                        Vector2 p3V = (pos - lastPos3) * 60;
                        pos = c.controlledPlayers[1].transform.position;
                        Vector2 p4V = (pos - lastPos4) * 60;
                        msg += "\r\nT1: (" + p1V.x.ToString("0.00") + ", " + p1V.y.ToString("0.00") + ", " + (t1CJ ? "T" : "F") + ") T2: (" + p2V.x.ToString("0.00") + ", " + p2V.y.ToString("0.00") + ", " + (t2CJ ? "T" : "F") + ") T3: (" + p3V.x.ToString("0.00") + ", " + p3V.y.ToString("0.00") + ", " + (t3CJ ? "T" : "F") + ") T4: (" + p4V.x.ToString("0.00") + ", " + p4V.y.ToString("0.00") + ", " + (t4CJ ? "T" : "F") + ")" + (loading ? " (Loading)" : "");
                    } else {
                        msg += "\r\nT1: (" + p1V.x.ToString("0.00") + ", " + p1V.y.ToString("0.00") + ", " + (t1CJ ? "T" : "F") + ") T2: (" + p2V.x.ToString("0.00") + ", " + p2V.y.ToString("0.00") + ", " + (t2CJ ? "T" : "F") + ")" + (loading ? " (Loading)" : "");
                    }
                }
            } catch {
            }

            TASOutput = msg;
        }
        public static void DrawText() {
            if (style == null) {
                style = GUI.skin.GetStyle("TopBar");
                style.fontStyle = FontStyle.Bold;
                style.font = TotemUISettings.Instance.defaultUIFont;
                style.alignment = TextAnchor.UpperLeft;
                style.normal.textColor = Color.white;
            }
            if (HasFlag(tasState, TASState.Enable)) {
                style.fontSize = (int)Mathf.Round(22f * AspectUtility.screenWidth / 1920f);

                if (shouldUpdateInfo) {
                    UpdateText();
                    shouldUpdateInfo = false;
                }

                if (showOutput) {
                    GUI.Label(new Rect(5f, 2f, AspectUtility.screenWidth - 5f, 60f), TASOutput, style);
                }
            }
        }
    }
}