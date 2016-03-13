using System.Collections.Generic;

namespace KalimbaTAS {
	public class TASInput {
		public int Frames { get; set; }
		public int Player { get; set; }
		public bool Jump { get; set; }
		public bool Swap { get; set; }
		public bool Left { get; set; }
		public bool Right { get; set; }
		public int Line { get; set; }

		public TASInput() { }
		public TASInput(int frames, int player, int inputCount, BaseController controller) {
			this.Player = player;
			this.Frames = frames;
			this.Jump = controller.aButton.next || (controller is SteamKeyboardController ? ((Dictionary<string, EdgeDetectingBoolWrapper>)((SteamKeyboardController)controller).ButtonDict())["enter"].next : false);
			this.Swap = controller.xButton.next;
			this.Left = controller.dpadLeftButton.next || controller.leftStickX <= -0.5f;
			this.Right = controller.dpadRightButton.next || controller.leftStickX >= 0.5f;
			this.Line = inputCount;
		}
		public TASInput(string line, int lineNum) {
			try {
				string[] parameters = line.Split('|');
				if (parameters.Length == 3 || parameters.Length == 4) {
					this.Frames = int.Parse(parameters[0]);
					this.Jump = parameters[1].IndexOf('J') >= 0 || parameters[1].IndexOf('X') >= 0;
					this.Swap = parameters[1].IndexOf('S') >= 0 || parameters[1].IndexOf('X') >= 0;
					this.Left = parameters[2].IndexOf('L') >= 0;
					this.Right = parameters[2].IndexOf('R') >= 0;
					this.Player = parameters.Length == 3 || parameters[3] == "1" ? 1 : 2;
				} else if (parameters.Length == 5 || parameters.Length == 6) {
					this.Frames = int.Parse(parameters[0]);
					this.Jump = parameters[1] != ".";
					this.Swap = parameters[2] != ".";
					this.Left = parameters[3] != ".";
					this.Right = parameters[4] != ".";
					this.Player = parameters.Length == 5 || parameters[6] == "1" ? 1 : 2;
				}
				this.Line = lineNum;
			} catch { }
		}
		public void UpdateInput(BaseController controller) {
			controller.StartUpdate();

			controller.dpadLeftButton.Or(Left);
			controller.dpadRightButton.Or(Right);
			controller.aButton.Or(Jump);
			controller.xButton.Or(Swap);
			if (Jump && controller is SteamKeyboardController) {
				Dictionary<string, EdgeDetectingBoolWrapper> buttons = (Dictionary<string, EdgeDetectingBoolWrapper>)((SteamKeyboardController)controller).ButtonDict();
				buttons["enter"].Or(Jump);
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
			return one.Player == two.Player && one.Jump == two.Jump && one.Swap == two.Swap && one.Left == two.Left && one.Right == two.Right;
		}
		public static bool operator !=(TASInput one, TASInput two) {
			if ((object)one == null && (object)two != null) {
				return true;
			} else if ((object)one != null && (object)two == null) {
				return true;
			} else if ((object)one == null && (object)two == null) {
				return false;
			}
			return one.Player != two.Player || one.Jump != two.Jump || one.Swap != two.Swap || one.Left != two.Left || one.Right != two.Right;
		}
		public override string ToString() {
			return ToString(false);
		}
		public string ToString(bool singlePlayer) {
			return Frames.ToString().PadLeft(4, ' ') + "|" + (Jump && Swap ? "X" : Jump ? "J" : Swap ? "S" : ".") + "|" + (Left ? "L" : Right ? "R" : ".") + (singlePlayer ? "" : "|" + Player.ToString());
		}
		public string ToStringDisplay() {
			return "P" + Player.ToString() + "-Line" + Line.ToString() + "(" + Frames.ToString() + " | " + (Jump && Swap ? "X" : Jump ? "J" : Swap ? "S" : "0") + " | " + (Left ? "L" : Right ? "R" : "0") + ")";
		}
		public override bool Equals(object obj) {
			return base.Equals(obj);
		}
		public override int GetHashCode() {
			return Frames;
		}
	}
}