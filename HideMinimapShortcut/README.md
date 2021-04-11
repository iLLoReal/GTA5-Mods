<h1>Hide Minimap Shortcut</h1>
This mod is a request from alerossi82,

it is a simple shortcut key to hide or show the radar.

How To Install
------------------------------

1) Download the file
2) Extract "HideMinimap.cs"
3) Place "HideMinimap.cs" inside a folder named "scripts" in your main GTA V directory.
4) Install ScriptHookVDotNet & ScriptHookV


Changelog
--------------------
v0.2
* It is now possible to activate the "show in vehicle" mod
if you activate it, even if you deactivate the minimap, it will show up inside vehicles.
v0.3
* Added "show while using cellphone" mode (activated by default)
* Added possibility to hide police blips (disabled by default)
v0.3.2
* Fixed radar showing up when gps path recalculated
v0.3.3
* Fixed radar not showing up despite "show in vehicle" mode activated.
* Optimized OnTick method so it should reduce fps cost.
v0.3.4
*Fixed police blips appearing when player lose and regain wanted level.
police blips should now always be hidden when the value of the setting is 'false'

Thinking About
--------------------

* Adding a GUI menu


How To Use
--------------------

Hit the shortcut key (default key is B) to show / hide radar.
Hold the shortcut key to activate / deactivate the "show in vehicle" mode.
Note that even if you activate the "show in vehicle" mode, you still need to hit the shortcut key at least once to hide the minimap.

By default, when you use you're cellphone, the minimap should be displayed.

If you want to disable this (or any other) option, follow the "How To Set Up's Changing mod settings" instructions.

A little feature was added just for the author of the request : hiding police blips. As it is not the original purpose of this mod, it is set to "false" by default.
To enable it, refere to the "Changing mod settings" part of the How To Set Up section.
To activate it in game, just activate the "show in vehicle" mode.

How To Set Up
--------------------

When using this mod, you should have the in-game parameter Display->radar set to "on", or at least to "show blips".
If you set it to "off", the mod won't work until you set it to "on" again.

Replacing the key
The default shortcut key is "B", in order to modify this, you need to :

1) open the HideMinimap.cs file
2) go arround the end of the file
3) Look for "if (e.KeyCode == Keys.B)" and
4) modify the "Keys.B" to "Keys.YourKey:", replacing the "YourKey" part with the desired key, obviously.

For example, if you want to set it to the 'I' key, you would have
"if (e.KeyCode == Keys.I)".

Changing mod settings< /b>

In order to change mod settings, you need to

1) open the HideMinimap.cs file
2) go at the beginning (line 15)
3) Follow //commentary instructions :

In case instructions aren't clear enougth, here's an example :
To activate the "hide police blips when in showInVehicle mode", replace :
"private bool showPoliceBlip = true;"
to
"private bool showPoliceBlip = false";

4) Do not edit a variable if there isn't a comment suggesting that you can.
A comment starts with the "//" sentence.
5) Save and close .cs file
6) You're good to go !

Virus Scans
---------------------

0.1 : https://www.virustotal.com/#/file/282874ce61fc6673ff8f8a704fede68ec439e15809c57fd485a80f262faffccb/detection

0.2 : https://www.virustotal.com/#/file/d1ca788cbe0315c41a2dbe6a0bff499dd219bd26044e6a88ad576c0b76425556/detection

0.3 : https://www.virustotal.com/#/file/3d6361f991501c72022c833d9091c668cc9a0b48dc0b8fe60868e89f4c687214/detection

0.3.2 : https://www.virustotal.com/#/file/5bfade643fa2a65f12431be6929898d791477f271993fb1145d5ef3068e3846f/detection

0.3.3 : https://www.virustotal.com/#/file/7902a713f5ad4111c35a009f6fe7104e5b10b90a66fbc34bced0e9c02a3d4902/detection

0.3.4 : https://www.virustotal.com/#/file/b18b99a9394fab6794446def1ae9929760d372f2191bd9fc270a63379695d137/detection