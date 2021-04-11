using GTA;
using GTA.Native;
using GTA.Math;
using System;
using System.IO;
using System.Windows.Forms;

//using System.Threading;

namespace HideMinimap {
	public class HideMinimapMod : Script {
		    private bool visible = true;
		    private bool showInCar = false;
		    private bool setToVisible = true;
		    private bool showPoliceBlip = false; // Set to false if you don't want police blip to appear when in "show minimap inside vehicle" mode.
		    private bool isPoliceBlipShown = true;
		    private bool policeBlipsChecked = false;
		    private bool showWhileUsingPhone =  true; // Set to false if you don't want the minimap to be displayed when in cellphone mode;

		    private int holdingKey = 0;
		    private int FPS = 60; // Change to your fps rate (Doesn't currently do anything)

		    private GTA.Player player = null;


		    public HideMinimapMod()
		    {
		    	Tick += OnTick;
        		KeyDown += OnKeyDown;
        		KeyUp += OnKeyUp;
    		}

    		private void OnTick(object sender, EventArgs e)
    		{
    			if (Game.Player.WantedLevel > 0 && !policeBlipsChecked)
    			{
    				HidePoliceBlip();
    				policeBlipsChecked = true;
    			}
    			else if (Game.Player.WantedLevel == 0 && policeBlipsChecked)
    			{
    				policeBlipsChecked = false;
    			}
                if (!setToVisible)
                {

                    if (player != null && player.Character.IsInVehicle())
                        HideOrShow((showInCar || (PhoneIsUp() && showWhileUsingPhone)));
                    else if (PhoneIsUp())
                        HideOrShow(showWhileUsingPhone);
                    else if (!IsRadarHidden())
                        HideOrShow(false);
                }
                else if (IsRadarHidden())
                {
                    HideOrShow(true);
                }
    		}

    		public unsafe bool PhoneIsUp()
    		{
    			bool canbeseen = false;
    			float animTime;
    
    			if (player != null)
    			{
    				animTime = GetPhoneGestureAnimCurrentTime(player.Character);
                    canbeseen = (animTime != -1);
	    		}
    			return canbeseen;
    		}

		    public bool IsRadarHidden()
		    {
		    	return (Function.Call<bool>(Hash.IS_RADAR_HIDDEN));
		    }

		    public float GetPhoneGestureAnimCurrentTime(Ped ped)
		    {
		    	return (Function.Call<float>(Hash.GET_PHONE_GESTURE_ANIM_CURRENT_TIME, ped));
		    }

    		public void HideOrShow() // Old version. Was really annoying to use.
    		{
    			visible = !visible;
    			Function.Call(Hash.DISPLAY_RADAR, visible);
    		}

            public void HideOrShow(bool hideOrShow) // New version, much simpler to implement. "visible" isn't really useful.
            {
                visible = hideOrShow;
                Function.Call(Hash.DISPLAY_RADAR, hideOrShow);
            }

    		public void HidePoliceBlip()
    		{
    			Function.Call(Hash.SET_POLICE_RADAR_BLIPS, showPoliceBlip);
    			isPoliceBlipShown = showPoliceBlip;
    		}

    		void OnKeyDown(object sender, KeyEventArgs e)
		    {
		    	if (e.KeyCode == Keys.B) // Change Keys.B to Keys.YourKey
		    	{
		    		if (holdingKey == 10)
		    		{
			    		showInCar = !showInCar;
			    		UI.Notify(string.Format("Showing Minimap inside vehicles : {0}", (showInCar ? "on" : "off")));
   						HidePoliceBlip();
			    		holdingKey++;
			    	}
			    	else if (holdingKey < 10)
			    		holdingKey++;
		    	}

		    }

		    void OnKeyUp(object sender, KeyEventArgs e)
		    {
		    	if (e.KeyCode == Keys.B)
		    	{
		    		if (player == null)
		    			player = Game.Player;
		    		if (holdingKey <= 10)
		    		{
			    		HideOrShow();
			    		setToVisible = !setToVisible;
			    	}
		    		holdingKey = 0;
		    	}
		    }
	} 
}
