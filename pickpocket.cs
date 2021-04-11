using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/*

Made by iLLo - 02/20/2018

This mod is an "early access" version, that's why the code is so messy and things are still in commentaries.

*/


namespace PickPocket {
	public struct PickupData
	{
	    public String ScenarioString { get; private set; }
	    public PickupType PickupTypeValue { get; private set; }
	    public Vector3 Rotation { get; set; }

	    public PickupData(String scenarioString, PickupType pickupType, Vector3 rotation) : this()
	    {
	    	ScenarioString = scenarioString;
	        PickupTypeValue = pickupType;
	        Rotation = rotation;
	    }
	}

	public class PickPocketMod : Script 
	{
		List<Ped> emptyPeds = new List<Ped>();

		List<String> playerInventory = new List<String>();
		List<String> vehicleInventory = new List<String>();
		List<Prop> physicsPropList = new List<Prop>();
		String animDictPlayer = "anim@weapons@first_person@aim_idle@generic@melee@switchblade@shared@core";
		String holsterAnim = "holster";
		String unHolsterAnim = "settle_med";
		Bone AttachedBone = Bone.SKEL_R_Finger02;
		Entity curProp = null;
		Entity physicsProp = null;
		Vector3 cigRotation = new Vector3(-190, -150, -960);
		Vector3 phoneRotation = new Vector3(-270, 10, -240);
		Vector3 purseRotation = new Vector3(-280, 50, -240);
		Vector3 weedRotation = new Vector3(-270, 10, -240);
		Vector3 currentCameraDirection = Vector3.Zero;
		// -270:10:-240 : prop_amb_phone
		// -190 -150 -960 : ng_proc_cigpak01b
		// -280:50:-240 : purse
		// ? : prop_weed_bottle

		List<PickupData> pickupDataList = new List<PickupData>();
		List<PickupData> randomItems = new List<PickupData>();

		// peds used in the OnTick loop
		Ped dealerPed = null; // ped that holds dealer tasks. see SetupDealer() && StartDeal() functions.

		GTA.Player player = null;
		Vehicle currentVehicle = null;
		ScriptSettings configSettings;

		Model weaponModel = null;
		WeaponHash weaponHash = WeaponHash.Unarmed;

		Keys FillPlayer;
		Keys FillCar;
		Keys DisplayInventory;
		Keys StartMode;
		Keys StealStuff;
		Keys SeeObject;
		Keys NextObject;
		Keys ThrowObject;

		int MAX_ITEM = 20; // Change here to be able to pickpocket more stuff at once.
	    int nearDistance = 600; // Change here to have the dealer spawn farther or closer to you.
		int MAX_ITEM_STACK = 60; // Change here to be able to stack more stuff in your storage car.
		int MAX_FORCE = 60; // Change here to be able to throw things at a higher speed;
		int MIN_FORCE = 5; // ^
		int TIME_NOTICEABLE = 20; // This is the time within which you can get noticed by people

		int currentTick = 0;
		int globalIndex = 0;
		int curForce = 5;
		int currentLevel = 1;
		int succesfullSteals = 0;
		int NotificationsType = 1;
		int ammoToGive = 0;

		int lastEvent = 0;
		int timerNoticed = 0;
		bool wasNotified = false;

		bool grabbyMod = true; // If you want to make sure no ped will ever disappear from you bumping into them, set this false... 
		bool automaticMode = true; // Change here if you want to HAVE to press a key to steal the sweet sweet money and stuff.

		bool canSteal = true;
	    bool startDeal = false;
	    bool activated = false;
	    bool wasActivated = false;
	    bool seeObjectFlag = false;
		bool dealerIsFleeing = false;
		bool dealerStartAiming = false;
		bool controlFillPlayerPressed = false;
		bool controlFillVehiclePressed = false;
		bool checkForThrownObject = false;
		bool IncreaseForce = false;

		public PickPocketMod()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
			KeyUp += OnKeyUp;
			//Interval = 1;
			fillPickupData();
			fillRandomItems();
			LoadConfig("scripts//pickpocket.ini");
		}

		private void LoadConfig(String fileName)
		{
			try
			{
				/*KEY SETTINGS*/
				configSettings = ScriptSettings.Load(fileName);
				StartMode = configSettings.GetValue<Keys>("Key Settings", "StartPickpocketMod", Keys.F10);

				FillPlayer = configSettings.GetValue<Keys>("Key Settings", "FillPlayerInventory_And_SetCurrentVehicle", Keys.Y);
				FillCar = configSettings.GetValue<Keys>("Key Settings", "FillCarInventory", Keys.U);
				DisplayInventory = configSettings.GetValue<Keys>("Key Settings", "DisplayInventory", Keys.I);
				StealStuff = configSettings.GetValue<Keys>("Key Settings", "StealStuff", Keys.Y);
				SeeObject = configSettings.GetValue<Keys>("Key Settings", "ShowObject", Keys.K);
				NextObject = configSettings.GetValue<Keys>("Key Settings", "NextObject", Keys.U);
				ThrowObject = configSettings.GetValue<Keys>("Key Settings", "ThrowObject", Keys.L);

				/*VALUES SETTINGS*/
				MAX_ITEM = configSettings.GetValue<int>("Inventory Settings", "MaxPlayerInventoryCapacity", 20);
				MAX_ITEM_STACK = configSettings.GetValue<int>("Inventory Settings", "MaxCarInventoryCapacity", 60);
				MAX_FORCE = configSettings.GetValue<int>("Inventory Settings", "MaxForceThrowingObjects", 60);
				MIN_FORCE = configSettings.GetValue<int>("Inventory Settings", "MinForceThrowingObjects", 20);
				NotificationsType = configSettings.GetValue<int>("Inventory Settings", "NotificationsType", 1); // 1 = Notif, 2 = subs.

				automaticMode = configSettings.GetValue<bool>("Gameplay Settings", "EnableAutomaticTheftMode", true);
				canSteal = automaticMode;
				nearDistance = configSettings.GetValue<int>("Gameplay Settings", "MaxDistanceDealerAppearsFromPlayer", 600);
				grabbyMod = configSettings.GetValue<bool>("Gameplay Settings", "EnableGrabbyMod", false);
				IncreaseForce = configSettings.GetValue<bool>("Gameplay Settings", "IncreaseForce", false);
			}
			catch (Exception e)
			{
				UI.Notify("~r~Error~w~: " + fileName.ToString() + " failed to load : " + e.ToString() + ".");
				DefaultValues();
			}
		}

		private void DefaultValues()
		{
			StartMode = Keys.F10;
			FillPlayer = Keys.Y;
			FillCar = Keys.U;
			DisplayInventory = Keys.I;
			StealStuff = Keys.Y;
			SeeObject = Keys.K;
			NextObject = Keys.U;
			ThrowObject = Keys.L;
			MAX_ITEM = 20;
			MAX_ITEM_STACK = 60;
			MAX_FORCE = 60;
			MIN_FORCE = 20;
			NotificationsType = 1;
			automaticMode = true;
			canSteal = true;
			nearDistance = 600;
			grabbyMod = false;
			IncreaseForce = false;
		}

