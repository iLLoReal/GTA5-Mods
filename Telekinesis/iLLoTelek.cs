using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace HandleThings {

	public class HandleThingsMod : Script
	{
		/*KEYS THAT YOU CAN CHANGE*/

		// this mod is too little to require a .ini file in my opinion.
		/*KEYS*/
		Keys StartMod = Keys.F10; // activate / deactivate the mod. Change to Keys.YourKey (Keys.F11, Keys.F9, Keys. ...)
		Keys LockEntity = Keys.E; // Here, for instance, change to Keys.Y if you want the Y key to lock an entity.

		/*SETTINGS*/		
		int strength = 800; // The strength at which you can throw entities
		bool activated = false; // set this to true to have the mod activated by default
		bool ThrowWhenRelease = true; // set this to true if you want to throw the ped when released
		bool ThrowWhenReleaseNoMAtterWhat = false; // set this to true if you want to throw the ped when released straigth forward 

		/*END OF KEYS THAT YOU CAN CHANGE*/

		Entity ent = null;
		float curDistance = 0.0F;
		float distance = 100.0F; // Distance at which you get an entity. You can modify this as long as it isn't negative or > 1000;
		Vector3 curDirection = Vector3.Zero;
		Vector3 currentRayCastPosition = Vector3.Zero;
		Vector3 previousPedPos = Vector3.Zero;
		Vector3 currPedPos = Vector3.Zero;


		public HandleThingsMod()
		{
			Tick += OnTick;
			KeyDown += OnKeyDown;
			KeyUp += OnKeyUp;
		}

		// Not currently used. Couldn't get it to work with new position and any model.
		private bool WouldBeOccluded(Entity aroundEnt, Vector3 pos)
		{
			return Function.Call<bool>(Hash.WOULD_ENTITY_BE_OCCLUDED, aroundEnt.Model.Hash, pos.X, pos.Y,  pos.Z, true)
				   || Function.Call<bool>(Hash.WOULD_ENTITY_BE_OCCLUDED, aroundEnt.Model.Hash, pos.X, pos.Y + 1,  pos.Z, true)
				   || Function.Call<bool>(Hash.WOULD_ENTITY_BE_OCCLUDED, aroundEnt.Model.Hash, pos.X + 1, pos.Y,  pos.Z, true);
		}

		//Not currently used. It did work in some cases, but not all, so it wasn't worth it in the end.
		private bool IsCollidingWithVector(Entity aroundEnt, Vector3 newpos)
		{
			if (aroundEnt == Game.Player.Character)
				return false;
			OutputArgument minVec = new OutputArgument();
			OutputArgument maxVec = new OutputArgument();

			Function.Call(Hash.GET_MODEL_DIMENSIONS, aroundEnt.Model.Hash, minVec, maxVec);
			Vector3 minVecRes = minVec.GetResult<Vector3>();
			Vector3 maxVecRes = maxVec.GetResult<Vector3>();
			Vector3 dimensionsAround = Vector3.Subtract(maxVecRes, minVecRes);

			Function.Call(Hash.GET_MODEL_DIMENSIONS, ent.Model.Hash, minVec, maxVec);
			minVecRes = minVec.GetResult<Vector3>();
			maxVecRes = maxVec.GetResult<Vector3>();
			Vector3 dimensionsEnt = Vector3.Subtract(maxVecRes, minVecRes);

			List<Vector3> dimEnt = new List<Vector3>();
			List<Vector3> dimAroundEnt = new List<Vector3>();
			dimEnt.Add(new Vector3(ent.Position.X + (dimensionsEnt.X / 2), ent.Position.Y + (dimensionsEnt.Y / 2), ent.Position.Z)); // up-right
			dimEnt.Add(new Vector3(ent.Position.X, ent.Position.Y + (dimensionsEnt.Y / 2), ent.Position.Z)); // up
			dimEnt.Add(new Vector3(ent.Position.X - (dimensionsEnt.X / 2), ent.Position.Y + (dimensionsEnt.Y / 2), ent.Position.Z)); // up-left
			dimEnt.Add(new Vector3(ent.Position.X - (dimensionsEnt.X / 2), ent.Position.Y, ent.Position.Z)); // left
			dimEnt.Add(new Vector3(ent.Position.X - (dimensionsEnt.X / 2), ent.Position.Y - (dimensionsEnt.Y / 2), ent.Position.Z)); // down-left
			dimEnt.Add(new Vector3(ent.Position.X, ent.Position.Y - (dimensionsEnt.Y / 2), ent.Position.Z)); // down
			dimEnt.Add(new Vector3(ent.Position.X + (dimensionsEnt.X / 2), ent.Position.Y - (dimensionsEnt.Y / 2), ent.Position.Z)); // down-right
			dimEnt.Add(new Vector3(ent.Position.X + (dimensionsEnt.X / 2), ent.Position.Y, ent.Position.Z)); // right

			dimAroundEnt.Add(new Vector3(aroundEnt.Position.X + (dimensionsAround.X / 2), aroundEnt.Position.Y + (dimensionsAround.Y / 2), aroundEnt.Position.Z)); // up-right
			dimAroundEnt.Add(new Vector3(aroundEnt.Position.X, aroundEnt.Position.Y + (dimensionsAround.Y / 2), aroundEnt.Position.Z)); // up
			dimAroundEnt.Add(new Vector3(aroundEnt.Position.X - (dimensionsAround.X / 2), aroundEnt.Position.Y + (dimensionsAround.Y / 2), aroundEnt.Position.Z)); // up-left
			dimAroundEnt.Add(new Vector3(aroundEnt.Position.X - (dimensionsAround.X / 2), aroundEnt.Position.Y, aroundEnt.Position.Z)); // left
			dimAroundEnt.Add(new Vector3(aroundEnt.Position.X - (dimensionsAround.X / 2), aroundEnt.Position.Y - (dimensionsAround.Y / 2), aroundEnt.Position.Z)); // down-left
			dimAroundEnt.Add(new Vector3(aroundEnt.Position.X, aroundEnt.Position.Y - (dimensionsAround.Y / 2), aroundEnt.Position.Z)); // down
			dimAroundEnt.Add(new Vector3(aroundEnt.Position.X + (dimensionsAround.X / 2), aroundEnt.Position.Y - (dimensionsAround.Y / 2), aroundEnt.Position.Z)); // down-right
			dimAroundEnt.Add(new Vector3(aroundEnt.Position.X + (dimensionsAround.X / 2), aroundEnt.Position.Y, aroundEnt.Position.Z)); // right

			float offSet = 1.0F;
			foreach (Vector3 cur in dimEnt)
			{
				foreach (Vector3 curAround in dimAroundEnt)
				{
					if (World.GetDistance(curAround, cur) <= offSet)
						return true;
				}
			}

			return false;
/*			bool AroundIsReference = dimensionsAround.Y > dimensionsEnt.Y;

			float offSet;
			Vector3 borders = Vector3.Zero;
			if (Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, aroundEnt))
			{
				borders = aroundEnt.GetBoneCoord("chassis_dummy");
				offSet = 3.0F;
			}
			else if (Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, ent))
			{
				borders = ent.GetBoneCoord("chassis_dummy");
				offSet = 3.0F;
			}
			else
			{
				if (Function.Call<bool>(Hash.IS_ENTITY_A_PED, aroundEnt))
					borders = aroundEnt.GetBoneCoord("SKEL_ROOT");
				else if (Function.Call<bool>(Hash.IS_ENTITY_A_PED, ent))
					borders = ent.GetBoneCoord("SKEL_ROOT");
				offSet = 2.0F;
			}
			if (borders == Vector3.Zero)
				UI.ShowSubtitle("Couldn't find chassis bone");
			if (Function.Call<bool>(Hash.IS_ENTITY_AT_COORD, aroundEnt, borders.X, borders.Y, borders.Z, offSet, offSet, offSet, false, true, false) /*WouldBeOccluded(aroundEnt)/*Vector3.Distance(aroundEnt.Position, newpos) <= offSet)
			{
				UI.ShowSubtitle("He is at coords");
				if (aroundEnt != ent)
				{
					if (Function.Call<bool>(Hash.IS_ENTITY_A_PED, aroundEnt) && !Function.Call<bool>(Hash.IS_PED_RAGDOLL, Function.Call<Ped>(Hash.GET_PED_INDEX_FROM_ENTITY_INDEX, aroundEnt.Handle)))
						Function.Call((Hash)0xAE99FB955581844A, aroundEnt, 10, 10, 0, true, true, false); // small Ragdoll mode
					if (curDirection == Vector3.Zero)
						curDirection = Vector3.WorldUp;
		    		aroundEnt.ApplyForce(curDirection);
		    	}
	    	}*/
		}

		private float GetDimensionsMaxValue(Model entModel)
		{
			OutputArgument minVec = new OutputArgument();
			OutputArgument maxVec = new OutputArgument();

			Function.Call(Hash.GET_MODEL_DIMENSIONS, entModel.Hash, minVec, maxVec);
			Vector3 dimensionsAround = Vector3.Subtract(maxVec.GetResult<Vector3>(), minVec.GetResult<Vector3>());
			return GetMaxValue(dimensionsAround);
		}

		private float GetMaxValue(Vector3 dimensionsAround)
		{
			float max = 0;
			if (dimensionsAround.X > dimensionsAround.Y && dimensionsAround.X > dimensionsAround.Z)
				max = dimensionsAround.X;
			else if (dimensionsAround.Y > dimensionsAround.X && dimensionsAround.Y > dimensionsAround.Z)
				max = dimensionsAround.Y;
			else if (dimensionsAround.Z > dimensionsAround.X && dimensionsAround.Z > dimensionsAround.Y)
				max = dimensionsAround.Z;
			else
				max = dimensionsAround.Y;
			return max;
		}

		private bool IsEntityThere(Vector3 newpos)
		{
			Ped[] allThePeds = World.GetNearbyPeds(newpos, 15);
			foreach (Ped cur in allThePeds)
			{
				if (cur != Game.Player.Character && cur != ent && !cur.IsInVehicle() && !cur.IsDead)
				{
					if (Function.Call<bool>(Hash.IS_ENTITY_AN_OBJECT, ent))
					{
						if (ent.IsTouching(cur.Model))
						{
							if (!cur.IsRagdoll || Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, ent)) 
							{
								Function.Call((Hash)0xAE99FB955581844A, cur, 100, 100, 2, true, true, false); // bump Ragdoll mode
								Function.Call((Hash)0xAE99FB955581844A, cur, 10, 10, 0, true, true, false); // true Ragdoll mode							
							}							
							if ((curDirection) != Vector3.Zero)
								cur.ApplyForce((cur.Position - ent.Position) * (World.GetDistance(currPedPos, previousPedPos)) * 25);
							else
								cur.ApplyForce(cur.Position - ent.Position * 2);
						}
					}
					else
					{
						float curDim = GetDimensionsMaxValue(cur.Model);
						float entDim = GetDimensionsMaxValue(ent.Model);
						float maxDim = curDim > entDim ? curDim : entDim;					
						if (cur.IsInRangeOf(ent.Position, (Function.Call<bool>(Hash.IS_ENTITY_A_PED, ent) ? 1.5F : (maxDim + (Function.Call<bool>(Hash.IS_BIG_VEHICLE, ent) ? 10.0F : 1.0F)))) || ent.IsTouching(cur.Model))
						{
							if (!cur.IsRagdoll && (World.GetDistance(currPedPos, previousPedPos) > 0.3F) || Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, ent)) 
							{
								Function.Call((Hash)0xAE99FB955581844A, cur, 100, 100, 2, true, true, false); // bump Ragdoll mode
								Function.Call((Hash)0xAE99FB955581844A, cur, 10, 10, 0, true, true, false); // true Ragdoll mode							
							}
							if ((curDirection) != Vector3.Zero)
								cur.ApplyForce((cur.Position - ent.Position) * (World.GetDistance(currPedPos, previousPedPos)) * 25);
							else
								cur.ApplyForce(cur.Position - ent.Position * 2);
							return Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, ent);
						}
					}
				}
			}

			Vehicle[] allTheVehicles = World.GetNearbyVehicles(newpos, 15);
			foreach (Vehicle cur in allTheVehicles)
			{
				if (cur != Game.Player.Character && cur != ent)
				{
					float curDim = GetDimensionsMaxValue(cur.Model);
					float entDim = GetDimensionsMaxValue(ent.Model);
					float maxDim = curDim > entDim ? curDim : entDim;
					if (cur.IsInRangeOf(newpos, maxDim + (Function.Call<bool>(Hash.IS_BIG_VEHICLE, cur) || Function.Call<bool>(Hash.IS_BIG_VEHICLE, ent) ? 10.0F : 1.0F))  || ent.IsTouching(cur.Model))
					{
						cur.ApplyForce(cur.Position - ent.Position);
						return !Function.Call<bool>(Hash.IS_ENTITY_STATIC, ent) || !Function.Call<bool>(Hash.IS_ENTITY_AN_OBJECT);
					}
				}
			}
			Prop[] allTheProps = World.GetNearbyProps(newpos, 15);
			foreach (Prop cur in allTheProps)
			{
				if (cur != ent)
				{
					if (ent.IsTouching(cur.Model))
					{
						if ((curDirection) != Vector3.Zero)
							cur.ApplyForce((cur.Position - ent.Position) * (World.GetDistance(currPedPos, previousPedPos)) * 25 * GetDimensionsMaxValue(ent.Model));
						else
							cur.ApplyForce(cur.Position - ent.Position * 2);						
						return false;
					}
				}
			}
			return false;
		}

		private void OnTick(object sender, EventArgs e)
		{
			if (activated && ent != null)
			{
				Vector3 newpos = Vector3.Zero;
				if (curDistance == 0.0F)
					curDistance = World.GetDistance(ent.Position, Game.Player.Character.Position);
				if ((newpos = GetRaycastPosition(curDistance)) == Vector3.Zero || newpos == Game.Player.Character.Position)
					newpos = Game.Player.Character.Position + (GameplayCamera.Direction * curDistance);
				previousPedPos = currPedPos;
				if (Function.Call<bool>(Hash.IS_ENTITY_A_PED, ent) || Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, ent))
					ent.Heading = Game.Player.Character.Heading;
				else
					ent.Rotation = new Vector3(Game.Player.Character.Rotation.X - 90.0F, Game.Player.Character.Rotation.Y, Game.Player.Character.Rotation.Z);
				if (World.GetGroundHeight(newpos) == 0)
					newpos = currPedPos;
				currPedPos = newpos;
				if (curDirection == Vector3.Zero)
					curDirection = currPedPos - previousPedPos;
				if (newpos != Vector3.Zero)
				{					
					if (!IsEntityThere(newpos))
						Function.Call(Hash.SET_ENTITY_COORDS, ent, newpos.X, newpos.Y, newpos.Z, false, false, false, false);
				}
			}
		}

		private Entity GetEntityFromRayCast(float distance, GTA.IntersectOptions type)
		{
			Entity returnedEntity = null;
			RaycastResult res = World.Raycast(Game.Player.Character.Position, GameplayCamera.Direction, distance, type, Game.Player.Character);
			returnedEntity = res.HitEntity;
			if (returnedEntity == null)
			{
				Ped pedEnt = null;
				Vehicle vehicleEnt = null;
				Vector3 coords = res.HitCoords == Vector3.Zero ? GameplayCamera.Direction * distance : res.HitCoords;
				Prop[] propEnts = World.GetNearbyProps(coords, 2.0F);
				Prop propEnt = null;
				if (propEnts.Count() > 0)
					propEnt = propEnts[0];
				if (type == GTA.IntersectOptions.Peds1 && ((pedEnt = World.GetClosestPed(coords, 4.0F)) != null && pedEnt != Game.Player.Character))
					returnedEntity = pedEnt;
				else if ((vehicleEnt = World.GetClosestVehicle(coords, 4.0F))!= null && vehicleEnt != Game.Player.Character.CurrentVehicle)
					returnedEntity = vehicleEnt;
				else if (propEnt != null && propEnt.Model != Game.Player.Character.Weapons.Current.Model)
					returnedEntity = propEnt;
    	   	}
    	   	if (returnedEntity != null && Function.Call<bool>(Hash.IS_ENTITY_STATIC, returnedEntity))
    	   	{
    	   		if (Function.Call<bool>(Hash.IS_ENTITY_A_PED, returnedEntity) || Function.Call<bool>(Hash.IS_ENTITY_A_PED, returnedEntity.GetEntityAttachedTo()))
    	   		{
    	   			Ped pedEnt = Function.Call<Ped>(Hash.GET_PED_INDEX_FROM_ENTITY_INDEX, returnedEntity);
    	   			pedEnt.Task.ClearAllImmediately();
    	   			Wait(20);
    	   		}
    	   		else if (!Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, returnedEntity))
    	   		{
	    	   		returnedEntity.ApplyForce(GameplayCamera.Direction * 100);
	    	   		//
	    	   		/*Prop fakeCollision = World.CreateProp(Function.Call<int>(Hash.GET_HASH_KEY, "prop_table_04_chr"), Game.Player.Character.ForwardVector.Around(2.0F), true, true);
	    	   		if (fakeCollision != null)
		    	   		fakeCollision.ApplyForce((returnedEntity.Position - fakeCollision.Position) * 200);
	    	   		Wait(10);
	    	   		*/
	    	   		if (Function.Call<bool>(Hash.IS_ENTITY_STATIC, returnedEntity) && (World.GetDistance(Game.Player.Character.Position, returnedEntity.Position)) > 2.0F)
		    	   		World.AddExplosion(returnedEntity.Position, ExplosionType.Grenade, 0.1F, 0.0F, false, true);
				}
    	   	}
    	   	return (Function.Call<bool>(Hash.IS_ENTITY_STATIC, returnedEntity) && !Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, returnedEntity) ? null : returnedEntity);
		}

		private Vector3 GetRaycastPosition(float distance)
		{
			RaycastResult res = World.Raycast(Game.Player.Character.Position, GameplayCamera.Direction, distance, GTA.IntersectOptions.Map, Game.Player.Character);
			if (res.HitEntity != null && res.HitEntity == ent)
				return ent.Position;
    	   	return res.HitCoords;
		}

		private void clearAll()
		{
			ent = null;
		}

	    private void OnKeyDown(object sender, KeyEventArgs e)
	    {
	    	if (activated && e.KeyCode == LockEntity)
	    	{
	    		if (ent == null)
	    		{
	    			ent = ((ent = GetEntityFromRayCast(distance, GTA.IntersectOptions.Peds1)) != null ? ent : (ent = GetEntityFromRayCast(distance, GTA.IntersectOptions.Unk2)) != null ? ent : (ent = GetEntityFromRayCast(distance, GTA.IntersectOptions.Everything)) != null ? ent : (ent = GetEntityFromRayCast(distance, GTA.IntersectOptions.Unk2)));
	    			if (ent != null)
	    			{
		    	   		ent.IsInvincible = true;
		    	   	}

	    		}
	       }
	    }
	    private void OnKeyUp(object sender, KeyEventArgs e)
	    {
	    	if (e.KeyCode == StartMod)
	    	{
	    		activated = !activated;
	    		if (!activated)
	    			clearAll();
	    		UI.Notify("Telekinesis " + (activated ? "on" : "off"));
	    	}
	    	if (activated && e.KeyCode == LockEntity)
	    	{
	    		if (ent != null)
	    		{
					Function.Call((Hash)0xAE99FB955581844A, ent, 10, 10, 2, true, true, false); // small Ragdoll mode
		    		if (ThrowWhenRelease)
		    		{
			    		Vector3 direction = currPedPos - previousPedPos;
	    				float releasedStrength = (World.GetDistance(currPedPos, previousPedPos) * strength) / World.GetDistance(currPedPos, Game.Player.Character.Position);
		    			if (ThrowWhenReleaseNoMAtterWhat)
		    			{
		    				if (releasedStrength < 10.0F)
		    					releasedStrength = strength;
		    				if (World.GetDistance(currPedPos, previousPedPos) < 10.0F)
		    					direction = GameplayCamera.Direction;
		    			}
		    			ent.ApplyForce(direction * releasedStrength);
		    		}
	    	   		ent.IsInvincible = false;
		    		ent = null;
		    		curDistance = 0.0F;
		    	}
	    	}
	    }
	}
}