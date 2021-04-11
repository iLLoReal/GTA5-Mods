<h1>iLLo's Telekinesis</h1>
This mod is my version of a telekinesis mod. It lets you pick an entity from whatever distance you decided, and keeps it at this distance, while enabling you to move them around, and let them fly away as you release the key.

Since it is a very simple mod, i didn't make a .ini file for it. It's back to oldschool way of changing keys : in the source code directly.

Changelog
-------------------
v3.0
* Fixed distance variable not effectively impacting actual telekinesis distance
* Added possibility to grab almost every possible object. Try it on stuff, it is fun ! Sometimes you will be able to grab the object, sometimes you will just impact it.
* Added possibility to hit almost every object with almost every object. Have fun! The bigger the object, the higher the impact force !
v2.0
* Fixed entities disappearing when touching each other.
* Added possibility to hit entities with the selected entity.

How To Use
--------------------

Hit the shortcut key (default key is F10) to activate / deactivate the mod.

Then look at a ped / vehicle you would like to "handle" (as in make levitate/throw), and press the LockEntity key (default is E)
Keep pressing that key and do whatever you want as you are holding the key, to make the ped follow your cursor.
When you feel like you don't want to "handle" that ped / vehicle anymore, just release the key. Be aware that if you were moving your camera while you stop pressing the key, you will launch the entity accordingly.

If you want to change how you throw peds/vehicles, or how you start the mod, or at what strength you want the entities to be thrown, follow the "How To Set Up" instructions.

How To Install
------------------------------

1) Download the file
2) Extract "iLLoTelek.cs"
3) Place "iLLoTelek.cs" inside a folder named "scripts" in your main GTA V directory.
4) Install ScriptHookVDotNet & ScriptHookV

How To Set Up
--------------------
Replacing the keys

1) open the "iLLoTelek.cs" file
2) go at the beginning of the file
3) Look for "/*KEYS THAT YOU CAN CHANGE*/" then go to the "/*KEYS*/" section
4) modify the Keys that you want modified by following commentary instructions.

Changing mod settings

In order to change mod settings, you need to

1) open the "iLLoTelek.cs" file
2) go at the beginning of the file
2.5) Look for "/*KEYS THAT YOU CAN CHANGE*/" then go to the "/*SETTINGS*/" section
3) Follow //commentary instructions :

In case instructions aren't clear enougth, here's an example :
To change the strength at whitch entities will be thrown, replace :
"int strength = 800;"
to
"int strength = 500;"
Note that 500 is an arbitrary value, you can set it to whatever. Just don't be too enthousiastic about it ! (5000 is too much, 10 is too low for instance).

4) Do not edit a variable if there isn't a comment suggesting that you can. (unless you know what you're doing, of course, then i won't be responsible for anything, not that i was in the first place !)
A comment starts with the "//" sentence.
5) Save and close .cs file
6) You're good to go !

Virus Scans
---------------------
3.0 : https://www.virustotal.com/#/file/014fefceadb8a7b25799c23ef41f0a5cb5d368bd1e6f13a2dfb68ca45847540c/detection
2.0 : https://www.virustotal.com/#/file/61d8fc018a9272c4648ff6b93de8aceccaa9aec635f98f74a3aaa8b1e8b42984/detection
1.0 : https://www.virustotal.com/#/file/811a1c2409300ea5c529460ef547132d0e85bc7722d1a485cac42ae7057c797e/detection