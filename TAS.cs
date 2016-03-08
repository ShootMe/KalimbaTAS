using System;
using System.Collections.Generic;
using System.IO;
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
		private static List<TASInput> inputs = new List<TASInput>();
		private static TASInput lastInput;
		private static int frame, index, frameTotal;
		public static float deltaTime = 0.016666667f;
		public static int frameRate = 60;
		private static GUIStyle style;
		private static float triggerThreshholdRelease = 0.1f, triggerThreshholdPressed = 0.7f;
		static TAS() {
			NGUIDebug.Log("");
		}
		public static void UpdateTAS(int controllerIndex, ref TotemGamePadPlugin.GamepadState gamepad) {
			if (controllerIndex != 0) { return; }

			UnityEngine.Time.fixedDeltaTime = deltaTime;
			UnityEngine.Time.maximumDeltaTime = deltaTime;
			UnityEngine.Time.captureFramerate = frameRate;
			Application.targetFrameRate = frameRate;
			QualitySettings.vSyncCount = 0;

			CheckControls(ref gamepad);
			FrameStepping(ref gamepad);

			if (HasFlag(tasState, TASState.Enable)) {
				if (HasFlag(tasState, TASState.Record)) {
					RecordNextFrame(controllerIndex, gamepad);
				} else if (index <= inputs.Count) {
					PlayNextFrame(controllerIndex, ref gamepad);
				} else {
					DisableRun();
				}
				frame++;
			}
		}
		private static void RecordNextFrame(int controllerIndex, TotemGamePadPlugin.GamepadState gamepad) {
			TASInput input = new TASInput(frame, controllerIndex + 1, gamepad);
			if (frame == 0 && input == lastInput) {
				return;
			} else if (input != lastInput) {
				lastInput.Frames = frame - lastInput.Frames;
				if (lastInput.Frames != 0) {
					File.AppendAllText(filePath, $"{lastInput}\r\n");
				}
				lastInput = input;
			}
		}
		private static void PlayNextFrame(int controllerIndex, ref TotemGamePadPlugin.GamepadState gamepad) {
			if (frame >= frameTotal) {
				if (index + 1 >= inputs.Count) {
					DisableRun();
					index++;
					return;
				}
				lastInput = inputs[++index];
				frameTotal += lastInput.Frames;
			}
			lastInput.UpdateInput(ref gamepad);
		}
		private static void FrameStepping(ref TotemGamePadPlugin.GamepadState gamepad) {
			if (HasFlag(tasState, TASState.Enable) && (HasFlag(tasState, TASState.FrameStep) || (gamepad.IsDPadUpPressed && gamepad.LeftTrigger <= triggerThreshholdRelease && gamepad.RightTrigger <= triggerThreshholdRelease))) {
				bool ap = gamepad.IsDPadUpPressed;
				while (HasFlag(tasState, TASState.Enable)) {
					TotemGamePadPlugin.UpdateGamepads();
					TotemGamePadPlugin.GamepadState gamepadState;
					TotemGamePadPlugin.GetGamepadState(0, out gamepadState);

					CheckControls(ref gamepadState);
					bool triggerReleased = gamepadState.LeftTrigger <= triggerThreshholdRelease && gamepadState.RightTrigger <= triggerThreshholdRelease;
					if (!ap && gamepadState.IsDPadUpPressed && triggerReleased) {
						tasState |= TASState.FrameStep;
						break;
					} else if (gamepadState.IsDPadDownPressed && triggerReleased) {
						tasState &= ~TASState.FrameStep;
						break;
					} else if (gamepadState.RightThumbstickX >= 0.5) {
						tasState |= TASState.FrameStep;
						Thread.Sleep(33);
						break;
					}
					ap = gamepadState.IsDPadUpPressed;
					Thread.Sleep(1);
				}
			}
		}
		private static void DisableRun() {
			tasState &= ~TASState.Enable;
			tasState &= ~TASState.FrameStep;
			tasState &= ~TASState.Record;
		}
		private static void CheckControls(ref TotemGamePadPlugin.GamepadState gamepad) {
			if (!gamepad.IsDPadLeftPressed && HasFlag(tasStateNext, TASState.CheckpointPrevious)) {
				tasStateNext &= ~TASState.CheckpointPrevious;
				gamepad.IsDPadLeftPressed = false;
				SelectCheckPoint(-1);
			} else if (!gamepad.IsDPadRightPressed && HasFlag(tasStateNext, TASState.CheckpointNext)) {
				tasStateNext &= ~TASState.CheckpointNext;
				gamepad.IsDPadRightPressed = false;
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
					gamepad.IsDPadLeftPressed = false;
				} else if (!HasFlag(tasState, TASState.Record) && !HasFlag(tasState, TASState.Enable) && gamepad.IsDPadRightPressed) {
					tasStateNext |= TASState.CheckpointNext;
					gamepad.IsDPadRightPressed = false;
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
			ReadFile();

			if (inputs.Count == 0) { return; }
			lastInput = inputs[0];
			frameTotal = lastInput.Frames;
		}
		private static void RecordRun() {
			tasStateNext &= ~TASState.Record;

			UpdateVariables(true);
			File.Delete(filePath);
		}
		private static void UpdateVariables(bool recording) {
			tasState |= TASState.Enable;
			tasState &= ~TASState.FrameStep;
			if (recording) {
				tasState |= TASState.Record;
			} else {
				tasState &= ~TASState.Record;
			}
			frame = 0;
			frameTotal = 0;
			index = 0;
			lastInput = new TASInput();
		}
		private static void ReloadRun() {
			tasStateNext &= ~TASState.Reload;

			ReadFile();

			if (inputs.Count == 0) { return; }
			index = 0;
			lastInput = inputs[0];
			frameTotal = lastInput.Frames;

			while (frame >= frameTotal) {
				if (index + 1 >= inputs.Count) {
					index++;
					return;
				}
				lastInput = inputs[++index];
				frameTotal += lastInput.Frames;
			}
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
				string msg = null;
				if (HasFlag(tasState, TASState.Record)) {
					msg = (lastInput != null ? lastInput.ToString() : "") + " (" + frame + ")";
				} else {
					int inputFrames = lastInput.Frames;
					int startFrame = frameTotal - inputFrames;
					msg = lastInput.ToStringMono() + " (" + (frame - startFrame).ToString() + " of " + inputFrames + ", " + frame + ")";
					if (index + 1 < inputs.Count) {
						msg += "\r\n" + inputs[index + 1].ToStringMono();
					}
				}

				GUI.Label(new Rect(5f, 2f, 200f, 50f), msg, style);
			}
		}
		private static void ReadFile() {
			inputs.Clear();
			if (!File.Exists(filePath)) { return; }
			int count = 0;
			using (StreamReader sr = new StreamReader(filePath)) {
				while (!sr.EndOfStream) {
					string line = sr.ReadLine();
					count++;
					TASInput input = new TASInput(line);
					if (input.Frames != 0) {
						inputs.Add(input);
					}
				}
			}
		}
	}
}