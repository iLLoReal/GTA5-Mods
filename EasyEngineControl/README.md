<h1>Easy Engine Control</h1>
update V0.7
Version 0.7 is going to be the final version of this mod. To be more precise, it's going to be the last time something new is implemented. From now on, every update made to the game will be regarding fixing issues / enhancing already existing features.

This mod is an idea from westcoastsosa (link to the request forum below), that i found very funny, and it was indeed funny to code.

You get to be the petty thief, the struggler, the dude that will ruin tourists trip on any occasion he gets.

You have two angles here.

You can either choose to be fast, and brutal, bumping into people, forcing your way out
or you can play it subtle, like an artist, and arrive slowly behind your victime , and discretfully grab whatever you wanted to grab. Of course, you will have to be forcefull at one moment or another, people will realise they lost their phone. Or.. will they ?
You may get the latest iFruit, maybe a pack of cigarettes, or maybe just some good old cash, for your pockets' pleasure. You won't always get nice stuff though. Sometimes, shit happens.

You can't take money AND stuff at the same time, of course. You'll have to do the deed as many times as a pedeistrian might have stuff on him (as of now, two is the limit : one for the cash, one for the stuff)

Hey, you know what they say, a dollar is worth something, when you're at the bar.

You want to stay in touch with your very best dealer, without whom you wouldn't get such a beautiful reward for your misdemeanors. He's a bit touchy though, so don't try to mess with him. Unless that is your plan, of course...

He will be available at a location near you, pointed to by a green dot on your gps.
If he dies, well, c'est la vie, another one will take his place pretty soon, don't worry about that.

Changelog
------------------------------
v0.7
* Almost everything you see a ped holding can now be stolen.That is, if you are able to get to it before the ped drops it. Not everything will be added to your inventory though.
* Added possibility to steal most hats and glasses (the ones that are actually created by the game when the peds gets hit in the face for instance). They won't be added to your inventory though. Requires a certain level.
* Added a simple leveling system. There is no limit to the actual level you can reach, but the maximum level after which game won't be impacted by leveling is 20 at the moment. Good luck getting to That level though.
* Added possibility to steal guns. Now you know how pedestrians would have reacted if they were unarmed ! (thinking about you, Ammunation guy!). When you steal a weapon, it is automatically equipped to your character. Requires a certain level.
* Improved dealer's replacement in case of unfortunate accident.
* Fixed sold items list's display.
* Added possibility to choose not to increase strength at which you throw objects (ini).
* Added possibility to choose notifications type : subs / notifications (ini).
* Added wanted level system : getting noticed stealing will eventually get you one star. If enough people notice you. That or you got noticed three times.
* Added dealer snitching and trying to rob you regarding your current level as a pickpocket. Keep in mind a dealer is a dealer. His goal is to make money, and being able to spend it.
* Added peds reactions. As previously mentioned, from now on, people will be able to notice you, should you be a bad pickpocket. Some peds will try to stop you, while others will get scared of you. The more people are after you/ fleeing from you, the more people you will attract, so you better run fast, and smart.
v0.6
* Added multiple items to steal.
* Added the possibility to throw items by releasing the "L" key.
Holding the key allows you to increase the strength at which you throw it.
* Fixed several bug relative to selecting next inventory object.
v0.5
* More stable handling for animations.
* Fixed some of the peds disappearing.
Unfortunately, the "grabby hands" functionnality has to be removed for that to happen (what a surprise). As it was so much fun to me, and i didn't mind for one or two ped to disappear occasionnaly, i let the grabbyMod option enabled in the ini file by default. If you get irritated by the disappearances, all you have to do is to set it to false, and they will go away. But so will grabby hands ! :(
* Added Wallet animation.
* Added Weed animation.
* It is now possible to steal purses and joints !
* Items now have a maximum and minimum sell price according to their kind.
Hint : purses are expensive !
v0.4
* Added animations ! peds will try to resist sometime, but they aren't a match to you so don't worry.
* Added two keys : ShowObject (default K) and NextObject (default U). See description below for more info.

v0.3 :
* Optimized code to be less long to compile
* Enhanced collision detection system, should be far less perf consuming now
* Added special notification for the dealer
* Removed long list of selled items notifications, replaced with item shown by type of item and count of that type you had in your inventory
* Removed wanted levels obtained when hussling pedestrians.
* Removed time_before_wait feature, rendered useless by the update.

v0.2 :
* Added .ini file for easier config, and readme file for easier comprehension.
* Added possibility to have to do the theft yourself, with a key (check .ini file)
* More pedestrian animations recognition.
* Better wanted level adjustements. (still needs to be adjusted some more).
* Possibility to store your inventory in your vehicle.
* Enhanced inventory display.
* Enhanced ped detection system using raycasting
* Better overwhole processing time.
* Added DeathRay mod ! set deathMode to "true" as usual to test it.

Upcoming features
------------------------------

* Possibility to gain levels in the art of pickpocketing. Done
* Dealer's betrayal (snitch, robs you, ambush you, don't pay you.. you name it) Done
* Even better wanted level adjustments Done
* Possibility to store your inventory in a secluded area. Aborted
* To be determined. Done

