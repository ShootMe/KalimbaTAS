using System;
using System.Collections.Generic;
using System.Text;
namespace KalimbaTAS {
	[Flags]
	public enum Actions {
		None,
		Left = 1,
		Right = 2,
		Jump = 4,
		Swap = 8,
		Back = 16,
		Up = 32,
		Down = 64
	}
	public class InputRecord {
		public int Frames { get; set; }
		public int Player { get; set; }
		public int Line { get; set; }
		public Actions Actions { get; set; }

		public InputRecord() { }
		public InputRecord(int frames, int player, int inputCount, BaseController controller) {
			Player = player;
			Frames = frames;
			Actions |= (controller.aButton.next || (controller is SteamKeyboardController ? ((Dictionary<string, EdgeDetectingBoolWrapper>)((SteamKeyboardController)controller).ButtonDict())["enter"].next : false)) ? Actions.Jump : Actions.None;
			Actions |= controller.xButton.next ? Actions.Swap : Actions.None;
			Actions |= controller.dpadLeftButton.next || controller.leftStickX <= -0.5f ? Actions.Left : Actions.None;
			Actions |= controller.dpadRightButton.next || controller.leftStickX >= 0.5f ? Actions.Right : Actions.None;
			Actions |= controller.bButton.next || controller.startButton.next ? Actions.Back : Actions.None;
			Actions |= controller.dpadUpButton.next || controller.leftStickY >= 0.5f ? Actions.Up : Actions.None;
			Actions |= controller.dpadDownButton.next || controller.leftStickY <= -0.5f ? Actions.Down : Actions.None;
			Line = inputCount;
		}
		public InputRecord(string line, int lineNum) {
			Line = lineNum;
			Player = 1;
			int index = 0;
			Frames = ReadFrames(line, ref index);
			if (Frames == 0) { return; }

			while (index < line.Length) {
				char c = line[index];

				switch (char.ToUpper(c)) {
					case 'L': Actions ^= Actions.Left; break;
					case 'R': Actions ^= Actions.Right; break;
					case 'U': Actions ^= Actions.Up; break;
					case 'D': Actions ^= Actions.Down; break;
					case 'J': Actions ^= Actions.Jump; break;
					case 'S': Actions ^= Actions.Swap; break;
					case 'X': Actions ^= Actions.Swap | Actions.Jump; break;
					case 'B': Actions ^= Actions.Back; break;
					case '1': Player = 1; break;
					case '2': Player = 2; break;
				}

				index++;
			}
		}
		private int ReadFrames(string line, ref int start) {
			bool foundFrames = false;
			int frames = 0;

			while (start < line.Length) {
				char c = line[start];

				if (!foundFrames) {
					if (char.IsDigit(c)) {
						foundFrames = true;
						frames = c ^ 0x30;
					} else if (c != ' ') {
						return frames;
					}
				} else if (char.IsDigit(c)) {
					if (frames < 9999) {
						frames = frames * 10 + (c ^ 0x30);
					} else {
						frames = 9999;
					}
				} else if (c != ' ') {
					return frames;
				}

				start++;
			}

			return frames;
		}
		public bool HasActions(Actions actions) {
			return (Actions & actions) != 0;
		}
		public void UpdateInput(BaseController controller) {
			controller.StartUpdate();

			controller.dpadLeftButton.StartUpdateSet(HasActions(Actions.Left));
			controller.dpadRightButton.StartUpdateSet(HasActions(Actions.Right));
			controller.aButton.StartUpdateSet(HasActions(Actions.Jump));
			controller.xButton.StartUpdateSet(HasActions(Actions.Swap));
			controller.bButton.StartUpdateSet(HasActions(Actions.Back));
			controller.startButton.StartUpdateSet(HasActions(Actions.Back));
			controller.dpadDownButton.StartUpdateSet(HasActions(Actions.Down));
			controller.dpadUpButton.StartUpdateSet(HasActions(Actions.Up));
			controller.leftStickX = 0;
			controller.leftStickY = 0;
			if (controller is SteamKeyboardController) {
				Dictionary<string, EdgeDetectingBoolWrapper> buttons = (Dictionary<string, EdgeDetectingBoolWrapper>)((SteamKeyboardController)controller).ButtonDict();

				buttons["space"].StartUpdateSet(HasActions(Actions.Jump));
				buttons["enter"].StartUpdateSet(HasActions(Actions.Jump));
				buttons["jump"].StartUpdateSet(HasActions(Actions.Jump));

				buttons["w"].StartUpdateSet(HasActions(Actions.Up));
				buttons["up"].StartUpdateSet(HasActions(Actions.Up));

				buttons["s"].StartUpdateSet(HasActions(Actions.Down));
				buttons["down"].StartUpdateSet(HasActions(Actions.Down));

				buttons["a"].StartUpdateSet(HasActions(Actions.Left));
				buttons["left"].StartUpdateSet(HasActions(Actions.Left));

				buttons["d"].StartUpdateSet(HasActions(Actions.Right));
				buttons["right"].StartUpdateSet(HasActions(Actions.Right));

				buttons["lShift"].StartUpdateSet(HasActions(Actions.Swap));
				buttons["rShift"].StartUpdateSet(HasActions(Actions.Swap));
				buttons["lCtrl"].StartUpdateSet(HasActions(Actions.Swap));
				buttons["rCtrl"].StartUpdateSet(HasActions(Actions.Swap));
				buttons["swap"].StartUpdateSet(HasActions(Actions.Swap));

				buttons["esc"].StartUpdateSet(HasActions(Actions.Back));
				buttons["cancel"].StartUpdateSet(HasActions(Actions.Back));
			}
		}
		public string ActionsToString() {
			StringBuilder sb = new StringBuilder();
			if (HasActions(Actions.Left)) { sb.Append(",L"); }
			if (HasActions(Actions.Right)) { sb.Append(",R"); }
			if (HasActions(Actions.Up)) { sb.Append(",U"); }
			if (HasActions(Actions.Down)) { sb.Append(",D"); }
			if (HasActions(Actions.Jump)) { sb.Append(",J"); }
			if (HasActions(Actions.Swap)) { sb.Append(",S"); }
			if (HasActions(Actions.Back)) { sb.Append(",B"); }
			return sb.ToString();
		}
		public static bool operator ==(InputRecord one, InputRecord two) {
			bool oneNull = (object)one == null;
			bool twoNull = (object)two == null;
			if (oneNull != twoNull) {
				return false;
			} else if (oneNull && twoNull) {
				return true;
			}
			return one.Player == two.Player && one.Actions == two.Actions;
		}
		public static bool operator !=(InputRecord one, InputRecord two) {
			bool oneNull = (object)one == null;
			bool twoNull = (object)two == null;
			if (oneNull != twoNull) {
				return true;
			} else if (oneNull && twoNull) {
				return false;
			}
			return one.Player != two.Player || one.Actions != two.Actions;
		}
		public override string ToString() {
			return ToString(false);
		}
		public string ToString(bool singlePlayer) {
			return Frames.ToString().PadLeft(3, ' ') + ActionsToString() + (singlePlayer ? "" : "," + Player.ToString());
		}
		public string ToStringDisplay() {
			return "P" + Player.ToString() + "-L" + Line.ToString() + "(" + Frames.ToString() + (Actions != Actions.None ? "," + Actions.ToString().Replace(" ", "") : "") + ")";
		}
		public override bool Equals(object obj) {
			return obj is InputRecord && ((InputRecord)obj) == this;
		}
		public override int GetHashCode() {
			return Frames ^ (int)Actions;
		}
	}
}