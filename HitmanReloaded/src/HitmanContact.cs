using GTA;
using GTA.Math;
using System;
using GTA.Native;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using iFruitAddon2;


namespace Hitman
{
    class iFruitHitman : Script
    {
        public const int MESSAGE_LIST = 6;
        public const int MESSAGE_VIEW = 7;

        CustomiFruit phone;
        iFruitContact contact;
        bool startMission;
        int _mScriptHash;

        private String name;
        private int id;
        public iFruitHitman()
        {
            InitPhone();
            name = "Middleman";
            contact = new iFruitContact(name);
            id = contact.Index;
            contact.Answered += Contact_Answered;
            contact.DialTimeout = 8000;
            contact.Active = true;
            contact.Icon = new ContactIcon("char_hunter");
            phone.Contacts.Add(contact);
            startMission = false;
            _mScriptHash = Function.Call<int>(Hash.GET_HASH_KEY, "cellphone_flashhand");
        }

        private void InitPhone()
        {
            phone = new CustomiFruit()
            {
                CenterButtonColor = System.Drawing.Color.Orange,
                LeftButtonColor = System.Drawing.Color.LimeGreen,
                RightButtonColor = System.Drawing.Color.Purple,
                CenterButtonIcon = SoftKeyIcon.Fire,
                LeftButtonIcon = SoftKeyIcon.Police,
                RightButtonIcon = SoftKeyIcon.Website
            };
            phone.SetWallpaper(Wallpaper.BadgerDefault);
        }
        public bool IsContactActive()
        {
            return contact.Active;
        }
        public bool HasMissionStarted()
        {
            return startMission;
        }

        public void StopMissions()
        {
            Hitman.HitmanMission.hitmanNotify("Glad to count you in, don't hesitate to call back ! We ALWAYS have pizzas awaiting delivery.");
            startMission = false;
        }
        public void Contact_Answered(iFruitContact contact)
        {
            if (contact == this.contact)
            {
                this.startMission = !this.startMission;
                if (startMission)
                    Hitman.HitmanMission.hitmanNotify("Hey there, I got your call. Clients are always hungry !");
                else
                    Hitman.HitmanMission.hitmanNotify("Okay. Clients will have to go get their pizzas themselves, i guess...");
                phone.Close();
            }
        }
        internal int GetSelectedIndex(int handle)
        {
            Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION, handle, "GET_CURRENT_SELECTION");
            int num = Function.Call<int>(Hash._POP_SCALEFORM_MOVIE_FUNCTION);
            while (!Function.Call<bool>(Hash._0x768FF8961BA904D6, num))
                Script.Wait(0);
            int data = Function.Call<int>(Hash._0x2DE7EFA66B906036, num);
            return data;
        }
        public void Update()
        {
            phone.Update();
        }
        protected override void Dispose(bool A_0)
        {
            phone.Contacts.ForEach(x => x.EndCall());
            base.Dispose(A_0);
        }
    }
}