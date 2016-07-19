using System.Collections.Generic;
using System.IO;
namespace KalimbaTAS {
	public class TASPlayer {
		public int Player { get; set; }

		private List<TASInput> inputs = new List<TASInput>();
		private TASInput lastInput;
		private int currentFrame, inputIndex, frameToNext;
		private string filePath;

		public TASPlayer(int player, string filePath) {
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
				lastInput = new TASInput() { Player = Player };
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
			lastInput = new TASInput() { Player = Player };
			frameToNext = 0;
			inputs.Clear();
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
		public void RecordPlayer(TASPlayer otherPlayer, BaseController controller) {
			TASInput input = new TASInput(currentFrame, Player, inputIndex + 1, controller);
			if (currentFrame == 0 && otherPlayer.currentFrame == 0 && input == lastInput) {
				return;
			} else if (input != lastInput) {
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

					TASInput input = new TASInput(line, ++lines);
					if (input.Frames != 0 && input.Player == Player) {
						inputs.Add(input);
					}
				}
			}
		}
	}
}