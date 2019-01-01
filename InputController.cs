using System.Collections.Generic;
using System.IO;
namespace KalimbaTAS {
	public class InputController {
		public int Player { get; set; }

		private List<InputRecord> inputs = new List<InputRecord>();
		private InputRecord lastInput;
		private int currentFrame, inputIndex, frameToNext;
		private string filePath;

		public InputController(int player, string filePath) {
			this.filePath = filePath;
			this.Player = player;
		}

		public bool CanPlayback { get { return inputIndex < inputs.Count; } }
		public int CurrentFrame { get { return currentFrame; } }
		public override string ToString() {
			if (frameToNext == 0 && lastInput != null) {
				return lastInput.ToStringDisplay() + "(" + currentFrame.ToString() + ")";
			} else if (inputIndex < inputs.Count && lastInput != null) {
				int inputFrames = lastInput.Frames;
				int startFrame = frameToNext - inputFrames;
				return lastInput.ToStringDisplay() + "(" + (currentFrame - startFrame).ToString() + " / " + inputFrames + " : " + currentFrame + ")";
			}
			return string.Empty;
		}
		public string NextInput() {
			if (frameToNext != 0 && inputIndex + 1 < inputs.Count) {
				return inputs[inputIndex + 1].ToStringDisplay();
			}
			return string.Empty;
		}
		public void InitializePlayback() {
			ReadFile();

			currentFrame = 0;
			inputIndex = 0;
			if (inputs.Count > 0) {
				lastInput = inputs[0];
				frameToNext = lastInput.Frames;
			} else {
				lastInput = new InputRecord() { Player = Player };
				frameToNext = 1;
			}
		}
		public void ReloadPlayback() {
			int playedBackFrames = currentFrame;
			InitializePlayback();
			currentFrame = playedBackFrames;

			while (currentFrame >= frameToNext) {
				if (inputIndex + 1 >= inputs.Count) {
					inputIndex++;
					return;
				}
				lastInput = inputs[++inputIndex];
				frameToNext += lastInput.Frames;
			}
		}
		public void InitializeRecording() {
			currentFrame = 0;
			inputIndex = 0;
			lastInput = new InputRecord() { Player = Player };
			frameToNext = 0;
			inputs.Clear();
			if (File.Exists(filePath)) {
				string bakPath = Path.ChangeExtension(filePath, ".bak");
				File.Delete(bakPath);
				File.Move(filePath, bakPath);
			}
			File.Delete(filePath);
		}
		public void PlaybackPlayer(BaseController controller) {
			if (inputIndex < inputs.Count) {
				if (!GlobalGameManager.Instance.levelIsLoading) {
					if (currentFrame >= frameToNext) {
						if (inputIndex + 1 >= inputs.Count) {
							inputIndex++;
							return;
						}
						lastInput = inputs[++inputIndex];
						frameToNext += lastInput.Frames;
					}

					currentFrame++;
				}

				lastInput.UpdateInput(controller);
			}
		}
		public void RecordPlayer(InputController otherPlayer, BaseController controller) {
			InputRecord input = new InputRecord(currentFrame, Player, inputIndex + 1, controller);
			if (currentFrame == 0 && otherPlayer.currentFrame == 0 && input == lastInput) {
				return;
			} else if (input != lastInput && !GlobalGameManager.Instance.levelIsLoading) {
				lastInput.Frames = currentFrame - lastInput.Frames;
				inputIndex++;
				if (lastInput.Frames != 0) {
					File.AppendAllText(filePath, lastInput.ToString(otherPlayer.currentFrame == 0) + "\r\n");
				}
				lastInput = input;
			}
			currentFrame++;
		}
		private void ReadFile() {
			inputs.Clear();
			if (!File.Exists(filePath)) { return; }

			int lines = 0;
			using (StreamReader sr = new StreamReader(filePath)) {
				while (!sr.EndOfStream) {
					string line = sr.ReadLine();

					if (line.IndexOf("Read", System.StringComparison.OrdinalIgnoreCase) == 0 && line.Length > 5) {
						lines++;
						ReadFile(line.Substring(5), lines);
						lines--;
					}

					InputRecord input = new InputRecord(line, ++lines);
					if (input.Frames != 0 && input.Player == Player) {
						inputs.Add(input);
					}
				}
			}
		}
		private void ReadFile(string extraFile, int lines) {
			int index = extraFile.IndexOf(',');
			string filePath = index > 0 ? extraFile.Substring(0, index) : extraFile;
			int skipLines = 0;
			int lineLen = int.MaxValue;
			if (index > 0) {
				int indexLen = extraFile.IndexOf(',', index + 1);
				if (indexLen > 0) {
					int.TryParse(extraFile.Substring(index + 1, indexLen - index - 1), out skipLines);
					int.TryParse(extraFile.Substring(indexLen + 1), out lineLen);
				} else {
					int.TryParse(extraFile.Substring(index + 1), out skipLines);
				}
			}

			if (!File.Exists(filePath)) { return; }

			int subLine = 0;
			using (StreamReader sr = new StreamReader(filePath)) {
				while (!sr.EndOfStream) {
					string line = sr.ReadLine();

					subLine++;
					if (subLine <= skipLines) { continue; }
					if (subLine > lineLen) { break; }

					if (line.IndexOf("Read", System.StringComparison.OrdinalIgnoreCase) == 0 && line.Length > 5) {
						ReadFile(line.Substring(5), lines);
					}

					InputRecord input = new InputRecord(line, lines);
					if (input.Frames != 0 && input.Player == Player) {
						inputs.Add(input);
					}
				}
			}
		}
	}
}