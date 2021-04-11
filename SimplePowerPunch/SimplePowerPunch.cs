using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SimpleSuperPunch {

	public class SimpleSuperPunchMod : Script
	{
		private int DEFAULT_STRENGTH = 30;
		private int currentStrength = 30;
		private int STRENGTH_OFFSET = 10;

		private float dist = 10.0F;

		private bool activated = false;
		private bool shouldSave = true;
		private bool shouldCheck = false;
		private bool shouldIncrease = true;
		private bool shouldDecrease = true;

		private bool EveryOne = false;
		private bool NeverRagdoll = true;
		private bool ShouldBeRagdoll = false;
		private bool ThereIsNoStoppingMe = true;
		private bool ThereIsNoStoppingMeAtAll = true;
		private bool WhereYouAimIsWhereYouThrow = true;

		private Vector3 direction = Vector3.Zero;
		private Vector3 curDirection = Vector3.Zero;
		private Vector3 savedCurDirection = Vector3.Zero;

		private ScriptSettings configSettings;
		private ScriptSettings saveSettings;

		private Keys StartMode = Keys.F10;
		private Keys StoreModel = Keys.F11;
		private Keys NextStrength = Keys.F8;
		private Keys PrevStrength = Keys.F7;

		private GTA.Player player = null;
		private Entity weaponEntity = null;

		private String curModel = "";

		public SimpleSuperPunchMod()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
			KeyUp += OnKeyUp;
			LoadConfig("scripts//SimplePowerPunchSettings//SimplePowerPunch.ini");
		}


		private void LoadConfig(String fileName)
		{
			try
			{
				/*KEY SETTINGS*/
				configSettings = ScriptSettings.Load(fileName);
				StartMode = configSettings.GetValue<Keys>("Key Settings", "StartSimplePowerMod", Keys.F10);
				StoreModel = configSettings.GetValue<Keys>("Key Settings", "SaveModelWithStrength", Keys.F11);
				NextStrength = configSettings.GetValue<Keys>("Key Settings", "NextStrength", Keys.F8);
				PrevStrength = configSettings.GetValue<Keys>("Key Settings", "PrevStrength", Keys.F7);

				/*VALUES SETTINGS*/
				DEFAULT_STRENGTH = configSettings.GetValue<int>("Values Settings", "DefaultStrength", 30);
				STRENGTH_OFFSET = configSettings.GetValue<int>("Values Settings", "StrengthIncreaseDecreaseBy", 10);
				if (STRENGTH_OFFSET < 1)
					STRENGTH_OFFSET = 1;
				else if (STRENGTH_OFFSET > 2000)
					STRENGTH_OFFSET = 2000;
				ThereIsNoStoppingMe = configSettings.GetValue<bool>("Values Settings", "ThereIsNoStoppingMe", true);
				ThereIsNoStoppingMeAtAll = configSettings.GetValue<bool>("Values Settings", "ThereIsNoStoppingMeAtAll", true);
				WhereYouAimIsWhereYouThrow = configSettings.GetValue<bool>("Values Settings", "WhereYouAimIsWhereYouThrow", true);
				NeverRagdoll = configSettings.GetValue<bool>("Values Settings", "NeverRagdoll", true);
				EveryOne = configSettings.GetValue<bool>("Values Settings", "EveryOne", false);
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
			StoreModel = Keys.F11;
			NextStrength = Keys.F8;

			NeverRagdoll = true;
			STRENGTH_OFFSET = 10;
			DEFAULT_STRENGTH = 30;
			ThereIsNoStoppingMe = true;
			ThereIsNoStoppingMeAtAll = true;
		}

		private bool HasPedRecentlyDamagedPed(Ped aggressor, Ped victim)
		{
			bool damaged = (victim.HasBeenDamagedBy(aggressor) && Function.Call<bool>(Hash.HAS_PED_BEEN_DAMAGED_BY_WEAPON, victim, 0, 1));
			bool deadDamaged = (aggressor == player.Character ? victim.IsDead && aggressor.IsTouching(victim) && Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_HIT_PED, player) == 0 : false);

			return (damaged);
		}


		private int GetTakedownTime(Entity weaponEnt)
		{
			if (weaponEnt == null)
				return 550;
			else if (weaponEnt.Model.ToString() == "0x89D650BF"/*Couteau de base*/)
				return 550;
			else if (weaponEnt.Model.ToString() == "0x44973FE6"/*WeaponHash.Bottle*/)
				return 370;
			else if (weaponEnt.Model.ToString() == "0x62954071"/*WeaponHash.BattleAxe*/)
				return 370;
			else if (weaponEnt.Model.ToString() == "0x6EFFF508"/*Crowbar*/)
				return 370;
			else if (weaponEnt.Model.ToString() == "0x1F242A3"/*bat de baseball*/)
				return 370;
			else if (weaponEnt.Model.ToString() == "0x9E8C3644"/*baton de police*/)
				return 370;
			else if (weaponEnt.Model.ToString() == "0xDD6AE86A"/*Club de golf*/)
				return 370;
			else if (weaponEnt.Model.ToString() == "0xB32BE614"/*Poing americain*/)
				return 370;
			else if (weaponEnt.Model.ToString() == "0x99D81D79"/*Machete*/)
				return 370;
			else if (weaponEnt.Model.ToString() == "0x3D22723"/*Hammer*/)
				return 370;
			else if (weaponEnt.Model.ToString() == "0x23DD6B9D"/*Daguer*/)
				return 370;
			else if (weaponEnt.Model.ToString() == "0x87CEDC90"/*Flashlight*/)
				return 370;
			else if (weaponEnt.Model.ToString() == "0xC68D1A60"/*Switchblade*/)
				return 370;
			else if (weaponEnt.Model.ToString() == "0x215844F3")
				return 400;
			return 550;
		}

		private bool IsTouching(Entity entOne, Entity entTwo)
		{
			if (entTwo.Health <= 0)
				entTwo.Health = 1;
			Function.Call(Hash.SET_ENTITY_CAN_BE_DAMAGED, entTwo, true);
			float offSet = 0;
			Ped takedownPed = null;
			takedownPed = Function.Call<Ped>(Hash.GET_PED_INDEX_FROM_ENTITY_INDEX, entTwo);
			if (takedownPed != null && takedownPed.WasKilledByTakedown)
			{
				Entity weaponEnt = GetCurrentMeleeWeapon(takedownPed);
				Wait(GetTakedownTime(weaponEnt));
				return true;
			}

			if (entOne == player.Character)
			{
				offSet = Function.Call<bool>(Hash.IS_ENTITY_A_PED , entTwo) ? 0.0F : Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, entTwo) ? 0.01F : 0.01F;
				List<Vector3> bonePositions = new List<Vector3>();
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Foot));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Foot));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger00));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger01));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger02));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger10));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger11));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger12));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger20));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger21));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger22));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger30));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger31));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger32));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger40));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger41));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Finger42));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger00));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger01));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger02));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger10));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger11));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger12));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger20));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger21));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger22));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger30));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger31));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger32));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger40));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger41));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Finger42));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_R_Toe0));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.SKEL_L_Toe0));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.IK_R_Foot));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.PH_R_Foot));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.MH_R_Elbow));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.PH_L_Hand));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.IK_L_Hand));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.PH_L_Hand));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.PH_L_Hand));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.PH_L_Hand));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.PH_L_Hand));
				bonePositions.Add(player.Character.GetBoneCoord(Bone.PH_L_Hand));
			
				foreach(Vector3 pos in bonePositions)
				{
					if (World.GetDistance(pos, entTwo.Position) <= offSet)
						return true;
				}
			}
			else
			{
				offSet = Function.Call<bool>(Hash.IS_ENTITY_A_PED , entTwo) ? 0.4F : Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, entTwo) ? (Function.Call<bool>(Hash.IS_BIG_VEHICLE, (Vehicle)entTwo) ? 10.0F : 3.0F) : 0.4F;
				if (World.GetDistance(entOne.Position, entTwo.Position) <= offSet)
					return true;
			}
			return entTwo.IsNearEntity(entOne, new Vector3(offSet, offSet, offSet)) || entTwo.HasBeenDamagedBy(Game.Player.Character);
		}

		private bool CheckForEntityToThrow()
		{
			if (weaponEntity == null || weaponEntity.Position == Vector3.Zero)
				return false;
			Prop ent = null;
			bool keepChecking = true;
			Prop[] ents = World.GetNearbyProps(weaponEntity.Position, 4.0F);
			if (ents.Count() == 0)
				return false;
			foreach (Prop cur in ents)
			{
				if (cur != weaponEntity && IsTouching((Entity)weaponEntity, cur))
				{
					ent = cur;
					break;
				}
			}
			if (ent == null)
				return false;
			Function.Call(Hash.SET_ENTITY_DYNAMIC, player.Character, false);
			ent.ApplyForce(((savedCurDirection == Vector3.Zero) ? Game.Player.Character.ForwardVector : savedCurDirection) * currentStrength);
			savedCurDirection = Vector3.Zero;
			Function.Call(Hash.SET_ENTITY_DYNAMIC, player.Character, true);
			Function.Call(Hash.CLEAR_ENTITY_LAST_DAMAGE_ENTITY, ent);
			Function.Call(Hash.CLEAR_ENTITY_LAST_WEAPON_DAMAGE, ent);
			keepChecking = false;
			return keepChecking;
		}

		//I know i found these natives somewhere, but unfortunatly i can't seem to remember where, and i can't find it with google... I think it is thanks to @stillthere though. So thank you @stillthere for that !
		private Entity GetCurrentMeleeWeapon(Ped ped)
		{
			Entity weapon = null;
			if ((weapon = Function.Call<Entity>(Hash.GET_CURRENT_PED_WEAPON_ENTITY_INDEX, player.Character)) == null)
				weapon = player.Character;
			return ((!Function.Call<bool>(Hash.IS_PED_ARMED, ped, 7) || Function.Call<bool>(Hash.IS_PED_ARMED, ped, 1)) ? weapon : null);
		}

		private bool CheckVehicleTouch(bool punch, float distance)
		{
			if (punch)
			{
				Vehicle[] vehs = World.GetNearbyVehicles(Game.Player.Character.Position, 16.0F);
				if (vehs == null || vehs.Count() == 0)
					return false;
				foreach (Vehicle veh in vehs)
				{
					if (Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_WEAPON, veh, 0, 1) && veh.HasBeenDamagedBy(Game.Player.Character))
					{
						veh.ApplyForce(((curDirection == Vector3.Zero) ? Game.Player.Character.ForwardVector : curDirection) * currentStrength);
						Function.Call(Hash.CLEAR_ENTITY_LAST_DAMAGE_ENTITY, veh);
						Function.Call(Hash.CLEAR_ENTITY_LAST_WEAPON_DAMAGE, veh);
						return true;
					}
				}
			}
			else
			{
				Vehicle veh = World.GetClosestVehicle(Game.Player.Character.Position, 100.0F);
				if (veh != null && veh.IsTouching(Game.Player.Character.Model))
					veh.ApplyForce(Game.Player.Character.ForwardVector * (Game.Player.Character.IsSprinting ? 20 : Game.Player.Character.IsRunning ? 10 : 5));
			}
			return false;
		}

		private bool checkPedTouch()
		{
			Ped testPed = Function.Call<Ped>(Hash.GET_MELEE_TARGET_FOR_PED, Game.Player.Character);
			if (testPed != null)
			{
				if (testPed.Health <= 0)
					testPed.Health = 1;
				if (GetWeaponHitDirection(weaponEntity, Game.Player.Character) != Vector3.Zero || IsTouching(weaponEntity, testPed))
				{
					if (testPed.IsInMeleeCombat)
						Wait(20);
					Function.Call((Hash)0xAE99FB955581844A, testPed, 10, 10, 0, true, true, false); // small Ragdoll mode
					testPed.ApplyForce(((curDirection == Vector3.Zero) ? Game.Player.Character.ForwardVector : curDirection) * currentStrength);
					Function.Call(Hash.CLEAR_ENTITY_LAST_DAMAGE_ENTITY, testPed);
					Function.Call(Hash.CLEAR_ENTITY_LAST_WEAPON_DAMAGE, testPed);
					Function.Call(Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, testPed);
					return true;
				}
			}
			return false;
		}

		private void EveryOneHasPowerPunch()
		{
			Ped[] peds = World.GetNearbyPeds(Game.Player.Character.Position, 100.0F);
			if (peds == null)
				return;
			foreach (Ped curPed in peds)
			{
				if (Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ANY_PED, curPed) && Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_WEAPON, curPed, 0, 1))
				{

					if (!curPed.HasBeenDamagedBy(curPed) && !curPed.HasBeenDamagedBy(Game.Player.Character))
					{
						if (curPed.IsInMeleeCombat)
							Wait(20);
						Function.Call((Hash)0xAE99FB955581844A, curPed, 10, 10, 0, true, true, false); // small Ragdoll mode					
						curPed.ApplyForce((curPed.ForwardVector * -1) * currentStrength);
					}
					Function.Call(Hash.CLEAR_ENTITY_LAST_DAMAGE_ENTITY, curPed);
					Function.Call(Hash.CLEAR_ENTITY_LAST_WEAPON_DAMAGE, curPed);
					Function.Call(Hash.CLEAR_PED_LAST_WEAPON_DAMAGE, curPed);
					return;
				}
			}
		}

		private bool CheckEntityTouch()
		{
			Entity[] vehs = null;
			Entity veh = null;
			vehs = World.GetNearbyEntities(Game.Player.Character.Position, 1.0F);
			if (vehs != null && vehs.Length > 1)
				veh = vehs[1];
			if (veh != null && veh.IsTouching(Game.Player.Character.Model))
			{
				if (Function.Call<bool>(Hash.IS_ENTITY_A_PED, veh))
					Function.Call((Hash)0xAE99FB955581844A, veh, 10, 10, 0, true, true, false); // small Ragdoll mode
				veh.ApplyForce(Game.Player.Character.ForwardVector * (Game.Player.Character.IsSprinting ? 20 : Game.Player.Character.IsRunning ? 10 : 5));
				return true;
			}
			return false;
		}

		private Vector3 GetWeaponHitDirection(Entity weaponEnt, Ped targetPed)
		{
			OutputArgument arg = new OutputArgument();
			Function.Call<bool>(Hash.GET_PED_LAST_WEAPON_IMPACT_COORD, targetPed, arg);
			Vector3 toSub = arg.GetResult<Vector3>();
			return toSub;
		}

		private void OnTick(object sender, EventArgs e)
		{
			if (activated)
			{
				if (Game.Player.Character.Model.ToString() != curModel)
					TryLoadChar("scripts//SimplePowerPunchSettings//ModelBank.txt");
				if (player == null)
					player = Game.Player;
				if ((weaponEntity = GetCurrentMeleeWeapon(player.Character)) == null)
					return;
				if (Game.IsControlPressed(2, GTA.Control.Attack) || Game.Player.Character.IsInMeleeCombat || Game.Player.Character.IsRagdoll)
					shouldCheck = true;
				else if (Game.IsControlPressed(2, GTA.Control.Context) && shouldCheck != true && WhereYouAimIsWhereYouThrow)
					curDirection = GameplayCamera.Direction;
				if (shouldCheck)
				{
					if (!CheckVehicleTouch(true, (dist += 0.1F)) && !checkPedTouch())
					{
						if (curDirection != Vector3.Zero)
							savedCurDirection = curDirection;
						shouldCheck = CheckForEntityToThrow();
					}
					else
					{
						curDirection = Vector3.Zero;
						shouldCheck = false;
					}
				}
				else
				{
					if (ThereIsNoStoppingMe && !Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, Game.Player.Character, 160))
						CheckVehicleTouch(false, dist);
					if (ThereIsNoStoppingMeAtAll && !Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, Game.Player.Character, 160))
						CheckEntityTouch();
				}
				if (EveryOne)
				{	
					EveryOneHasPowerPunch();
				}
				Game.Player.Character.CanRagdoll = !NeverRagdoll;
			}

		}

		private void TrySaveChar(String fileName)
		{
			try
			{
				saveSettings = ScriptSettings.Load(fileName);
				saveSettings.SetValue<int>("Characters", Game.Player.Character.Model.ToString(), currentStrength);
				curModel = Game.Player.Character.Model.ToString();
				UI.Notify("Model " + curModel + " saved with strength " + currentStrength.ToString());
				saveSettings.Save();
			}
			catch (Exception e)
			{
				UI.Notify("~r~Error~w~: " + fileName.ToString() + " failed to load : " + e.ToString() + ".");
			}
		}

		private void TryLoadChar(String fileName)
		{
			try
			{
				saveSettings = ScriptSettings.Load(fileName);
				currentStrength = saveSettings.GetValue<int>("Characters", Game.Player.Character.Model.ToString(), -1);
				curModel = Game.Player.Character.Model.ToString();
				if (currentStrength == -1)
				{
					currentStrength = DEFAULT_STRENGTH;
					UI.Notify("Couldn't find value for " + curModel);
				}
				else
					UI.Notify("Character was found. Loaded with strength " + currentStrength.ToString());
			}
			catch(Exception e)
			{
				UI.Notify("~r~Error~w~: " + fileName.ToString() + " failed to load : " + e.ToString() + ".");
			}
		}

		private void ClearAll()
		{
			if (Game.Player.Character.CanRagdoll != ShouldBeRagdoll)
				Game.Player.Character.CanRagdoll = ShouldBeRagdoll;
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == StartMode)
			{
				if (!activated)
					ShouldBeRagdoll = Game.Player.Character.CanRagdoll;
				activated = !activated;
				if (!activated)
					ClearAll();
				UI.Notify("V3.0 SimplePowerPunch is " + ((activated) ? "on" : "off"));
			}   
			else if (activated && e.KeyCode == StoreModel && shouldSave)
			{
				TrySaveChar("scripts//SimplePowerPunchSettings//ModelBank.txt");
				shouldSave = false;
			}
			else if (activated && e.KeyCode == NextStrength && shouldIncrease)
			{
				currentStrength += STRENGTH_OFFSET;
				if (currentStrength > 2000)
					currentStrength = DEFAULT_STRENGTH;
				UI.ShowSubtitle("STRENGTH UP : Current strength is " + currentStrength.ToString(), 2000);
				shouldIncrease = false;
			}
			else if (activated && e.KeyCode == PrevStrength && shouldDecrease)
			{
				if (currentStrength - STRENGTH_OFFSET >= DEFAULT_STRENGTH)
					currentStrength -= STRENGTH_OFFSET;
				else
					currentStrength = 2000;
				UI.ShowSubtitle("STRENGTH DOWN : Current strength is " + currentStrength.ToString(), 2000);
				shouldDecrease = false;
			}
		}
		private void OnKeyUp(object sender, KeyEventArgs e)
		{
			if (activated && e.KeyCode == NextStrength)
			{
				shouldIncrease = true;
			}
			else if (activated && e.KeyCode == PrevStrength)
			{
				shouldDecrease = true;
			}
			else if (activated && e.KeyCode == StoreModel)
			{
				shouldSave = true;
			}
		}
	}
}