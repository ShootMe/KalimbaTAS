# KalimbaTAS
Tool Assisted Modification for Kalimba

Open Kalimba in your Steam directory (usually C:\Program Files (x86)\Steam\steamapps\common\Kalimba\)

### Setup
	Copy Assembly-CSharp.dll and KalimbaTAS.dll to (\Kalimba_Data\Managed\).
	
	Make sure to backup the original Assembly-CSharp.dll before hand.

If you are going to playback a TAS file that is already created, copy the file to the steam directory listed at the top and make sure it is named 'Kalimba.tas'
Otherwise the program will record a new file there in it's place.

### Controls
1. To playback already recorded TAS
	* Left Trigger + Right Trigger + Right Stick Button
2. To record completely new TAS
	* Left Trigger + Right Trigger + Left Stick Button
3. To stop playback/recording
	* Left Trigger + Right Trigger + DPad Down
4. To move to the next checkpoint
	* Left Trigger + Right Trigger + DPad Right
5. To move to the previous checkpoint
	* Left Trigger + Right Trigger + DPad Left
6. While playing back:
	* To frame step forward one frame - DPad Up
	* While frame stepping hold Right Analog Stick to the right to frame step continuously
	* To continue playback at normal speed from frame stepping - DPad Down
	* When not frame stepping move Right Analog Stick to slowdown or speedup playback

###File Format
000|J/S/X/.|L/R/.(|1/2)

Examples:

  1|J|R   (For 1 frame, hold Jump and Right)
  
  1|X|L|2 (Coop player 2 will for 1 frame, hold Jump, Switch, and Left)
  
100|.|R   (For 100 frames, hold Right)

 20|.|.   (For 20 frames, hold nothing)

###Things to try and avoid while TAS'ing
1. Trampolines can give the TAS troubles. So try and not fast forward through them as the game will probably desync (but not always).
2. Try not to jump off a ledge on the 3rd frame. This seems to be RNG dependant and will not always work in every playback.
3. Try not to buffer jumps from under a ledge. This also seems to be RNG and the totems will end up jumping at different frames on playback to playback.
4. There can be times when getting pickups are RNG if you barely clip them. Just try it multiple times to make sure it will always work.

###Examples
You can find all of the current files I have completed in the Examples folder.