How To Install
------------------------------
1) Download the file
2) Extract "pickpocket.cs" and "pickpocket.ini"
3) Place "pickpocket.cs" and "pickpocket.ini" inside a folder named "scripts" in your main GTA V directory.
4) Install ScriptHookVDotNet & ScriptHookV
5) open readme_pickpocketmod.txt if you're lost.


How To Use
------------------------------
- Once in game, press the F10 key to activate / deactivate.
You'll know it's on, because of the
dealer's blip (green) that shouldn't take too long to appear.

- To rob people, you need to look at them first.
Yeah, look at them, litterally, if you don't, how will you be able to do it anyway ?
- Seriously though, you do need to have the center of your screen pointing in their general directions, at least when you're about to comit the stealing.
--- (UPDATE) v0.6: You don't need to look at people in order to rob them anymore.

Go straight to the ped, walking or running, your choice. Be carefull though,
if you use force to get their stuff, there's a strong probability that they'll call the cops on you.
----(UPDATE) v0.6 : Peds won't call the cops anymore. they will react as GTA V make them react when you bump into them.

- Bump into them, get the stuff, and get the hell out ! By default, the EnableAutomaticTheftMode option is set to false, so you also have to press the 'Y' key as you bump them. You can configure what key you want in the pickpocket.ini file under the key settings section.

- You have an inventory of up to 20 slots !

- You can use a vehicle to store up to 60 items in it
In order to do so, you need to

press the 'Y' button, while inside the car.

- In order to store the stuff, while inside a car that is set, you need to press the 'U' button.

- You can only use one vehicle at a time.

- You don't actually have to DO the transfer from one vehicle to another though, it is done for you automatically.
Just press 'Y' again when inside the car

- Dealer needs to be close to you in order for you to make the deal. You also need to be on foot.
Once at a handshake's distance from him, press the contextual interaction control button (default 'E'). You know, that button you use in order to insult people...

- You can select an object from your inventory to look what it is. To navigate inside your inventory, press the NextObject key (default U). To show the selected object, you need to press the ShowObject key. (default K)

(UPDATE) v0.6- You can now THROW objects shown using the ShowObject key, using the ThrowObject key (default L).


Note : This mode is in a very "early access" stage, all ideas are welcome, and suggestions alike, a lot of things will be added in the near future.


How To Set Up
--------------------

Replacing the key
Key settings can be changed within the .ini file under the category [Key Settings]

[Key Settings]
StartPickpocketMode (default F10) As its name suggest, it activates / deactivates the mode.
FillPlayerInventory_And_SetCurrentVehicle (default Y) When inside a car sets the car as your storage box. once car is set, allows you to put in your inventory whatever is in its inventory.
FillCarInventory (default U) When inside a car that has been set as your storage box, allows you to put the stuff you have in your inventory into the car's inventory.
DisplayInventory (default I) Displays what is in your inventory. If in a car set as your storage box, also displays its inventory.
StealStuff (default Y) When on foot, if EnableAutomaticTheftMode is set to false, is the key that lets you actually do the steal. You still also need to be touching your victim for it to actually happen.
ShowObject (default K) Allows you to check selected object in inventory.
NextObject (Default U) Allows you to select the next object in your inventory, for you to show later.
ThrowObject (Default L) Allows you to throw (and therefor abandon) selected object. The more you hold, the harder it throws.

[Inventory Settings]
MaxPlayerInventoryCapacity (default 20) Maximum player's inventory capacity for holding stuff.
MaxCarInventoryCapacity (default 60) Maximum car's inventory capacity for holding stuff. This shouldn't be less than MaxPlayerInventoryCapacity.
MaxForceThrowingObjects Don't know what'll happen if you set this too high. I wouldn't recommend putting more than 200. Default 60, determines how hard the object can be thrown.
MinForceThrowingObjects

[Gameplay Settings]
MaxDistanceDealerAppearsFromPlayer (default 600) This sets the limit in meters for which the dealer is to appear near you.
DeathRayDistance (default 10.0F) The distance you can check for a target. Don't put this too high or it won't work anymore. Max 50
EnableAutomaticTheftMode (default false) If you want the theft to happen automatically or not. If set to true, check StealStuff key (default Y) to do the deed.
EnableGrabbyMod (defauult true) This enables or disables the fact that peds will try to hold on to the object you're taking from them. At the moment, if you let this true, you will have some peds disappear in front of you when you bump into them.


Controls :

Contextual interaction control (default key is E) When on foot :
Press Contextual interaction control (E) when near a dealer whom you want to sell your stuff to.

- You need to be at a handshake's distance from him. If you aren't, the deal won't happen.


Virus Scans
---------------------

0.1 : https://www.virustotal.com/#/file/d4b18d70f855740b1343c9579c8cd2b513fa47a5de7944ab5aba82716ec344ad/detection
0.2 : https://www.virustotal.com/#/file/bce4a28882670923c8bd5840770ae9aafcca8892a58297181c7a990063bbd270/detection
0.5 https://www.virustotal.com/#/file/5254a64371fd9aa781e02da367e59659ca6071c345e141e5520b21df8b6aa58c/detection
0.6 https://www.virustotal.com/#/file/fb395229fdce6cb84b05e6ce028aa257566a66157aa3d5d3d69ad3e1a637b6e6/detection
0.7 https://www.virustotal.com/#/file/7882f61490934c408b51a55c199fdfde4127b3c93a410c5f8417c01c21871f5d/detection