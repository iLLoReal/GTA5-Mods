This mode is a remake of https://www.gta5-mods.com/scripts/hitman-mod-updated

It is thanks to this mode that i was able to start making mods for gta 5, so i owe its creator quite a lot, again, thanks alot to you, PurpleOldMan !

It was originally a mod made by Avees that you can find here https://www.gta5-mods.com/scripts/assassin-hitman-mod

Thanks a lot to you too, Avees !

I've added quite a lot of things to it, at first i wanted PurpleOldMan to add it to his mod, but then he allowed me to create a new mod.

Changelog from PurpleOldMan's 0.8 version
* Player must now lose the police if he is wanted
* Player must leave the "crime scene".
* Target is now randomely generated, at a distance chosen by the player.
* There is a possibility that target will spawn inside a vehicle
* There is a possibility that an escort is following the target
* You are not a pizza delivery boy, in case you wondered. Added Middleman email notification. He will tell you where the target is, and what the target is driving.


Changelog
------------------------------
v1.0.1 :
* Set correct value to let the player know the target will be difficult
v1.1
* Fixed blips not removing after mission completed.
* Added random escort on foot
* Added relationship between target and escorts
* Added more balanced "vehicle/onfoot" target ratio.
* Added more random on-foot target outside of town.
* Added indication regarding the reward. The tips are big !
v1.2
* Remastered the whole script.
* Added iFruit phone contact using iFruitAddon2 by Bob74 (which is a remaster of iFruitAddon, originaly made by CamxxCore)
* Added .ini file
* Hopefully fixed a few crashes with the new class.
* Added better escort handling system.
* It is now possible, thanks to the new class, to customize your own missions ! It still needs a few tweeks here and there, but it should work.


How To Install
------------------------------
1) Download the file
2) Extract "HitmanReloaded.dll"; "iFruitAddon2.dll" and "Hitman.ini"
3) Place them inside a folder named "scripts" in your main GTA V directory.
4) Install ScriptHookVDotNet & ScriptHookV

How To Use
------------------------------
Once in game, press the F10 key. Misssions should start soon

Replacing the key
The default shortcut key is "F10", in order to modify this, follow the "Changing mod settings" instructions.

Recommended mods that fits well

WeaponCarryingWeight
https://www.gta5-mods.com/scripts/weaponcarryingweight
Backpack
https://www.gta5-mods.com/scripts/backpack

Adding more realism to realism itself ! it is a good idea to not carry around too many rifles when you decide to start delivering pizzas ! Unless it's a spicy one, of course... A nice touch of realism, i recommend getting these !

Pull Me Over
https://www.gta5-mods.com/scripts/pull-me-over-0-8
Back on the beat
https://www.gta5-mods.com/misc/cops-back-on-the-beat
Hide Minimap Shortcut
https://www.gta5-mods.com/scripts/hide-minimap-shortcut
Easy Engine Control
https://www.gta5-mods.com/scripts/easy-engine-control

These mods works great together. If you decide to hide the minimap, then until you activate the "Show minimap inside vehicle mod", you will have to use your phone to find out where the target actually is. Pull Me Over is actually well done since it detects when your phone is up and police will give you a ticket. That is, unless you have the Easy Engine Control, and actually take the time to turn off your engine before you put your phone up, then the police won't harass you !

Back On The Beat is a well balanced police patrole spawning mod, and works well with Pull Me Over. It is actually recommended by the author of the mod.


Changing mod settings

1) Open "Hitman.ini" file
2) Edit one of the followings :

[Key Settings]
StartHitmanMode = F10 // Activate / Deactivate hitman mod

[Gameplay Settings]
MaxDistanceTargetAppearsFromPlayer = 600 // self explanatory, 600 == 600 meters
EnableMiddlemanPhoneContact = true // if you set this to false, you won't be able to see the Middleman as a phone contact, and you won't be able to activate CallMiddlemanBeforeEach
TimeBeforeNextTargetAppears = 12 // self explanatory, 12 is supposed to be the time in seconds. It depends on YourGeneralFpsInGame
YourGeneralFpsInGame = 60 // This is something you want to set up. If you happen to have a slow computer, and have low fps ingame, you should define a medium fps constant and set this to it.
CallMiddlemanBeforeEach = false // When this is set to true, you will have to call the middleman every time you want a new mission.
SpawnHitmanVehicle = false // El famoso vehicle, that you may or may not want to spawn when you hit the Start key.

3) You're good to go !

I hope you'll enjoy this new version, it took me quite some time to make !
Regarding the iFruitAddon2.dll file, credits go to Bob74 (https://github.com/Bob74/iFruitAddon2/releases) and, furthermore, to CamxxCore (https://github.com/CamxxCore/iFruitAddon/releases) for their hard work regarding scaleforms push and phone interaction !
