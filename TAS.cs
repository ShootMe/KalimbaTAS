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
		CheckpointPrevious = 32
	}
	public class TAS {
		private static TASState tasStateNext, tasState;
		private static string filePath = "Kalimba.tas";
		private static TASPlayer player1 = new TASPlayer(1, filePath);
		private static TASPlayer player2 = new TASPlayer(2, filePath);
		public static float deltaTime = 0.016666667f, timeScale = 1f;
		private static float triggerThreshholdRelease = 0.1f, triggerThreshholdPressed = 0.7f;
		public static int frameRate = 0;
		private static GUIStyle style;
		private static PlatformManagerImplementation.Player p1, p2;

		static TAS() {
			NGUIDebug.Log("");
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

					if (!player1.CanPlayback && !player2.CanPlayback) {
						DisableRun();
					}
				}
			}
		}
		private static void HandleFrameRates(TotemGamePadPlugin.GamepadState gamepad) {
			if (HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.FrameStep) && !HasFlag(tasState, TASState.Record)) {
				if (gamepad.RightThumbstickX <= -0.9) {
					SetFrameRate(20);
				} else if (gamepad.RightThumbstickX <= -0.8) {
					SetFrameRate(25);
				} else if (gamepad.RightThumbstickX <= -0.7) {
					SetFrameRate(30);
				} else if (gamepad.RightThumbstickX <= -0.6) {
					SetFrameRate(35);
				} else if (gamepad.RightThumbstickX <= -0.5) {
					SetFrameRate(40);
				} else if (gamepad.RightThumbstickX <= -0.4) {
					SetFrameRate(45);
				} else if (gamepad.RightThumbstickX <= -0.3) {
					SetFrameRate(50);
				} else if (gamepad.RightThumbstickX <= -0.2) {
					SetFrameRate(55);
				} else if (gamepad.RightThumbstickX <= 0.2) {
					SetFrameRate();
				} else if (gamepad.RightThumbstickX <= 0.3) {
					SetFrameRate(75);
				} else if (gamepad.RightThumbstickX <= 0.4) {
					SetFrameRate(90);
				} else if (gamepad.RightThumbstickX <= 0.5) {
					SetFrameRate(105);
				} else if (gamepad.RightThumbstickX <= 0.6) {
					SetFrameRate(120);
				} else if (gamepad.RightThumbstickX <= 0.7) {
					SetFrameRate(135);
				} else if (gamepad.RightThumbstickX <= 0.8) {
					SetFrameRate(150);
				} else if (gamepad.RightThumbstickX <= 0.9) {
					SetFrameRate(165);
				} else {
					SetFrameRate(180);
				}
			} else {
				SetFrameRate();
			}
		}
		private static void SetFrameRate(int newFrameRate = 60) {
			if (frameRate == newFrameRate) { return; }

			frameRate = newFrameRate;
			timeScale = (float)newFrameRate / 60f;
			UnityEngine.Time.timeScale = timeScale;
			UnityEngine.Time.captureFramerate = frameRate;
			Application.targetFrameRate = frameRate;
			UnityEngine.Time.fixedDeltaTime = deltaTime;
			UnityEngine.Time.maximumDeltaTime = deltaTime;
			QualitySettings.vSyncCount = 0;
		}
		private static void FrameStepping(TotemGamePadPlugin.GamepadState gamepad) {
			if (HasFlag(tasState, TASState.Enable) && (HasFlag(tasState, TASState.FrameStep) || (gamepad.IsDPadUpPressed && gamepad.LeftTrigger <= triggerThreshholdRelease && gamepad.RightTrigger <= triggerThreshholdRelease))) {
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
			}
		}
		private static void DisableRun() {
			tasState &= ~TASState.Enable;
			tasState &= ~TASState.FrameStep;
			tasState &= ~TASState.Record;
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
				if (!HasFlag(tasState, TASState.Enable) && gamepad.IsRightThumbstickPressed) {
					tasStateNext |= TASState.Enable;
				} else if (gamepad.IsDPadDownPressed) {
					DisableRun();
				} else if (!HasFlag(tasState, TASState.Reload) && HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.Record) && gamepad.IsDPadUpPressed) {
					tasStateNext |= TASState.Reload;
				} else if (!HasFlag(tasState, TASState.Record) && gamepad.IsLeftThumbstickPressed) {
					tasStateNext |= TASState.Record;
				} else if (!HasFlag(tasState, TASState.Record) && !HasFlag(tasState, TASState.Enable) && gamepad.IsDPadLeftPressed) {
					tasStateNext |= TASState.CheckpointPrevious;
				} else if (!HasFlag(tasState, TASState.Record) && !HasFlag(tasState, TASState.Enable) && gamepad.IsDPadRightPressed) {
					tasStateNext |= TASState.CheckpointNext;
				}
			}

			if (gamepad.RightTrigger <= triggerThreshholdRelease && gamepad.LeftTrigger <= triggerThreshholdRelease) {
				if (HasFlag(tasStateNext, TASState.Enable)) {
					EnableRun();
				} else if (HasFlag(tasStateNext, TASState.Record)) {
					RecordRun();
				} else if (HasFlag(tasStateNext, TASState.Reload)) {
					ReloadRun();
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
				string msg = player1.ToString() + (player2.CurrentFrame != 0 ? "   " + player2.ToString() : "");
				string next = player1.NextInput() + (player2.CurrentFrame != 0 ? "   " + player2.NextInput() : "");
				if (next.Trim() != string.Empty) {
					msg += "   " + next;
				}

				GameManager gm = GlobalGameManager.Instance.currentSession.activeSessionHolder.gameManager;
				bool t1CJ = gm.controllers[0].controlledPlayers[0].CanJump();
				bool t2CJ = gm.controllers[0].controlledPlayers[1].CanJump();
				Vector3 p1V = gm.controllers[0].controlledPlayers[0].GetVelocity();
				Vector3 p2V = gm.controllers[0].controlledPlayers[1].GetVelocity();
				bool loading = GlobalGameManager.Instance.levelIsLoading;
				if (gm.controllers.Length > 1) {
					Controller c2 = gm.controllers[1];
					bool t3CJ = c2.controlledPlayers[0].CanJump();
					bool t4CJ = c2.controlledPlayers[1].CanJump();
					Vector3 p3V = c2.controlledPlayers[0].GetVelocity();
					Vector3 p4V = c2.controlledPlayers[1].GetVelocity();
					msg += "\r\n(T1: " + p1V.x.ToString("0.00") + "," + p1V.y.ToString("0.00") + " " + (t1CJ ? "T" : "F") + " T2: " + p2V.x.ToString("0.00") + "," + p2V.y.ToString("0.00") + " " + (t2CJ ? "T" : "F") + " T3: " + p3V.x.ToString("0.00") + "," + p3V.y.ToString("0.00") + " " + (t3CJ ? "T" : "F") + " T4: " + p4V.x.ToString("0.00") + "," + p4V.y.ToString("0.00") + " " + (t4CJ ? "T" : "F") + ")" + (loading ? " (Loading)" : "");
				} else {
					msg += "\r\n(T1: " + p1V.x.ToString("0.00") + "," + p1V.y.ToString("0.00") + " " + (t1CJ ? "T" : "F") + " T2: " + p2V.x.ToString("0.00") + "," + p2V.y.ToString("0.00") + " " + (t2CJ ? "T" : "F") + ")" + (loading ? " (Loading)" : "");
				}

				GUI.Label(new Rect(5f, 2f, AspectUtility.screenWidth - 5f, 60f), msg, style);
			}
		}
	}
}