	    private void fillPickupData() // More data recognition to come. A bit hard to develop though, every kind of prop seems to have its own implementation system, so...
	    {
	    	pickupDataList.Add(new PickupData("prop_amb_ciggy_01", GTA.PickupType.MoneyWallet, cigRotation));
	    	pickupDataList.Add(new PickupData("prop_cs_ciggy_01", GTA.PickupType.MoneyWallet, cigRotation));
	    	pickupDataList.Add(new PickupData("prop_cs_ciggy_01b", GTA.PickupType.MoneyWallet, cigRotation));
	    	pickupDataList.Add(new PickupData("p_cs_ciggy_01b_s", GTA.PickupType.MoneyWallet, cigRotation));

	    	pickupDataList.Add(new PickupData("p_amb_coffeecup_01", GTA.PickupType.MoneyWallet, phoneRotation));
	    	pickupDataList.Add(new PickupData("ng_proc_coffee_01a", GTA.PickupType.MoneyWallet, phoneRotation));

	    	pickupDataList.Add(new PickupData("p_amb_bagel_01", GTA.PickupType.MoneyWallet, cigRotation));
	    	pickupDataList.Add(new PickupData("prop_amb_donut", GTA.PickupType.MoneyWallet, cigRotation));
	    	pickupDataList.Add(new PickupData("prop_donut_02b", GTA.PickupType.MoneyWallet, cigRotation));
	    	pickupDataList.Add(new PickupData("prop_donut_02", GTA.PickupType.MoneyWallet, cigRotation));
	    	pickupDataList.Add(new PickupData("prop_donut_01", GTA.PickupType.MoneyWallet, cigRotation));

	    	pickupDataList.Add(new PickupData("prop_amb_phone", GTA.PickupType.MoneyWallet, phoneRotation));
	    	pickupDataList.Add(new PickupData("p_amb_phone_01", GTA.PickupType.MoneyWallet, phoneRotation)); // <Those two never seem to pop.

		    pickupDataList.Add(new PickupData("p_amb_joint_01", GTA.PickupType.MoneyWallet, weedRotation)); // <|___________________________|
	    	pickupDataList.Add(new PickupData("prop_sh_joint_01", GTA.PickupType.MoneyWallet, weedRotation));
	    	pickupDataList.Add(new PickupData("p_cs_joint_01", GTA.PickupType.MoneyWallet, weedRotation));
	    	pickupDataList.Add(new PickupData("p_cs_joint_02", GTA.PickupType.MoneyWallet, weedRotation));

	    	pickupDataList.Add(new PickupData("prop_amb_handbag_01", GTA.PickupType.MoneyWallet, purseRotation));
	    	pickupDataList.Add(new PickupData("prop_ld_purse_01", GTA.PickupType.MoneyWallet, purseRotation));
	    	pickupDataList.Add(new PickupData("prop_ld_handbag_s", GTA.PickupType.MoneyWallet, purseRotation));
	    	pickupDataList.Add(new PickupData("prop_ld_handbag", GTA.PickupType.MoneyWallet, purseRotation));

	    	pickupDataList.Add(new PickupData("p_watch_01", GTA.PickupType.MoneyWallet, cigRotation));
	    	pickupDataList.Add(new PickupData("p_watch_02", GTA.PickupType.MoneyWallet, cigRotation));
	    	pickupDataList.Add(new PickupData("p_watch_03", GTA.PickupType.MoneyWallet, cigRotation));
	    	pickupDataList.Add(new PickupData("p_watch_04", GTA.PickupType.MoneyWallet, cigRotation));
	    	pickupDataList.Add(new PickupData("p_watch_05", GTA.PickupType.MoneyWallet, cigRotation));
	    	pickupDataList.Add(new PickupData("p_watch_06", GTA.PickupType.MoneyWallet, cigRotation));

	    	pickupDataList.Add(new PickupData("prop_binoc_01", GTA.PickupType.MoneyWallet, cigRotation));

			pickupDataList.Add(new PickupData("prop_tourist_map_01", GTA.PickupType.MoneyWallet, Vector3.Zero));
			pickupDataList.Add(new PickupData("p_tourist_map_01_s", GTA.PickupType.MoneyWallet, Vector3.Zero));

			pickupDataList.Add(new PickupData("prop_amb_beer_bottle", GTA.PickupType.MoneyWallet, Vector3.Zero));
			pickupDataList.Add(new PickupData("prop_amb_40oz_03", GTA.PickupType.MoneyWallet, Vector3.Zero));
			pickupDataList.Add(new PickupData("p_amb_bag_bottle_01", GTA.PickupType.MoneyWallet, Vector3.Zero));
			pickupDataList.Add(new PickupData("prop_amb_40oz_02", GTA.PickupType.MoneyWallet, Vector3.Zero));

			pickupDataList.Add(new PickupData("p_amb_brolly_01_s", GTA.PickupType.MoneyWallet, Vector3.Zero));
			pickupDataList.Add(new PickupData("p_amb_brolly_01", GTA.PickupType.MoneyWallet, Vector3.Zero));

			pickupDataList.Add(new PickupData("prop_cs_tablet", GTA.PickupType.MoneyWallet, Vector3.Zero)); 

			pickupDataList.Add(new PickupData("prop_acc_guitar_01", GTA.PickupType.MoneyWallet, Vector3.Zero));
			pickupDataList.Add(new PickupData("prop_acc_guitar_01_d1", GTA.PickupType.MoneyWallet, Vector3.Zero));
			pickupDataList.Add(new PickupData("prop_el_guitar_01", GTA.PickupType.MoneyWallet, Vector3.Zero));
			pickupDataList.Add(new PickupData("prop_el_guitar_02", GTA.PickupType.MoneyWallet, Vector3.Zero));
			pickupDataList.Add(new PickupData("prop_el_guitar_03", GTA.PickupType.MoneyWallet, Vector3.Zero));

			pickupDataList.Add(new PickupData("prop_bongos_01", GTA.PickupType.MoneyWallet, Vector3.Zero));
	    	pickupDataList.Add(new PickupData("prop_ing_camera_01", GTA.PickupType.MoneyWallet, cigRotation));
	    	pickupDataList.Add(new PickupData("prop_pap_camera_01", GTA.PickupType.MoneyWallet, cigRotation));

			pickupDataList.Add(new PickupData("physics_hat", GTA.PickupType.MoneyWallet, Vector3.Zero));
			pickupDataList.Add(new PickupData("physics_glasses", GTA.PickupType.MoneyWallet, Vector3.Zero));
			pickupDataList.Add(new PickupData("PHYSICS", GTA.PickupType.MoneyWallet, Vector3.Zero));
	    	pickupDataList.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, purseRotation)); // Don't move/remove this one.
	    }

	    private void fillRandomItems()
	    {
	    	randomItems.Add(new PickupData("prop_weed_bottle", GTA.PickupType.MoneyWallet, weedRotation));
	    	randomItems.Add(new PickupData("prop_cs_fork", GTA.PickupType.MoneyWallet, cigRotation));
	    	randomItems.Add(new PickupData("p_cs_lighter_01", GTA.PickupType.MoneyWallet, cigRotation));
	    	randomItems.Add(new PickupData("p_notepad_01_s", GTA.PickupType.MoneyWallet, purseRotation));
	    	randomItems.Add(new PickupData("prop_fib_badge", GTA.PickupType.MoneyWallet, phoneRotation));
	    	randomItems.Add(new PickupData("prop_cs_business_card", GTA.PickupType.MoneyWallet, Vector3.Zero));
	    	randomItems.Add(new PickupData("p_ld_id_card_002", GTA.PickupType.MoneyWallet, Vector3.Zero));
	    	randomItems.Add(new PickupData("prop_cs_credit_card", GTA.PickupType.MoneyWallet, Vector3.Zero));
	    	randomItems.Add(new PickupData("prop_toothbrush_01", GTA.PickupType.MoneyWallet, cigRotation));
	    	randomItems.Add(new PickupData("prop_pencil_01", GTA.PickupType.MoneyWallet, cigRotation));
	    	randomItems.Add(new PickupData("prop_phone_ing_02", GTA.PickupType.MoneyWallet, phoneRotation)); //phoneRotation
	    	randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
	    	randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
	    	randomItems.Add(new PickupData("prop_binoc_01", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("ng_proc_cigpak01a", GTA.PickupType.MoneyWallet, cigRotation)); //cigRotation
			randomItems.Add(new PickupData("p_meth_bag_01_s", GTA.PickupType.MoneyWallet, purseRotation)); //purseRotation
			randomItems.Add(new PickupData("p_syringe_01_s", GTA.PickupType.MoneyWallet, Vector3.Zero));
			randomItems.Add(new PickupData("p_jewel_necklace_02", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("prop_security_case_01", GTA.PickupType.MoneyWallet, purseRotation)); //purseRotation
			randomItems.Add(new PickupData("prop_cs_swipe_card", GTA.PickupType.MoneyWallet, Vector3.Zero));
			randomItems.Add(new PickupData("ng_proc_sodacan_01b", GTA.PickupType.MoneyWallet, phoneRotation));
			randomItems.Add(new PickupData("p_watch_01", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("prop_npc_phone_02", GTA.PickupType.MoneyWallet, phoneRotation)); //phoneRotation
			randomItems.Add(new PickupData("prop_v_m_phone_01", GTA.PickupType.MoneyWallet, phoneRotation)); //phoneRotation
			randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("p_banknote_onedollar_s", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("prop_cs_crackpipe", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("prop_knife", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("prop_fib_badge", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("prop_cs_r_business_card", GTA.PickupType.MoneyWallet, Vector3.Zero));
			randomItems.Add(new PickupData("ng_proc_candy01a", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("prop_npc_phone", GTA.PickupType.MoneyWallet, phoneRotation)); //phoneRotation
			randomItems.Add(new PickupData("prop_phone_ing_03", GTA.PickupType.MoneyWallet, phoneRotation)); //phoneRotation
			randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("ng_proc_cigbuts01a", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("prop_anim_cash_note", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("p_car_keys_01", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("prop_cs_sol_glasses", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("prop_ld_contact_card", GTA.PickupType.MoneyWallet, Vector3.Zero));
			randomItems.Add(new PickupData("prop_cs_lipstick", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("p_watch_05", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("prop_phone_ing", GTA.PickupType.MoneyWallet, phoneRotation)); //phoneRotation
			randomItems.Add(new PickupData("prop_ld_wallet_pickup", GTA.PickupType.MoneyWallet, purseRotation)); //purseRotation
			randomItems.Add(new PickupData("prop_sgun_casing", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
			randomItems.Add(new PickupData("NONE", GTA.PickupType.MoneyWallet, cigRotation));
	    }


		private void ClearAll()
		{
			foreach (Ped curPed in emptyPeds)
			{
				if (Entity.Exists(curPed))
					curPed.MarkAsNoLongerNeeded();
			}
			currentVehicle = null;
			pickupDataList.Clear();
			playerInventory.Clear();
			randomItems.Clear();
			if (Entity.Exists(dealerPed) && !dealerPed.IsDead)
			{
				dealerPed.Task.ClearAllImmediately();
				dealerPed.CurrentBlip.Remove();
				dealerPed.MarkAsNoLongerNeeded();
			}
			if (Entity.Exists(curProp))
			{
				curProp.Delete();
				curProp = null;
			}
			wasActivated = false;
			player.Character.Task.ClearAllImmediately();
		}

		private void CheckModStatus()
		{
			if (!activated)
			{
				if (wasActivated)
					ClearAll();
				return;
			}
			else if (!wasActivated)
				wasActivated = true;
		}

/*--------------------------------------------------------------------------------------------------------------------*/

/*-------------------------------------DISPLAY AND INFORMATIONS FUNCTIONS---------------------------------------------*/
		
		private void ShowObject(String scenarioString)
		{
			if (scenarioString == "")
			{
				if (playerInventory.Count != 0)
					scenarioString = playerInventory[0];
				else
					return ;
			}
			if (globalIndex >= playerInventory.Count)
				globalIndex = playerInventory.Count - 1;
			//UI.Notify("DEBUG::this is a " + scenarioString);
			UI.ShowSubtitle("This is a" + GetItemNameByScenario(scenarioString).ToString() + " item number " + globalIndex.ToString());
			holsterRegardingScenario(FindPickup(scenarioString), false, null);
		}

		private PickupData FindPickup(String scenarioString)
		{
			foreach (PickupData data in pickupDataList)
			{
				if (data.ScenarioString == scenarioString)
					return data;
			}
			foreach (PickupData data in randomItems)
			{
				if (data.ScenarioString == scenarioString)
					return data;
			}
			return pickupDataList[pickupDataList.Count - 1];
		}

		private bool IsSpecialItem(String item)
		{

			return item.Contains("cig") || item.Contains("phone") || item.Contains("tourist") || item.Contains("tablet") || item.Contains("purse") || item.Contains("binoc");
		}

		public Entity unholsterObject(String model, Vector3 rotation, Ped victim)
		{
			Entity prop = physicsProp;

			Vector3 boneCoord = Vector3.Zero;

			if (grabbyMod && !Game.Player.Character.IsWalking && !Game.Player.Character.IsSprinting && Game.Player.Character.IsIdle)
				boneCoord = Game.Player.Character.GetBoneCoord(AttachedBone);
			if (prop != null && victim != null)
			{
				if (currentLevel >= 5 && prop.Model.ToString() == victim.Weapons.Current.Model.ToString())
				{
					boneCoord = Vector3.Zero;
					prop = World.CreateProp(prop.Model, boneCoord, true, false);
					weaponModel = prop.Model;
					weaponHash = victim.Weapons.Current.Hash;
					ammoToGive = victim.Weapons.Current.Ammo;
					victim.Weapons.Remove(victim.Weapons.Current);
				}
			}
			if (prop == null || IsSpecialItem(model))
			{
				if (model != "NONE" && model != "PHYSICS")
					prop = World.CreateProp(model, boneCoord, true, false);
			}
			Function.Call((Hash)0xAE99FB955581844A, victim, 10, 10, 3, true, true, false); // small Ragdoll mode
			Wait(100);
			if (prop != null)
			{
				prop.AttachTo(Game.Player.Character, Game.Player.Character.GetBoneIndex(AttachedBone), Vector3.Zero, rotation);
				if (!Game.Player.Character.IsInVehicle())
					Function.Call(Hash.TASK_PLAY_ANIM, Game.Player.Character, animDictPlayer, unHolsterAnim, 2.0f, 0.0F, -1, 48, 0.0F, false, false, false);
			}
			physicsProp = null;
			return prop;
		}

		public void holsterObject(String propName, Entity prop, bool hussling)
		{
			bool shouldBePlayed = (propName.Contains("wallet") || !hussling || (hussling && playerInventory.Count < MAX_ITEM));
			if (shouldBePlayed)
				Function.Call(Hash.TASK_PLAY_ANIM, Game.Player.Character, animDictPlayer, holsterAnim, 2.0f, -2.0F, -1, 48, 0.0F, false, false, false);
			if (!Game.Player.Character.IsInVehicle())
				Function.Call(Hash.TASK_PLAY_ANIM, Game.Player.Character, animDictPlayer, holsterAnim, 2.0f, -2.0F, -1, 48, 0.0F, false, false, false);
			if (shouldBePlayed)
				player.Character.Task.ClearAnimation(animDictPlayer, holsterAnim);
			Wait(1000);
			if (prop != null && prop.Model.ToString() == weaponModel.ToString())
			{
				bool hasWeapon = Game.Player.Character.Weapons.Select(weaponHash);
				if (ammoToGive == -1)
					ammoToGive = (new Random()).Next(1, 26);
				if (hasWeapon)
				{
					Game.Player.Character.Weapons.Current.Ammo += ammoToGive;
					//UI.ShowSubtitle("DEBUG:Weapon ammo == " + ammoToGive.ToString());
				}
				else
				{
					Game.Player.Character.Weapons.Give(weaponHash, ammoToGive, true, true);
					//UI.ShowSubtitle("DEBUG:He didn't have the weapon" + ammoToGive.ToString());
				}
				weaponHash = WeaponHash.Unarmed;
			}
			if (prop != null && Entity.Exists(prop))
				prop.Detach();
			if (prop != null && (shouldBePlayed || prop.Model.ToString() == weaponModel.ToString()) && Entity.Exists(prop))
				prop.Delete();
			else if (prop != null && Entity.Exists(prop))
			{
				prop.MarkAsNoLongerNeeded();
			}
		}

		private bool isHusslingAnimActive()
		{
			if (Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, Game.Player.Character, animDictPlayer, holsterAnim,  3)
				|| Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, Game.Player.Character, animDictPlayer, unHolsterAnim,  3))
				return true;
			if (curProp != null && curProp.IsAttachedTo(Game.Player.Character))
				return true;
			return false;
		}

		private void throwObject()
		{
			if (curProp != null && curProp.IsAttachedTo(Game.Player.Character))
			{
				Function.Call(Hash.TASK_PLAY_ANIM, Game.Player.Character, animDictPlayer, holsterAnim, 2.0f, -2.0F, -1, 48, 0.0F, false, false, false);
				Game.Player.Character.Task.ClearAnimation(animDictPlayer, unHolsterAnim);
				Game.Player.Character.Task.ClearAnimation(animDictPlayer, holsterAnim);
				Function.Call(Hash.TASK_PLAY_ANIM, Game.Player.Character, "weapons@projectile@", "throw_l_fb_stand", 2.0f, -2.0F, -1, 48, 0.0F, false, false, false);
				Wait(300);
				curProp.Detach();
				Game.Player.Character.Task.ClearAnimation("weapons@projectile@", "throw_l_fb_stand");
				curForce = curForce > MAX_FORCE ? MAX_FORCE : curForce;
				currentCameraDirection = GameplayCamera.Direction;
				curProp.ApplyForce(currentCameraDirection * curForce);
				checkForThrownObject = true;
			}
		}

		private void SetVehicleStack()
		{
			if (Game.Player.Character.IsInVehicle())
				currentVehicle = Game.Player.Character.CurrentVehicle;
			UI.Notify(currentVehicle.FriendlyName.ToString() + " is now your storage vehicle.");
		}

		private String GetItemNameByScenario(String scenario)
		{
			if (scenario.Contains("joint") || scenario.Contains("weed"))
				return " Spliff !";
			else if (scenario.Contains("phone"))
				return "n iFruit phone";
			else if (scenario.Contains("cigp") || scenario.Contains("cigg"))
				return " Pack of cigs";
			else if (scenario.Contains("handbag"))
				return " Handbag";
			else if (scenario.Equals("prop_ld_purse_01"))
				return " Ponsonby's purse !";
			else if (scenario.Contains("watch"))
			{
				if (scenario.Contains("4"))
					return " Precious watch";
				return " Watch";
			}
			else if (scenario.Equals("p_cs_lighter_01"))
				return " Lighter";
			else if (scenario.Contains("jewel"))
			{
				if (scenario.Contains("neck"))
					return " Necklace";
				return " Jewel";
			}
			else if (scenario.Contains("dollar") || scenario.Contains("cash"))
				return " Dollar note";
			else if (scenario.Contains("notepad"))
				return " Notepad";
			else if (scenario.Contains("binoc"))
				return " Pair of binoculars";
			else if (scenario.Contains("meth_bag"))
				return " Bag filled with meth !";
			else if (scenario.Contains("crackpipe"))
				return " Crack pipe";
			else if (scenario.Contains("syringe"))
				return " Syringe";
			else if (scenario.Contains("card"))
				return " Random card";
			else if (scenario.Contains("car_keys"))
				return " Set of car keys";
			else if (scenario.Contains("tablet"))
				return " Brand new tablet !";
			else if (scenario.Equals("prop_knife"))
				return " Big arse knife";
			else if (scenario.Equals("prop_cs_sol_glasses"))
				return " Cool pair of glasses";
			else if (scenario.Equals("prop_ing_camera_01"))
				return " Sofisticated camera !";
			else if (scenario.Equals("prop_security_case_01"))
				return " Locked suitcase. Who knows what might be in it ?";
			else if (scenario.Contains("shit"))
				return "n Unknown object (it smells weird)";
			else
				return " Random object";
		}

		private void fillInventory(bool player)
		{
			bool inventoryFull = false;
			int i = 0;
			int maxItem = 0;

			maxItem = (player) ? MAX_ITEM : MAX_ITEM_STACK;
			if (currentVehicle == null 
				|| !Game.Player.Character.IsInVehicle() 
				|| Game.Player.Character.CurrentVehicle != currentVehicle)
				return;
			if ((player && vehicleInventory.Count == 0) || (!player && playerInventory.Count == 0))
				return;
			if (player)
			{
				foreach(String item in vehicleInventory)
				{
					if (playerInventory.Count >= MAX_ITEM)
					{
						inventoryFull = true;
						break;
					}
					String newString = string.Format("{0}", item.ToString());
					playerInventory.Add(newString);
					i++;
				}
				vehicleInventory.RemoveRange(0, i);
			}
			else
			{
				foreach(String item in playerInventory)
				{
					if (vehicleInventory.Count >= MAX_ITEM_STACK)
					{
						inventoryFull = true;
						break;
					}
					String newString = string.Format("{0}", item.ToString());
					vehicleInventory.Add(newString);
					i++;
				}
				playerInventory.RemoveRange(0, i);
			}
			if (inventoryFull)
				UI.Notify("Your" + ((!player) ? " car " : " ") + "inventory is full (" + maxItem.ToString() + ").");
			if (i > 0)
				UI.Notify("Added " + i.ToString() + " to your" + ((!player) ? " car " : " ") + "inventory.");
		}

		private void displayInventory()
		{
			String inventory = "Pickpocket level : " + currentLevel.ToString() + "\nNext level in " + ((currentLevel * 10) - succesfullSteals).ToString() + " steals\n";

			inventory += "Your inventory (" + playerInventory.Count.ToString() + "/" + MAX_ITEM.ToString() + ")" + "\n";
			for (int i = 0; i < pickupDataList.Count; i++)
			{
				String countString = (playerInventory.Count(x => x == pickupDataList[i].ScenarioString.ToString())).ToString();
				if (countString != "0")
					inventory += "a" + (GetItemNameByScenario(pickupDataList[i].ScenarioString.ToString())).ToString() + " (" + countString.ToString() + ")" + "\n";
			}
			for (int i = 0; i < randomItems.Count; i++)
			{
				String countString = (playerInventory.Count(x => x == randomItems[i].ScenarioString.ToString())).ToString();
				if (countString != "0")
					inventory += "a" + (GetItemNameByScenario(randomItems[i].ScenarioString.ToString())).ToString() + " (" + countString.ToString() + ")" + "\n";				
			}
			if (playerInventory.Count == 0)
				inventory += " None";

			if (Game.Player.Character.IsInVehicle() && Game.Player.Character.CurrentVehicle == currentVehicle)
			{
				inventory += "\n\n Stacked in your car (" + vehicleInventory.Count.ToString() + "/" + MAX_ITEM_STACK.ToString() + ")" + "\n";
				for (int i = 0; i < pickupDataList.Count; i++)
				{
					String countString = (vehicleInventory.Count(x => x == pickupDataList[i].ScenarioString.ToString())).ToString();
					if (countString != "0")
						inventory += "a" + (GetItemNameByScenario(pickupDataList[i].ScenarioString.ToString())).ToString() + " (" + countString.ToString() + ")" + "\n";
				}
				for (int i = 0; i < randomItems.Count; i++)
				{
					String countString = (vehicleInventory.Count(x => x == randomItems[i].ScenarioString.ToString())).ToString();
					if (countString != "0")
						inventory += "a" + (GetItemNameByScenario(randomItems[i].ScenarioString.ToString())).ToString() + " (" + countString.ToString() + ")" + "\n";
				}
				if (vehicleInventory.Count == 0)
					inventory += " None";
			}
			UI.Notify(inventory);
		}

/*--------------------------------------------------------------------------------------------------------------------*/

/*-----------------------------------------DEALER RELATIVE FUNCTIONS---------------------------------------------------*/

	    public Vector3 getTotallyRandomPos(Vector3 player)
	    {
	        Random rnd = new Random();
	        float x, y, z;
	        Vector3 safeCoords = Vector3.Zero;
	        Vector3 unSafeCoords = Vector3.Zero;

	        for (int i = 0; i < 20; i++)
	        {
	            x = rnd.Next((int)player.X - nearDistance, (int)player.X + nearDistance);
	            y = rnd.Next((int)player.Y - nearDistance, (int)player.Y + nearDistance);
	            z = World.GetGroundHeight(new Vector2(x, y));
	            unSafeCoords = new Vector3(x, y, z);
	            safeCoords = World.GetSafeCoordForPed(unSafeCoords);
	            if (safeCoords != Vector3.Zero)
	                break;
	        }
	        return safeCoords;
	    }
	    public Vector3 getTotallyRandomPos(Vector3 player, int nearDistance)
	    {
	        Random rnd = new Random();
	        float x, y, z;
	        Vector3 safeCoords = Vector3.Zero;
	        Vector3 unSafeCoords = Vector3.Zero;

	        for (int i = 0; i < 20; i++)
	        {
	            x = rnd.Next((int)player.X - nearDistance, (int)player.X + nearDistance);
	            y = rnd.Next((int)player.Y - nearDistance, (int)player.Y + nearDistance);
	            z = World.GetGroundHeight(new Vector2(x, y));
	            unSafeCoords = new Vector3(x, y, z);
	            safeCoords = World.GetSafeCoordForPed(unSafeCoords);
	            if (safeCoords != Vector3.Zero)
	                break;
	        }
	        return safeCoords;
	    }

	    public void dealerNotify(String message)
	    {
	        Function.Call(Hash._SET_NOTIFICATION_TEXT_ENTRY, "STRING");
	        Function.Call((Hash)0x6C188BE134E074AA , message); //ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME
	        Function.Call<int>(Hash._SET_NOTIFICATION_MESSAGE_CLAN_TAG_2, "CHAR_BLANK_ENTRY", "CHAR_BLANK_ENTRY", false, 2, "Dealer", "Stop", 1.0f, "___!'", 9);
	        Function.Call(Hash._DRAW_NOTIFICATION, 0, 1);
	    }

		// SetUpDealer function is pretty bad, i basically just added stuff as ideas came to my mind, tricking my way around problems...
		//Need to be redone 
		private void SetUpDealer(int nearDistance)
		{
			if (dealerPed != null && !Entity.Exists(dealerPed))
			{
				dealerPed = null;
				dealerIsFleeing = false;
			}
			if (dealerPed == null)
			{
				Vector3 dealerPedPos = getTotallyRandomPos(Game.Player.Character.Position);
				if (dealerPedPos != Vector3.Zero)
				{
					dealerStartAiming = false;
					dealerPed = World.CreateRandomPed(dealerPedPos);
					Blip dealerBlip = dealerPed.AddBlip();
					dealerBlip.Color = GTA.BlipColor.Green;
					dealerPed.Task.ClearAllImmediately();
					dealerPed.Weapons.Give(WeaponHash.VintagePistol, 120, true, true);
					dealerPed.Task.GuardCurrentPosition();
				}
			}
			else if (Vector3.Distance(dealerPed.Position, Game.Player.Character.Position) > nearDistance)
			{
				dealerPed.Task.ClearAllImmediately();
				dealerPed.MarkAsNoLongerNeeded();
				dealerStartAiming = false;
			}
			else if (dealerPed.IsDead && Vector3.Distance(dealerPed.Position, Game.Player.Character.Position) > 20)
			{
				dealerPed.Task.ClearAllImmediately();
				dealerPed.CurrentBlip.Remove();
				dealerPed.MarkAsNoLongerNeeded();
				dealerStartAiming = false;
			}
			else if (Vector3.Distance(dealerPed.Position, Game.Player.Character.Position) <= 5.0F)
			{
				if (Game.Player.WantedLevel != 0 && !dealerIsFleeing && !dealerPed.IsDead)
				{
					dealerPed.Task.ClearAllImmediately();
					dealerPed.Task.FleeFrom(Game.Player.Character);
					dealerPed.MarkAsNoLongerNeeded();
					dealerNotify("I'm outta here!");
					dealerIsFleeing = true;
					dealerStartAiming = false;
				}
				else if (dealerStartAiming && !Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, dealerPed, 4) && !Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, dealerPed, 355) && (Function.Call<bool>((Hash)0x6CD5A433374D4CFB, dealerPed, Game.Player.Character) || Function.Call<bool>(Hash.CAN_PED_HEAR_PLAYER, Game.Player, dealerPed)))
				{
					dealerPed.Task.AimAt(Game.Player.Character, -1);
				}
				else if (startDeal == false && Game.IsControlJustReleased(2, GTA.Control.Context) && !dealerPed.IsDead && !dealerStartAiming)
				{
					startDeal = true;
					dealerPed.CurrentBlip.IsFlashing = false;
				}
			}
			else if (Vector3.Distance(dealerPed.Position, Game.Player.Character.Position) >= 2.0F)
			{
				if (dealerStartAiming && Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, dealerPed, 4) && ((!Function.Call<bool>((Hash)0x6CD5A433374D4CFB, dealerPed, Game.Player.Character) && !Function.Call<bool>(Hash.CAN_PED_HEAR_PLAYER, Game.Player, dealerPed)) || (Vector3.Distance(dealerPed.Position, Game.Player.Character.Position) >= 50.0F)))
				{
					dealerPed.Task.ClearAllImmediately();
					dealerPed.Task.FleeFrom(Game.Player.Character);
					dealerPed.MarkAsNoLongerNeeded();
					dealerNotify("I'm outta here!");
					dealerIsFleeing = true;
					dealerStartAiming = false;
				}
				startDeal = false;
			}
		}

		private int GetRandomMoneyForItem(String item)
		{
			int max = 60;
			int min = 20;
			Random rand = new Random();

			max = item.Contains("cig") ? 10 : item.Contains("joint") || item.Contains("weed") ? 20 : item.Contains("phone") || item.Contains("watch") ? 100 : item.Contains("purse") || item.Contains("meth") || GetItemNameByScenario(item).Contains("Precious") ? 2000 : item.Equals("prop_security_case_01") ? 10000 : 60;
			min = max / 3;
			return (rand.Next(min, max + 1));
		}

		private void StartPoliceBust()
		{
			int nbCops = (new Random()).Next(1, currentLevel);
			UI.Notify("StartPoliceBust");
			for (int i = 0; i < nbCops; i++)
			{
				Vector3 position = getTotallyRandomPos(Game.Player.Character.Position, 70);
				if (position == Vector3.Zero)
					position = Game.Player.Character.Position.Around(70.0F);
				Ped pol_one = World.CreatePed(new Model("s_m_y_cop_01"), position.Around(16.0F));
				pol_one.Weapons.Give(WeaponHash.Pistol, -1, true, true);
				pol_one.MarkAsNoLongerNeeded();
			}
			Game.Player.WantedLevel = 1;
		}

		private bool HasSnitched()
		{
			int chancesOfSnitching = 0;

			chancesOfSnitching = (new Random()).Next((0 - currentLevel), 100);
			//UI.Notify("DEBUG:chancesOfSnitching : " + chancesOfSnitching.ToString());
			if (chancesOfSnitching > 90)
				return true;
			return false;
		}

		private bool WillSteal()
		{
			int chancesOfStealing = 0;

			chancesOfStealing = (new Random()).Next((0 - currentLevel), 100);
			//UI.Notify("DEBUG:chancesOfStealing : " + chancesOfStealing.ToString());
			if (chancesOfStealing > 90)
				return true;
			return false;
		}

		private bool StartDeal()
		{
			bool hasDealGoneRight = false;
			bool gotStolen = false;
			bool gotSnitchedOn = false;
			bool canSell = (playerInventory.Count > 0);
			int itemSold = 0;
			int totalMoney = 0;
			String sold = "";

			gotStolen = WillSteal();
			Wait(1);
			if (!gotStolen)
				gotSnitchedOn = HasSnitched();
			if (canSell)
			{
				foreach (String item in playerInventory)
				{
					int rndMoney = GetRandomMoneyForItem(item);

					Game.Player.Money += ((gotStolen || gotSnitchedOn) ? 0 : rndMoney);
					dealerPed.Money += rndMoney * ((gotStolen || gotSnitchedOn) ? 4 : 3);
					totalMoney += rndMoney;
					itemSold++;
				}
			}
			if (canSell && itemSold >= playerInventory.Count - 1)
			{
				for (int i = 0; i < pickupDataList.Count; i++)
				{
					String countString = (playerInventory.Count(x => x == pickupDataList[i].ScenarioString)).ToString();
					if (countString != "0" && sold.Contains("a"))
						sold += " and ";
					if (countString != "0")
						sold += "a" + (GetItemNameByScenario(pickupDataList[i].ScenarioString)) + " (" + countString + ")";
				}
				for (int i = 0; i < randomItems.Count; i++)
				{
					String countString = (playerInventory.Count(x => x == randomItems[i].ScenarioString)).ToString();
					if (countString != "0" && sold.Contains("a"))
						sold += " and ";
					if (countString != "0")
						sold += "a" + (GetItemNameByScenario(randomItems[i].ScenarioString)) + " (" + countString + ")";
				}

				hasDealGoneRight = true;
				playerInventory.Clear();
				succesfullSteals += itemSold;
				if (gotSnitchedOn)
				{
					dealerNotify("Sorry, but i can't go back to jail..");
					StartPoliceBust();
					return false;
				}
				else if (gotStolen)
				{
					dealerNotify("Thanks for the free stuff ! hahaha ! Now get out of here before i cap your arse.");
					dealerStartAiming = true;
					return false;
				}
				dealerNotify("Nice doing business with you, i'll give you $" +  totalMoney + " for all that.");
//				UI.Notify("You sold " + sold);
			}
			if (!sold.Contains("a"))
			{
				dealerNotify("Get out of here ! you're wasting my time, here. And time is Money!");
			}
			if (Entity.Exists(dealerPed))
			{
				dealerPed.Task.TurnTo(player.Character, 2000);
			}

			return hasDealGoneRight;
		}

/*--------------------------------------------------------------------------------------------------------------------*/

/*----------------------------------------PICKPOCKET INTERACTION FUNCTIONS--------------------------------------------*/

		private bool holsterRegardingScenario(PickupData scenarioData, bool hussling, Ped victim)
		{
			Vector3 curRotation = scenarioData.Rotation;
			String realScenario = (scenarioData.ScenarioString == "") ? "prop_ld_wallet_01" : (scenarioData.ScenarioString.Contains("ciggy")) ? "ng_proc_cigpak01b" : (scenarioData.ScenarioString.Contains("joint")) ? "prop_weed_bottle" : scenarioData.ScenarioString;

			if (!Game.Player.Character.IsRagdoll)
			{
				curProp = unholsterObject(realScenario, curRotation, victim);

				if (curProp != null && curProp.IsAttachedTo(Game.Player.Character) && hussling)
					holsterObject(realScenario, curProp, hussling);
				else
					return false;
				return true;
			}
			return false;
		}

		private void Notify(String message)
		{
			switch (NotificationsType)
			{
				case 1:
					UI.Notify(message);
				break;
				case 2:
					UI.ShowSubtitle(message, 2000);
				break;
			}
		}

		private bool Hussle(Ped victim, PickupData scenarioData)
		{
			bool hasHusslingSucceeded = false;
			String variationModel = "";

			if (scenarioData.ScenarioString != "NONE")
			{
				if (!isHusslingAnimActive())
					hasHusslingSucceeded = holsterRegardingScenario(scenarioData, true, victim);
				else
					hasHusslingSucceeded = true;
				int rS = (new Random()).Next(0, 3);
				String speech = (rS == 0 || rS == 1) ? "SHOOT" : ((rS == 2) ? ((victim.Gender == GTA.Gender.Male) ? "GENERIC_INSULT_MALE" : "GENERIC_INSULT_FEMALE") : "SHOP_HURRY");
				Function.Call((Hash)0x8E04FEDD28D42462, Game.Player.Character, speech, "SPEECH_PARAMS_SHOUTED_CLEAR");
			}
			else
				hasHusslingSucceeded = false;

			// I'm getting tired of notifications... Maybe draw something like a bubble menu to make it rp ?
			// Using it with SET_DRAW_ORIGIN or something...
			if (playerInventory.Count < MAX_ITEM)
			{
				if (scenarioData.ScenarioString != "PHYSICS" && scenarioData.ScenarioString != "physics_hat" && scenarioData.ScenarioString != "physics_glasses")
				{
					playerInventory.Add(scenarioData.ScenarioString);
					Notify("You got yourself a" + GetItemNameByScenario(scenarioData.ScenarioString).ToString() + " (" + playerInventory.Count.ToString() + ")");
				}
			}
			else
				UI.ShowSubtitle("You're full ! go sell your stuff. Or find somewhere to stash it");				
			return hasHusslingSucceeded;
		}
		void updatePlayerLevel()
		{
			if (succesfullSteals >= currentLevel * 10 && succesfullSteals != 0)
			{
				int remainingSteals = succesfullSteals % (currentLevel == 0 ? 1 : (currentLevel * 10));
				currentLevel += succesfullSteals / (currentLevel == 0 ? 1 : (currentLevel * 10));
				succesfullSteals = remainingSteals;
				Notify("Congratulations ! You are now level " + currentLevel.ToString());
			}
		}
		private unsafe bool FightOrFlight(bool shouldNotifyPlayer, bool *reset)
		{
			int combatFloat = 0;
			bool noticed = false;
			int totalParticipants = 0;
			List<Ped> allPeds = World.GetNearbyPeds(Game.Player.Character.Position, 50.0F).ToList(); 
			foreach(Ped cur in allPeds)
			{
				combatFloat = Function.Call<int>(Hash.GET_PED_ALERTNESS, cur);
				if (combatFloat > 0 || cur.IsFleeing || cur.IsInMeleeCombat || cur.GetMeleeTarget() == Game.Player.Character || cur.IsRagdoll)
				{
					if (cur.IsInMeleeCombat)
						*reset = true;
					noticed = true;
					totalParticipants++;
				}
				if (Function.Call<int>(Hash.GET_PED_TYPE, cur) == 6)
					totalParticipants = 10;
			}
			if (noticed)
			{
				if (shouldNotifyPlayer)
				{
					Notify("You got noticed by someone");
					wasNotified = true;
				}
				if (totalParticipants >= 5 && Game.Player.WantedLevel == 0)
				{
					Function.Call(Hash.REPORT_CRIME, Game.Player, 11, 1);
					if (Game.Player.WantedLevel != 0)
						Notify("They called the 5-O !");
				}
			}
			timerNoticed -= 1;
			return noticed;
		}

		private void StartEvent(Ped ped)
		{
			int type = 108;
			float duration = 10.0F;
			if (lastEvent != 0)
			{
				Function.Call(Hash.REMOVE_SHOCKING_EVENT, lastEvent);
				lastEvent = 0;
			}
			lastEvent = Function.Call<int>(Hash.ADD_SHOCKING_EVENT_FOR_ENTITY, type, Game.Player.Character, duration);
		}

		// This one made me go crazy.. At first i thought i didn't detect ped because the moment you hit them, they stop their scenario.
		// So i figured : hey, let's do some functions to store these animations ! and later retrieve them if player hits one of them.
		// Bad idea, indeed... That's when the madness began.
		private unsafe void PickpocketMode()
		{
			bool hasHusslingSucceeded = false;
			SetUpDealer(nearDistance);
			if (Game.Player.Character.IsInVehicle() || isHusslingAnimActive())
				return;
			Ped[] allThePeds = World.GetNearbyPeds(Game.Player.Character.Position, 2);
			if (pickupDataList.Count == 0)
				fillPickupData();

			PickupData scenarioData = pickupDataList[pickupDataList.Count - 1];

			foreach (Ped ped in allThePeds)
			{
				if (!(ped == player.Character) && ped.IsTouching(Game.Player.Character) && canSteal && !ped.IsInVehicle() && ((!ped.IsInMeleeCombat && IsPlayerAvailable()) || currentLevel >= 15))
				{
					if ((scenarioData = isInScenario(ped)).ScenarioString != "NONE")
					{
						if (!emptyPeds.Contains(ped) || currentLevel >= 15)
						{
							hasHusslingSucceeded = Hussle(ped, scenarioData);
							if (hasHusslingSucceeded)
							{
								succesfullSteals++;
							}
						}
					}
					else if (ped != dealerPed && ped.Money > 0)
					{
						if (ped.Money > 20)
							succesfullSteals++;
						Game.Player.Money += ped.Money;
						ped.Money -= ped.Money;
						Function.Call(Hash._PLAY_AMBIENT_SPEECH1, Game.Player.Character, "GAME_WIN_SELF", "SPEECH_PARAMS_STANDARD");
						if (!isHusslingAnimActive())
							holsterRegardingScenario(new PickupData("prop_ld_wallet_01", GTA.PickupType.MoneyWallet, purseRotation), true, null);
						currentTick = 0;
						hasHusslingSucceeded = true;
					}
					if (!emptyPeds.Contains(ped))
						emptyPeds.Add(ped);
					timerNoticed = TIME_NOTICEABLE;
				}
			}
			if (emptyPeds.Count > 50)
				emptyPeds.Clear();
			if (startDeal == true && !dealerPed.IsDead && !dealerPed.IsFleeing)
			{
				if (StartDeal())
					Function.Call(Hash._PLAY_AMBIENT_SPEECH1, Game.Player.Character, "GAME_WIN_SELF", "SPEECH_PARAMS_STANDARD");
				else
					Function.Call(Hash._PLAY_AMBIENT_SPEECH1, Game.Player.Character, "GENERIC_INSULT_MALE", "SPEECH_PARAMS_STANDARD");					
				startDeal = false;
			}
			updatePlayerLevel();
			if (timerNoticed > 0)
			{
				bool reset = false;
				if (FightOrFlight(!wasNotified, &reset))
				{
					if (reset)
						timerNoticed = TIME_NOTICEABLE;
					StartEvent(Game.Player.Character);
				}
				else
					wasNotified = false;
			}
			else
				wasNotified = false;
		}

		private void CheckForThrownObject()
		{
			if (checkForThrownObject)
			{
				int boneIndex = Function.Call<int>(Hash.GET_LAST_MATERIAL_HIT_BY_ENTITY, curProp);
				Entity[] peds = World.GetNearbyEntities(curProp.Position, 2);
				foreach (Entity touchedPed in peds)
				{
					if (curProp != null && curProp.HasCollidedWithAnything)
					{
						if (curProp != null && touchedPed != null && touchedPed != Game.Player.Character && curProp.IsTouching(touchedPed))
						{
							Function.Call((Hash)0xAE99FB955581844A, touchedPed, 10, 10, 3, true, true, false);
							touchedPed.ApplyForce(currentCameraDirection * (curForce / 6)/*-3*/); // Here i could check how long since object was thrown to have a good ratio.
							//Function.Call(Hash.APPLY_FORCE_TO_ENTITY, touchedPed, /*ForceType.MaxForceRot2*/3, currentCameraDirection.X, currentCameraDirection.Y, currentCameraDirection.Z, 2.0F, 2.0F, 2.0F, boneIndex, true, false, true, false, false);
						}
						if (touchedPed == peds[peds.Count() - 1])
							checkForThrownObject = false;
					}
				}
				if (!checkForThrownObject)
				{
					curForce = MIN_FORCE;
					if (curProp != null)
						curProp.MarkAsNoLongerNeeded();
					Notify("You dropped a" + GetItemNameByScenario(playerInventory[globalIndex]));
					playerInventory.RemoveAt(globalIndex);
					if (globalIndex >= playerInventory.Count)
						globalIndex = 0;
				}
			}
		}

		private bool IsPlayerAvailable()
		{
			return (!Game.Player.Character.IsInMeleeCombat 
				&& !Game.Player.Character.IsInVehicle() 
				&& !Game.Player.IsAiming
				&& !Game.Player.Character.IsRagdoll);
		}

		private void OnTick(object sender, EventArgs e)
		{
			CheckModStatus();
			if (activated)
			{
				if (player == null && Game.Player != null)
					player = Game.Player;
				PickpocketMode();
				currentTick += 1;
				if (currentTick == 30)
					currentTick = 0;
				CheckForThrownObject();
			}
		}

/*----------------------------------------------------------------------------------------------------------*/

/*---------------------------------GET PED HOLDING OBJECTS INFO FUNCTIONS-----------------------------------*/

		private int GET_HASH_KEY(String modelName)
		{
			return Function.Call<int>(Hash.GET_HASH_KEY, modelName);
		}

		private PickupData GetRandomHoldingObject(Ped ped)
		{
			if (randomItems.Count == 0)
				fillRandomItems();
			int rndItem = new Random().Next(0, randomItems.Count);
			return randomItems[rndItem];
		} 

		public String GetObjectFromComponentId(int componentId)
		{
			String prop = "";
			switch (componentId)
			{
				case 0:
					prop = "physics_hat";
					break;
				case 1:
					prop = "physics_glasses";
					break;
			}
			return prop;
		}

		public Entity GetPedHoldingObjectVariation(Ped ped)
		{
			Entity holdObject = null;
			int variationId = -1;
			int propIndex = -1;
			int componentId = 0;
			String propName = "";

			for (int i = 0; i < 11; i++)
			{
				propIndex = Function.Call<int>(Hash.GET_PED_PROP_INDEX, ped, i);
				if (propIndex != -1)
				{
					componentId = i;
					break;
				}
			}
			if ((propName = GetObjectFromComponentId(componentId)) != "")
			{
				Function.Call(Hash.KNOCK_OFF_PED_PROP, ped, true, true, true, true);
				Wait(20);
				holdObject = Function.Call<Entity>(Hash.GET_CLOSEST_OBJECT_OF_TYPE, ped.Position.X, ped.Position.Y, ped.Position.Z, 3.0F, new Model(propName).Hash, false, true, true);
			}
			return holdObject;
		}

		public Entity GetPedHoldingObject(Ped ped)
		{
			Entity holdObject = null;
			int propId = -1;

			if (ped == Game.Player.Character)
				return null;

			Prop[] propObjects =  World.GetNearbyProps(ped.Position, 1.0f);

			foreach (Prop obj in propObjects)
			{
				if ((obj.IsAttachedTo(ped) && ((obj.HeightAboveGround > 1.0F && obj.HeightAboveGround < 2.0F) || obj.Model.ToString() == ped.Weapons.Current.Model.ToString()) ) || (!obj.IsAttached() && obj.HeightAboveGround > 1.0F && obj.HeightAboveGround < 2.0F))
				{
					if (obj.Model.ToString() == ped.Weapons.Current.Model.ToString())
					{
						if (currentLevel < 5)
						{
							if (obj == propObjects[propObjects.Count() - 1])
								break;
							else
								continue;
						}
					}
					holdObject = obj;
				}
			}
			if (holdObject != null)
			{
				physicsProp = holdObject;
				curProp	 = holdObject;
				if (holdObject.IsAttachedTo(ped) && holdObject.Model.ToString() != ped.Weapons.Current.Model.ToString())
					holdObject.Detach();
			}
			return holdObject;
		}

		public PickupData isInScenario(Ped ped)
		{
			Entity holding = GetPedHoldingObject(ped);
			PickupData resultData = pickupDataList[pickupDataList.Count - 1];

			if (holding == null || !Entity.Exists(holding))
			{
				if (currentLevel >= 5) //toChange
				{
					if (currentLevel >= 20 && holding == null && !emptyPeds.Contains(ped))
						return GetRandomHoldingObject(ped);
					holding = GetPedHoldingObjectVariation(ped);
					physicsProp = holding;
					if (physicsProp != null)
						return pickupDataList[pickupDataList.Count - 2];
					else
						return pickupDataList[pickupDataList.Count - 1];
				}
				return resultData;
			}
			if (holding != null)
			{
				physicsProp = holding;
				if (physicsProp.Model.ToString() == ped.Weapons.Current.Model.ToString())
					return pickupDataList[pickupDataList.Count - 2];
				foreach (PickupData data in pickupDataList)
				{
					if (holding.Model == new Model(data.ScenarioString))
					{
						resultData = data;
						break;
					}
				}
			}
			// Implement here to check for scenarios that won't involve ambiant objects.
			return resultData;
		}

/*----------------------------------------------------------------------------------------------------------*/

/*-----------------------------------USER INPUT DETECTION FUNCTIONS-----------------------------------------*/

	    void OnKeyDown(object sender, KeyEventArgs e)
	    {

	    	if (e.KeyCode == StartMode)
	    	{
            	activated = !activated;
            	UI.Notify("Pickpocket mode : " + ((activated) ? "on" : "off"));
	    	}
	    	else if (activated && e.KeyCode == DisplayInventory)
	    	{
            	displayInventory();
	    	}
	    	else if (activated && e.KeyCode == FillPlayer && Game.Player.Character.IsInVehicle())
	    	{
	            if (!controlFillPlayerPressed)
	            {
	      			if ((currentVehicle == null || currentVehicle != Game.Player.Character.CurrentVehicle))
	      				SetVehicleStack();
	        		fillInventory(true);
	        		controlFillPlayerPressed = true;
	        	}
	    	}
	    	else if (activated && e.KeyCode == FillCar && Game.Player.Character.IsInVehicle())
	    	{
	    		if (!controlFillVehiclePressed)
	    		{
		        	fillInventory(false); // If source is player, set to false; else, true;
		        	controlFillVehiclePressed = true;
		        }
	    	}
	    	else if (activated && e.KeyCode == StealStuff)
	    		canSteal = true;
	    	else if (activated && e.KeyCode == SeeObject)
	    	{
	    		if (playerInventory.Count > 0)
	    		{
	    			checkForThrownObject = false;
		    		seeObjectFlag = !seeObjectFlag;
		    		if (!seeObjectFlag && curProp != null)
		    			holsterObject(playerInventory[globalIndex].ToString(), curProp, false);
		    		else if (playerInventory.Count > 0)
		    			ShowObject(playerInventory[globalIndex]);
		    	}
	    	}
	    	else if (activated && e.KeyCode == NextObject && !checkForThrownObject)
	    	{
	    		if (globalIndex + 1 <= playerInventory.Count - 1)
	    			globalIndex++;
	    		else
	    			globalIndex = 0;

	   			UI.ShowSubtitle((globalIndex + 1).ToString() + " : a" + GetItemNameByScenario(playerInventory[globalIndex]));
	    	}
	    	else if (activated && e.KeyCode == ThrowObject)
	    	{
	    		if (seeObjectFlag && curProp != null && playerInventory.Count > 0)
	    		{
	    			if (curForce < MAX_FORCE && IncreaseForce)
	    				curForce++;
	    		}
	    	}
	    }
	    void OnKeyUp(object sender, KeyEventArgs e)
	    {
	    	if (e.KeyCode == FillPlayer)
	    		controlFillPlayerPressed = false;
	    	else if (e.KeyCode == FillCar)
	    		controlFillVehiclePressed = false;
	    	else if (activated && e.KeyCode == ThrowObject)
	    	{
	    		if (seeObjectFlag && curProp != null && playerInventory.Count > 0)
	    		{
		    		throwObject();
		   			seeObjectFlag = false;
	    		}
	    	}
	    	if (activated && !automaticMode && e.KeyCode == StealStuff)
	    		canSteal = false;
	    }
	}
}