namespace KalimbaTAS {
	public class TASInput {
		public int Frames { get; set; }
		public int Player { get; set; }
		public bool Jump { get; set; }
		public bool Swap { get; set; }
		public bool Left { get; set; }
		public bool Right { get; set; }

		public TASInput() { }
		public TASInput(int frames, int player, TotemGamePadPlugin.GamepadState gamepad) {
			this.Frames = frames;
			this.Jump = gamepad.IsAPressed;
			this.Swap = gamepad.IsXPressed;
			this.Left = gamepad.IsDPadLeftPressed || gamepad.LeftThumbstickX <= -0.5f;
			this.Right = gamepad.IsDPadRightPressed || gamepad.LeftThumbstickX >= 0.5f;
		}
		public TASInput(string line) {
			try {
				string[] parameters = line.Split('|');
				if (parameters.Length == 5 || parameters.Length == 6) {
					this.Frames = int.Parse(parameters[0]);
					this.Jump = parameters[1] != ".";
					this.Swap = parameters[2] != ".";
					this.Left = parameters[3] != ".";
					this.Right = parameters[4] != ".";
					this.Player = parameters.Length == 5 || parameters[5] == "1" ? 1 : 2;
				}
			} catch { }
		}
		public void UpdateInput(ref TotemGamePadPlugin.GamepadState gamepad) {
			gamepad.IsAPressed = Jump;
			gamepad.IsXPressed = Swap;
			gamepad.IsDPadLeftPressed = Left;
			gamepad.IsDPadRightPressed = Right;
			gamepad.LeftThumbstickX = 0;
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
			return Frames.ToString().PadLeft(4, ' ') + "|" + (Jump ? "J" : ".") + "|" + (Swap ? "S" : ".") + "|" + (Left ? "L" : ".") + "|" + (Right ? "R" : ".") + "|" + Player.ToString();
		}
		public string ToStringMono() {
			return "P" + Player.ToString() + " " + Frames.ToString("0000") + "|" + (Jump ? "J" : "0") + "|" + (Swap ? "S" : "0") + "|" + (Left ? "L" : "0") + "|" + (Right ? "R" : "0");
		}
		public override bool Equals(object obj) {
			return base.Equals(obj);
		}
		public override int GetHashCode() {
			return Frames;
		}
	}
}