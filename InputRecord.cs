using System;
using System.Collections.Generic;
namespace KalimbaTAS {
	[Flags]
	public enum Actions {
		None,
		L = 1,
		R = 2,
		J = 4,
		S = 8,
		B = 16,
		U = 32,
		D = 64
	}
	public class TASInput {
		public int Frames { get; set; }
		public int Player { get; set; }
		public bool Jump { get; set; }
		public bool Swap { get; set; }
		public bool Left { get; set; }
		public bool Right { get; set; }
		public bool Up { get; set; }
		public bool Down { get; set; }
		public bool Back { get; set; }
		public int Line { get; set; }
		public Actions Actions { get; set; }

		public TASInput() { }
		public TASInput(int frames, int player, int inputCount, BaseController controller) {
			Player = player;
			Frames = frames;
			Jump = controller.aButton.next || (controller is SteamKeyboardController ? ((Dictionary<string, EdgeDetectingBoolWrapper>)((SteamKeyboardController)controller).ButtonDict())["enter"].next : false);
			Swap = controller.xButton.next;
			Left = controller.dpadLeftButton.next || controller.leftStickX <= -0.5f;
			Right = controller.dpadRightButton.next || controller.leftStickX >= 0.5f;
			Back = controller.bButton.next || controller.startButton.next;
			Up = controller.dpadUpButton.next || controller.leftStickY >= 0.5f;
			Down = controller.dpadDownButton.next || controller.leftStickY <= -0.5f;
			Line = inputCount;
			UpdateActions();
		}
		public TASInput(string line, int lineNum) {
			string[] parameters = line.Split('|', ',');

			int frames = 0;
			int.TryParse(parameters[0], out frames);
			Frames = frames;
			Line = lineNum;
			Player = 1;

			for (int i = 1; i < parameters.Length; i++) {
				switch (parameters[i].Trim().ToUpper()) {
					case "L": Left = true; break;
					case "R": Right = true; break;
					case "J": Jump = true; break;
					case "S": Swap = true; break;
					case "X": Jump = true; Swap = true; break;
					case "B": Back = true; break;
					case "U": Up = true; break;
					case "D": Down = true; break;
					case "1": Player = 1; break;
					case "2": Player = 2; break;
				}
			}
			UpdateActions();
		}
		public void UpdateActions() {
			Actions = (Left ? Actions.L : Actions.None) | (Right ? Actions.R : Actions.None) | (Jump ? Actions.J : Actions.None)
				| (Swap ? Actions.S : Actions.None) | (Back ? Actions.B : Actions.None) | (Up ? Actions.U : Actions.None)
				| (Down ? Actions.D : Actions.None);
		}
		public void UpdateInput(BaseController controller) {
			controller.StartUpdate();

			controller.dpadLeftButton.StartUpdateSet(Left);
			controller.dpadRightButton.StartUpdateSet(Right);
			controller.aButton.StartUpdateSet(Jump);
			controller.xButton.StartUpdateSet(Swap);
			controller.bButton.StartUpdateSet(Back);
			controller.startButton.StartUpdateSet(Back);
			controller.dpadDownButton.StartUpdateSet(Down);
			controller.dpadUpButton.StartUpdateSet(Up);
			controller.leftStickX = 0;
			controller.leftStickY = 0;
			if (controller is SteamKeyboardController) {
				Dictionary<string, EdgeDetectingBoolWrapper> buttons = (Dictionary<string, EdgeDetectingBoolWrapper>)((SteamKeyboardController)controller).ButtonDict();

				buttons["space"].StartUpdateSet(Jump);
				buttons["enter"].StartUpdateSet(Jump);
				buttons["jump"].StartUpdateSet(Jump);

				buttons["w"].StartUpdateSet(Up);
				buttons["up"].StartUpdateSet(Up);

				buttons["s"].StartUpdateSet(Down);
				buttons["down"].StartUpdateSet(Down);

				buttons["a"].StartUpdateSet(Left);
				buttons["left"].StartUpdateSet(Left);

				buttons["d"].StartUpdateSet(Right);
				buttons["right"].StartUpdateSet(Right);

				buttons["lShift"].StartUpdateSet(Swap);
				buttons["rShift"].StartUpdateSet(Swap);
				buttons["lCtrl"].StartUpdateSet(Swap);
				buttons["rCtrl"].StartUpdateSet(Swap);
				buttons["swap"].StartUpdateSet(Swap);

				buttons["esc"].StartUpdateSet(Back);
				buttons["cancel"].StartUpdateSet(Back);
			}
		}
		public static bool operator ==(TASInput one, TASInput two) {
			if ((object)one == null && (object)two != null) {
				return false;
			} else if ((object)one != null && (object)two == null) {
				return false;
			} else if ((object)one == null && (object)two == null) {
				return true;
			}
			return one.Player == two.Player && one.Actions == two.Actions;
		}
		public static bool operator !=(TASInput one, TASInput two) {
			if ((object)one == null && (object)two != null) {
				return true;
			} else if ((object)one != null && (object)two == null) {
				return true;
			} else if ((object)one == null && (object)two == null) {
				return false;
			}
			return one.Player != two.Player || one.Actions != two.Actions;
		}
		public override string ToString() {
			return ToString(false);
		}
		public string ToString(bool singlePlayer) {
			return Frames.ToString().PadLeft(3, ' ') + (Actions != Actions.None ? "," + Actions.ToString().Replace(" ", "") : "") + (singlePlayer ? "" : "," + Player.ToString());
		}
		public string ToStringDisplay() {
			return "P" + Player.ToString() + "-L" + Line.ToString() + "(" + Frames.ToString() + (Actions != Actions.None ? "," + Actions.ToString().Replace(" ", "") : "") + ")";
		}
		public override bool Equals(object obj) {
			return base.Equals(obj);
		}
		public override int GetHashCode() {
			return Frames;
		}
	}
}