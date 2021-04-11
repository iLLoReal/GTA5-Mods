using GTA;
using GTA.Native;
using System;
using System.Windows.Forms;


namespace LeaveEngineRunning {
	public class LeaveEngineRunningMod : Script {
			GTA.Player player = null;
			bool shouldTurnEngineOff = false;
			int holdingKey = 0;
			int holdingKeyEngine = 0;
			bool hasEnteredVehicle = false;
			bool engineOffMan = false;

			public LeaveEngineRunningMod()
		    {
        		KeyDown += OnKeyDown;
        		KeyUp += OnKeyUp;
        		Tick += OnTick;
    		}
			private void OnTick(object sender, EventArgs e)
			{
				if (player != null)
				{
					if (player.Character.IsInVehicle())
					{
						if (!player.Character.IsSittingInVehicle())
						{
							if (hasEnteredVehicle == true)
							{
								if (!shouldTurnEngineOff && player.Character.CurrentVehicle.EngineRunning == false && !engineOffMan)
									player.Character.CurrentVehicle.EngineRunning = true;
								else if (shouldTurnEngineOff && player.Character.CurrentVehicle.EngineRunning == true)
									Function.Call(Hash.SET_VEHICLE_ENGINE_ON, player.Character.CurrentVehicle, false, false);
							}
						}
						else if (player.Character == player.Character.CurrentVehicle.Driver)
						{
							hasEnteredVehicle = true;
							if (!engineOffMan || Game.IsControlPressed(2, GTA.Control.VehicleAccelerate))
							{
				    			Function.Call(Hash.SET_VEHICLE_ENGINE_ON, player.Character.CurrentVehicle, true, false, true);
				    			if (engineOffMan)
				    				engineOffMan = false;
				    		}
						}
					}
					else if (shouldTurnEngineOff == true || engineOffMan == true)
					{
						shouldTurnEngineOff = false;
						hasEnteredVehicle = false;
						engineOffMan = false;
					}
				}
			}
    		void OnKeyDown(object sender, KeyEventArgs e)
		    {
		    	if (Game.IsControlPressed(2, GTA.Control.MeleeAttack1))
		    	{
		    		if (player == null)
		    		{
		    			player = Game.Player;
		    		}
		    		if (player.Character.IsInVehicle() && 
		    			player.Character.IsSittingInVehicle() && 
		    			player.Character == player.Character.CurrentVehicle.Driver)
		    		{
		    			if (holdingKeyEngine == 2)
		    			{
			    			engineOffMan = player.Character.CurrentVehicle.EngineRunning;
			    			Function.Call(Hash.SET_VEHICLE_ENGINE_ON, player.Character.CurrentVehicle, (engineOffMan ? false : true), false, true);
			    			holdingKeyEngine += 1;
			    		}
			    		else if (holdingKeyEngine < 2)
			    		{
			    			holdingKeyEngine++;
			    		}
		    		}
		    	}
		    	else if (Game.IsControlPressed(2, GTA.Control.VehicleExit)) // Change Keys.B to Keys.YourKey
		    	{
		    		if (player == null)
		    		{
		    			player = Game.Player;
		    		}
		    		if (holdingKey == 2 && player.Character.IsInVehicle())
		    		{
		    			shouldTurnEngineOff = true;			    		
			    	}
			    	else if (holdingKey < 2 && player.Character.IsInVehicle())
			    		holdingKey++;
		    	}
		    }
		    void OnKeyUp(object sender, KeyEventArgs e)
		    {
		    	if (Game.IsControlJustReleased(2, GTA.Control.VehicleExit))
		    	{
	    			holdingKey = 0;
		    	}
		    	else if (Game.IsControlJustReleased(2, GTA.Control.MeleeAttack1))
		    	{
		    		holdingKeyEngine = 0;
		    	}
		    }
		}
}