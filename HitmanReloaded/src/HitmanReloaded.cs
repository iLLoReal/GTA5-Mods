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
using System.Runtime.InteropServices;


// https://pastebin.com/6mrYTdQv 
// pastebin.com/yLNWicUi
// https://pastebin.com/Bbj7ANpQ


//HAS_PED_BEEN_DAMAGED_BY_WEAPON(Ped ped, Hash weaponHash, int weaponType);
//


namespace Hitman
{
    public struct Data
    {
        public Hash WeaponHashValue { get; private set; }
        public String WeaponName { get; private set; }

        public Data(String weaponName, Hash weaponHash) : this()
        {
            WeaponHashValue = weaponHash;
            WeaponName = weaponName;
        }

    }

    public class HitmanMission : Script
    {

        private List<Vehicle> escortVehicles = new List<Vehicle>();
        private List<Ped> escortPeds = new List<Ped>();
        private Ped target = null;
        private Vector3 decidedPosition = Vector3.Zero;
        private List<Data> weapons = new List<Data>();
        private List<String> prevPosName = new List<String>();
        private List<String> vehicleList = null;

//        private String targetHash = "Free";
        private String requestedVehicle = "Free";
        private String requestedWeapon = "Free";

        private const int MAX_NEAR_DISTANCE = 8000;

        private int escortDifficulty = 0; //  0 == none, 1 == simple, 2 == medium, 3 == spicy.
        private int nearDistance = 600;
        private int bonusReward = 0;
        private int reward = 0;
        private int groupId = 0;

        private bool inVehicle = false; // Should your target be inside a vehicle or not. Escort will act accordingly.
        private bool missionComplete = true;
        private bool missionStarted = false;
        private bool LosePoliceNotified = false;
        private bool LeaveAreaNotified = false;
        private bool LoseAssailantsNotified = false;

        private void fillVehicleList() // Should only be called once . feel free to add the vehicles you'd like to see pop.
        {
            vehicleList = new List<String>();
            vehicleList.Add("Baller2");
            vehicleList.Add("Minivan");
            vehicleList.Add("Patriot");
            vehicleList.Add("Adder");
            vehicleList.Add("Akuma");
            vehicleList.Add("Burrito"); // same as mule
            vehicleList.Add("Infernus");
            vehicleList.Add("Manana");
            //        vehicleList.Add("Mule"); // Mule has a weird bug that npc will be stuck inside.
            //   vehicleList.Add("Rhino"); // disabled for testing purposes
        }

        private void fillWeaponsList() // Should only be called once . feel free to add the vehicles you'd like to see pop.
        {
            weapons.Add(new Data("WEAPON_UNARMED", (Hash)0xA2719263));
            weapons.Add(new Data("WEAPON_KNIFE", (Hash)0x99B507EA));
            weapons.Add(new Data("WEAPON_NIGHTSTICK", (Hash)0x678B81B1));
            weapons.Add(new Data("WEAPON_HAMMER", (Hash)0x4E875F73));
            weapons.Add(new Data("WEAPON_BAT", (Hash)0x958A4A8F));
            weapons.Add(new Data("WEAPON_GOLFCLUB", (Hash)0x440E4788));
            weapons.Add(new Data("WEAPON_CROWBAR", (Hash)0x84BD7BFD));
            weapons.Add(new Data("WEAPON_PISTOL", (Hash)0x1B06D571));
            weapons.Add(new Data("WEAPON_COMBATPISTOL", (Hash)0x5EF9FEC4));
            weapons.Add(new Data("WEAPON_APPISTOL", (Hash)0x22D8FE39));
            weapons.Add(new Data("WEAPON_PISTOL50", (Hash)0x99AEEB3B));
            weapons.Add(new Data("WEAPON_MICROSMG", (Hash)0x13532244));
            weapons.Add(new Data("WEAPON_SMG", (Hash)0x2BE6766B));
            weapons.Add(new Data("WEAPON_ASSAULTSMG", (Hash)0xEFE7E2DF));
            weapons.Add(new Data("WEAPON_ASSAULTRIFLE", (Hash)0xBFEFFF6D));
            weapons.Add(new Data("WEAPON_CARBINERIFLE", (Hash)0x83BF0278));
            weapons.Add(new Data("WEAPON_ADVANCEDRIFLE", (Hash)0xAF113F99));
            weapons.Add(new Data("WEAPON_MG", (Hash)0x9D07F764));
            weapons.Add(new Data("WEAPON_COMBATMG", (Hash)0x7FD62962));
            weapons.Add(new Data("WEAPON_PUMPSHOTGUN", (Hash)0x1D073A89));
            weapons.Add(new Data("WEAPON_SAWNOFFSHOTGUN", (Hash)0x7846A318));
            weapons.Add(new Data("WEAPON_ASSAULTSHOTGUN", (Hash)0xE284C527));
            weapons.Add(new Data("WEAPON_BULLPUPSHOTGUN", (Hash)0x9D61E50F));
            weapons.Add(new Data("WEAPON_STUNGUN", (Hash)0x3656C8C1));
            weapons.Add(new Data("WEAPON_SNIPERRIFLE", (Hash)0x05FC3C11));
            weapons.Add(new Data("WEAPON_HEAVYSNIPER", (Hash)0x0C472FE2));
            weapons.Add(new Data("WEAPON_REMOTESNIPER", (Hash)0x33058E22));
            weapons.Add(new Data("WEAPON_GRENADELAUNCHER", (Hash)0xA284510B));
            weapons.Add(new Data("WEAPON_GRENADELAUNCHER_SMOKE", (Hash)0x4DD2DC56));
            weapons.Add(new Data("WEAPON_RPG", (Hash)0xB1CA77B1));
            weapons.Add(new Data("WEAPON_STINGER", (Hash)0x687652CE));
            weapons.Add(new Data("WEAPON_MINIGUN", (Hash)0x42BF8A85));
            weapons.Add(new Data("WEAPON_GRENADE", (Hash)0x93E220BD));
            weapons.Add(new Data("WEAPON_STICKYBOMB", (Hash)0x2C3731D9));
            weapons.Add(new Data("WEAPON_SMOKEGRENADE", (Hash)0xFDBC8A50));
            weapons.Add(new Data("WEAPON_BZGAS", (Hash)0xA0973D5E));
            weapons.Add(new Data("WEAPON_MOLOTOV", (Hash)0x24B17070));
            weapons.Add(new Data("WEAPON_BALL", (Hash)0x23C9F95C));
            weapons.Add(new Data("WEAPON_FLARE", (Hash)0x497FACC3));
        }

        //Check if escortDifficulty < 5; if not set to 0
        //Same for nearDistance
        public HitmanMission(bool inVehicle, int escortDifficulty, int nearDistance)
        {
            Tick += OnTick;
            this.inVehicle = inVehicle;
            this.escortDifficulty = (escortDifficulty < 0 || escortDifficulty > 5) ? 0 : escortDifficulty;
            this.nearDistance = (nearDistance < 20 || nearDistance > MAX_NEAR_DISTANCE) ? 20 : nearDistance;
        }
        //https://stackoverflow.com/questions/7166307/passing-a-condition-as-a-parameter
        public HitmanMission(bool inVehicle, int escortDifficulty, int nearDistance, String requestedWeapon, int bonusReward)
        {
            Tick += OnTick;
            this.inVehicle = inVehicle;
            this.escortDifficulty = (escortDifficulty < 0 || escortDifficulty > 5) ? 0 : escortDifficulty;
            this.nearDistance = (nearDistance < 20 || nearDistance > MAX_NEAR_DISTANCE) ? 20 : nearDistance;
            this.requestedWeapon = requestedWeapon;
            this.bonusReward = (bonusReward >= 0 && bonusReward < 1000000000) ? bonusReward : 0;
        }

        public HitmanMission(bool inVehicle, int escortDifficulty, int nearDistance, String requestedWeapon, String requestedVehicle, int bonusReward)
        {
            Tick += OnTick;
            this.inVehicle = inVehicle;
            this.escortDifficulty = (escortDifficulty < 0 || escortDifficulty > 5) ? 0 : escortDifficulty;
            this.nearDistance = (nearDistance < 20 || nearDistance > MAX_NEAR_DISTANCE) ? 20 : nearDistance;
            this.requestedWeapon = requestedWeapon;
            this.requestedVehicle = requestedVehicle;
            this.bonusReward = (bonusReward >= 0 && bonusReward < 1000000000) ? bonusReward : 0;
        }
        /*
            public HitmanMission(bool inVehicle, int escortDifficulty, int nearDistance, String requestedVehicle, int bonusReward)
            {
                Tick += OnTick;
                this.inVehicle = inVehicle;
                this.escortDifficulty = (escortDifficulty > 0) ? escortDifficulty : 0;
                this.nearDistance = (nearDistance > 20) ? nearDistance : 20;
                this.requestedVehicle = requestedVehicle;
                this.bonusReward = (bonusReward >= 0) ? bonusReward : 0;
            }
        */
        public HitmanMission()
        {
            Tick += OnTick;
        }

        public bool SetMissionDifficulty(int difficulty) // Only call this one when mission isn't started
        {
            if (missionStarted)
                return false;
            escortDifficulty = difficulty;
            return true;
        }
        public bool SetInVehicle(bool inVehicle) // Only call this one when mission isn't started
        {
            if (missionStarted)
                return false;
            this.inVehicle = inVehicle;
            return true;
        }
        public bool GetMissionStarted()
        {
            return missionStarted;
        }

        public bool GetMissionComplete()
        {
            return missionComplete;
        }

        private bool SetupMission()
        {
            fillWeaponsList();
            if (inVehicle)
            {
                fillVehicleList();
                if (escortDifficulty > 0)
                {
                    escortVehicles = new List<Vehicle>();
                    escortPeds = new List<Ped>();

                }
            }
            if (!CreateRandomTarget())
                return false;
            if (escortDifficulty > 0)
            {
                if (!CreateRandomEscort())
                    return false;
                CreatePedGroup();
            }
            Random rand = new Random();
            reward = rand.Next(1000, 10000);
            hitmanNotify(string.Format("Location : {0} on {1}", World.GetZoneNameLabel(target.Position), World.GetStreetName(target.Position)));
            if (inVehicle)
                hitmanNotify(string.Format("Pizza is in a {0}. {1}", target.CurrentVehicle.FriendlyName.ToString(), ((reward >= 30000) ? "Be careful, pizza is spicy" : "")));

            return SetUpPedsTasks();
        }

        /* Notifications */

        public static void hitmanNotify(String message)
        {
            //Function.Call((Hash)0x92F0DA1E27DB96DC, 6); // _SET_NOTIFICATION_BACKGROUND_COLOR
            Function.Call(Hash._SET_NOTIFICATION_TEXT_ENTRY, "STRING");
            Function.Call((Hash)0x6C188BE134E074AA, message); //ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME
            Function.Call<int>(Hash._SET_NOTIFICATION_MESSAGE_CLAN_TAG_2, "CHAR_HUNTER", "CHAR_HUNTER", false, 2, "Middleman", "Pizza delivery", 1.0f, "___Piz'", 9);
            Function.Call(Hash._DRAW_NOTIFICATION, 0, 1);
        }


        /* Random creation */


        private Vector3 getRandomPosNearPlayer()
        {
            Vector3 vec = getTotallyRandomPos(Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0, 5, 0)), nearDistance);
            String posName = "";
            if (vec != Vector3.Zero)
            {
                posName = string.Format("{0}{1}", World.GetZoneNameLabel(vec), World.GetStreetName(vec));
                if (prevPosName.Count >= 7)
                    prevPosName.Clear();
                if (prevPosName.Contains(posName))
                    return (Vector3.Zero);
                prevPosName.Add(posName);
            }
            return vec;
        }

        public Vector3 getTotallyRandomPos(Vector3 player, int distance)
        {
            Random rnd = new Random();
            float x;
            float y;
            float z;
            Vector3 safeCoords = Vector3.Zero;
            Vector3 unSafeCoords = Vector3.Zero;

            for (int i = 0; i < 20; i++)
            {
                x = rnd.Next((int)player.X - distance, (int)player.X + distance);
                y = rnd.Next((int)player.Y - distance, (int)player.Y + distance);
                z = World.GetGroundHeight(new Vector2(x, y));
                unSafeCoords = new Vector3(x, y, z);
                safeCoords = World.GetSafeCoordForPed(unSafeCoords);
                if (safeCoords != Vector3.Zero)
                    break;
            }
            if (inVehicle)
            {
                if (safeCoords == Vector3.Zero)
                    safeCoords = World.GetNextPositionOnStreet(unSafeCoords);
                else
                    safeCoords = World.GetNextPositionOnStreet(safeCoords);
            }
            else if (safeCoords == Vector3.Zero)
                safeCoords = World.GetNextPositionOnStreet(unSafeCoords);
            return safeCoords;
        }


        private Vehicle getRandomVehicle(Vector3 unSafeCoords, int distance, bool escort)
        {
            Random rnd = new Random();

            int rndVeh = rnd.Next(0, escort ? 3 : vehicleList.Count);

            Vector3 vec = getTotallyRandomPos(unSafeCoords, distance);
            if (vec == Vector3.Zero)
                return null;
            Vehicle pedVehicle = World.CreateVehicle(new Model(vehicleList[rndVeh]), vec.Around(5.0F));
            return pedVehicle;
        }

        private Ped CreateRandomDriver(Vector3 unSafeCoords, bool useNearDistance)
        {
            bool escort = false;
            if (Entity.Exists(target) && Entity.Exists(target.CurrentVehicle))
                escort = true;
            Vehicle veh = null;
            Ped ped = null;
            veh = getRandomVehicle(unSafeCoords, (useNearDistance) ? nearDistance : 10, escort);
            if (veh != null)
                ped = Function.Call<Ped>(Hash.CREATE_RANDOM_PED_AS_DRIVER, veh, true);
            return ped;
        }

        private bool CreateRandomTarget()
        {
            Random rand = new Random();
            int rnd = rand.Next(3, 12);
            if (inVehicle)
            {
                target = CreateRandomDriver(Game.Player.Character.Position, true);
                if (target == null)
                    return false;
                if (rnd < weapons.Count)
                    target.Weapons.Give((WeaponHash)weapons[rnd].WeaponHashValue, 120, false, true);
            }
            else
            {
                Vector3 awaitedVec = Vector3.Zero;
                if ((awaitedVec = getRandomPosNearPlayer()) != Vector3.Zero)
                {
                    target = GTA.World.CreateRandomPed(awaitedVec);
                    if (rnd < weapons.Count)
                        target.Weapons.Give((WeaponHash)weapons[rnd].WeaponHashValue, 120, false, true);
                }
                else
                    return false;
            }
            if (target != null)
                target.AddBlip();
            else
                return false;
            Function.Call(Hash.CLEAR_AREA_OF_VEHICLES, target.Position.X, target.Position.Y, target.Position.Z, 100, false, false, false, false, false);
            return true;
        }

        private bool AddDriverToEscort()
        {
            Ped escortPed = CreateRandomDriver(escortVehicles.Count > 0 ? escortVehicles[escortVehicles.Count - 1].Position.Around(50.0F) : target.Position.Around(10.0F), false);
            if (escortPed == null)
                return false;
            escortPeds.Add(escortPed);
            escortVehicles.Add(escortPed.CurrentVehicle);
            return true;
        }

        private int GetTotalSeatCount()
        {
            int res = 0;
            foreach (Vehicle veh in escortVehicles)
            {
                res += Function.Call<int>(Hash.GET_VEHICLE_MAX_NUMBER_OF_PASSENGERS, veh);
                res -= 1;
            }
            return (res);
        }

        private bool GiveWeaponToEscort()
        {
            Random rnd = new Random();
            int rndWeapons = 0;

            if (inVehicle)
                rndWeapons = rnd.Next(14, 24);
            else
                rndWeapons = rnd.Next();

            foreach (Ped ped in escortPeds)
            {
                if (rndWeapons <= weapons.Count)
                    ped.Weapons.Give((WeaponHash)weapons[rndWeapons].WeaponHashValue, 120, false, true);
                ped.Weapons.Give((WeaponHash)weapons[8].WeaponHashValue, 120, false, true);
                ped.Weapons.Give((WeaponHash)weapons[11].WeaponHashValue, 120, false, true);
                rndWeapons = rnd.Next(14, 24);
            }
            return true;
        }

        private void CreateEscortForVehicle(Vehicle veh)
        {
            for (int nbToCreate = Function.Call<int>(Hash.GET_VEHICLE_MAX_NUMBER_OF_PASSENGERS, veh); nbToCreate > 0; nbToCreate--)
            {
                Ped curPed = GTA.World.CreateRandomPed(target.Position.Around((float)(vehicleList.Count * 10)));
                curPed.Task.WarpIntoVehicle(veh, (VehicleSeat)nbToCreate - 1);
                escortPeds.Add(curPed);
            }
        }

        private bool CreateRandomEscortVehicles()
        {
            Random rnd = new Random();
            int rndVeh = rnd.Next(1, escortDifficulty * 2);

            for (int i = 0; i < rndVeh; i++)
            {
                if (!AddDriverToEscort())
                    return false;
            }

            CreateEscortForVehicle(target.CurrentVehicle);
            foreach (Vehicle veh in escortVehicles)
                CreateEscortForVehicle(veh);
            return true;
        }


        private bool CreateRandomEscort()
        {
            Random rnd = new Random();
            int rndNum = rnd.Next(1, escortDifficulty * 3);

            if (!inVehicle)
            {
                for (int i = 0; i < rndNum; i++)
                {
                    escortPeds.Add(GTA.World.CreateRandomPed(target.Position.Around(2.0F)));
                    if (escortPeds[i] == null)
                        return false;
                }
            }
            else
            {
                if (!(CreateRandomEscortVehicles()))
                    return false;
            }
            if (!GiveWeaponToEscort())
                return false;
            return true;
        }

        public void CreatePedGroup()
        {
            if (groupId == 0)
                groupId = World.AddRelationshipGroup("EscortGroup");
            target.RelationshipGroup = groupId;
            PedGroup targetGroup = new PedGroup();
            targetGroup.Add(target, true);
            foreach (Ped bodyGuard in escortPeds)
            {
                if (!inVehicle)
                    targetGroup.Add(bodyGuard, false);
                bodyGuard.RelationshipGroup = groupId;
                Function.Call(Hash.SET_PED_TO_INFORM_RESPECTED_FRIENDS, bodyGuard, 5000, -1);
            }
            World.SetRelationshipBetweenGroups(Relationship.Hate, Function.Call<int>(Hash.GET_PLAYER_GROUP, Game.Player), groupId);
        }

        private bool SetUpTargetTask()
        {
            Function.Call(Hash.SET_COMBAT_FLOAT, target, 11, 100);
            if (inVehicle)
                Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, target, target.CurrentVehicle, 25.0F, 786603);
            else
                target.Task.WanderAround();

            /*
            Vector3 randomDestination = getTotallyRandomPos(target.Position, 100);
            if (inVehicle)
            {
                randomDestination = World.GetNextPositionOnStreet(randomDestination);
                target.CurrentVehicle.PlaceOnNextStreet();
                if (randomDestination != Vector3.Zero)
                {
                    decidedPosition = randomDestination;
                    target.Task.DriveTo(target.CurrentVehicle, decidedPosition, 100.0F, 50.0F, 128);
                }
                else
                    target.Task.CruiseWithVehicle(target.CurrentVehicle, 50.0F, 128);
            }
            else
            {

                randomDestination = World.GetSafeCoordForPed(randomDestination, false);
                if (randomDestination != Vector3.Zero)
                {
                    target.Task.GoTo(randomDestination);
                    decidedPosition = randomDestination;
                }
                else
                    target.Task.WanderAround();
            }
            */
            return SetUpEscortTask();
            /*
                    if (randomDestination != Vector3.Zero)
                    {
                        if (inVehicle)
                        {
                            Function.Call(Hash.TASK_VEHICLE_DRIVE_TO_COORD, target, target.CurrentVehicle, randomDestination.X, randomDestination.Y, randomDestination.Z, 30f, 1f, target.CurrentVehicle.GetHashCode(), 16777216, 1f, true);
                        }
                        else
                            target.Task.GoTo(randomDestination);
                        decidedPosition = randomDestination;
                    }
                    else
                    {
                        if (inVehicle)
                        {
                            target.Task.CruiseWithVehicle(target.CurrentVehicle, 30.0F, 16777216);
                        }
                        else
                            target.Task.WanderAround();
                    }
                    return SetUpEscortTask();
                    */
        }

        private bool SetUpEscortTask()
        {
            Random rand = new Random();
            foreach (Ped ped in escortPeds)
            {
                Function.Call(Hash.SET_COMBAT_FLOAT, ped, 11, 100);
                if (inVehicle)
                {
                    if (ped.SeatIndex == VehicleSeat.Driver)
                    {
                        if (!(Function.Call<bool>(Hash.IS_POINT_ON_ROAD, ped.Position.X, ped.Position.Y, ped.Position.Z)))
                            ped.CurrentVehicle.Position = World.GetNextPositionOnStreet(ped.CurrentVehicle.Position);
                        Function.Call(Hash.TASK_VEHICLE_ESCORT, ped, ped.CurrentVehicle, target.CurrentVehicle, rand.Next(-2, 5), 30.0F, 1, 30.0F);
                    }
                }
                else
                {
                    ped.Task.GoTo(target);
                    //                Function.Call(Hash.TASK_FOLLOW_TO_OFFSET_OF_ENTITY, ped, target, 2.0, 2.0, 2.0, -1, 10.0, 1, true);
                }
            }
            return true;
        }

        private bool SetUpPedsTasks()
        {
            return SetUpTargetTask();
        }

        /* clear */

        private void clearTarget()
        {
            if (Entity.Exists(target))
            {
                target.Task.ClearAllImmediately();
                if (target.IsInVehicle())
                {
                    Vehicle toDelete = target.CurrentVehicle;
                    target.Task.LeaveVehicle();
                    toDelete.Delete();
                }
                else if (Entity.Exists(target.LastVehicle))
                    target.LastVehicle.Delete();
                target.MarkAsNoLongerNeeded();
            }
        }

        private void clearAll()
        {
            clearTarget();
            if (escortDifficulty > 0)
            {
                foreach (Ped ped in escortPeds)
                {
                    ped.Task.ClearAllImmediately();
                    ped.Task.LeaveVehicle();
                    ped.MarkAsNoLongerNeeded();
                }

                escortPeds.Clear();
            }
            if (inVehicle)
            {
                foreach (Vehicle veh in escortVehicles)
                    veh.Delete();
                escortVehicles.Clear();
                vehicleList.Clear();
            }
            if (groupId != 0)
            {
                World.RemoveRelationshipGroup(groupId);
                groupId = 0;
            }
        }


        /* Mission control */
        private int GetHashKey(string value)
        {
            return Function.Call<int>(Hash.GET_HASH_KEY, value);
        }

        private bool MeetsRequirements()
        {
            bool meets = false;
            bool exists = false;

            //        String requestedWeaponValid = "";
            Hash requestedWeaponHash = new Hash();
            if (requestedWeapon.Length > 0 && !requestedWeapon.Equals("Free"))
            {
                foreach (Data data in weapons)
                {
                    if (data.WeaponName.Contains(requestedWeapon))
                    {
                        //                    requestedWeaponValid = data.WeaponName;
                        requestedWeaponHash = data.WeaponHashValue;
                        exists = true;
                        break;
                    }
                }
            }
            else
                meets = true;
            if (exists && !meets)
            {
                if ((Function.Call<bool>((Hash)0x2D343D2219CD027A, target, (int)requestedWeaponHash/*GetHashKey(requestedWeaponValid)*/, 0)))
                    meets = true;
            }

            return meets;
        }

        public void MissionStart()
        {
            if (missionStarted == false)
            {
                if (!SetupMission())
                {
                    clearAll();
                    missionStarted = false;
                    missionComplete = true;
                }
                else
                {
                    missionStarted = true;
                    missionComplete = false;
                }
            }
        }
        private bool NearByEnemies()
        {
            foreach (Ped attacker in escortPeds)
            {
                if (!attacker.IsDead && Vector3.Distance(attacker.Position, Game.Player.Character.Position) <= 100)
                    return true;
            }
            return false;
        }

        public void MissionEnd(bool immediately)
        {
            if (immediately)
            {
                missionStarted = false;
                missionComplete = true;
                clearAll();
                return;
            }
            if (NearByEnemies())
            {
                if (!LoseAssailantsNotified)
                {
                    UI.Notify("Get away from the assailants");
                    LoseAssailantsNotified = true;
                }
                return;
            }
            if (Game.Player.WantedLevel != 0)
            {
                if (!LosePoliceNotified)
                {
                    UI.Notify("Lose the cops");
                    LosePoliceNotified = true;
                }
                return;
            }
            else if (LosePoliceNotified)
            {
                LosePoliceNotified = false;
                LeaveAreaNotified = false;
                LoseAssailantsNotified = false;
            }

            if (Vector3.Distance(Game.Player.Character.Position, target.Position) < nearDistance / 2)
            {
                if (!LeaveAreaNotified)
                {
                    UI.Notify("Leave the area");
                    LeaveAreaNotified = true;
                }
                return;
            }
            else if (LeaveAreaNotified)
                LeaveAreaNotified = false;

            if (!Game.Player.IsPlaying)
            {
                hitmanNotify("Pizza was too hot. Client didn't want to pay for it");
                missionStarted = false;
                missionComplete = true;
                clearAll();
                return;
            }
            if (!MeetsRequirements())
            {
                hitmanNotify("Pizza delivered, but without extra cheese. No tips for you !");
                Game.Player.Money += reward;
                missionStarted = false;
                missionComplete = true;
                clearAll();
                return;
            }

            hitmanNotify(string.Format("Pizza delivered. {0} $ awarded", reward));
            Game.Player.Money += (reward + bonusReward);
            missionStarted = false;
            missionComplete = true;
            clearAll();
        }



        private void OnTick(object sender, EventArgs e)
        {
        }

        public void CheckMissionStatus()
        {
            if (missionStarted)
            {
                if (missionComplete)
                {
                    this.MissionEnd(false);
                }
                else if (target != null && target.Position == decidedPosition)
                    SetUpTargetTask();
                if (target.IsDead)
                    missionComplete = true;
            }
        }
    }


    public class HitmanMod : Script
    {
        private int FPS = 60; // Change here to adapt your framerates : this affects time handling, so be careful !;
        private int time_before_next = 12; // time in seconds before next target appears;
        private ScriptSettings configSettings;

        private Keys StartMode;

        private bool isRunning = false;
        private int currentTick = 0;
        private int nearDistance = 600; // Change here the prefered distance between targets. 1000 is close to a kilometer. I Recommand 600
        private bool inVehicle = false;
        private bool shouldSpawnHitmanVehicle = false; // Change here wether you want (true) or not (false) to spawn a vehicle when mod starts.

        private bool middlemanContact = true; // Change here if you want the middleman to not appear as a contact in your phone.
        private bool oneAtATime = false; // Change here if you want the missions to loop until you call middleman. It requires to have middlemanContact set to true.

        private HitmanMission mission = null; // If you want to add your own scripted missions, check the list of constructors available.
        private Random rnd = new Random();
        private Hitman.iFruitHitman hitmanContact;
        private bool canStopMission = false;

        public HitmanMod()
        {
            Tick += OnTick;

            KeyDown += OnKeyDown;
            LoadConfig("scripts//Hitman.ini");
        }

        private void LoadConfig(String fileName)
        {
            try
            {
                /*KEY SETTINGS*/
                configSettings = ScriptSettings.Load(fileName);
                StartMode = configSettings.GetValue<Keys>("Key Settings", "StartHitmanMode", Keys.F10);

                /*VALUES SETTINGS*/
                middlemanContact = configSettings.GetValue<bool>("Gameplay Settings", "EnableMiddlemanPhoneContact", true);
                nearDistance = configSettings.GetValue<int>("Gameplay Settings", "MaxDistanceTargetAppearsFromPlayer", 600);
                time_before_next = configSettings.GetValue<int>("Gameplay Settings", "TimeBeforeNextTargetAppears", 12);
                FPS = configSettings.GetValue<int>("Gameplay Settings", "YourGeneralFpsInGame", 60);
                oneAtATime = configSettings.GetValue<bool>("Gameplay Settings", "CallMiddlemanBeforeEach", false);
                shouldSpawnHitmanVehicle = configSettings.GetValue<bool>("Gameplay Settings", "SpawnHitmanVehicle", false);
                
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
            nearDistance = 600;
            middlemanContact = true;
            time_before_next = 12;
            FPS = 60;
            oneAtATime = false;
            shouldSpawnHitmanVehicle = false;
        }
        private void OnTick(object sender, EventArgs e)
        {
            if (isRunning)
            {
                if (middlemanContact)
                    hitmanContact.Update();
                if (!middlemanContact || hitmanContact.HasMissionStarted())
                {
                    mission.CheckMissionStatus();
                    if (currentTick >= time_before_next * FPS)
                    {
                        if (!mission.GetMissionStarted())
                        {
                            if (canStopMission && middlemanContact && oneAtATime)
                            {
                                hitmanContact.StopMissions();
                                canStopMission = false;
                            }
                            if (mission.GetMissionComplete())
                            {
                                mission.SetMissionDifficulty(rnd.Next(0, 5));
                                mission.SetInVehicle(rnd.Next(0, 100) < 70 ? false : true);
                            }
                            if (!middlemanContact || hitmanContact.HasMissionStarted())
                                mission.MissionStart();
                        }
                        currentTick = 0;
                    }
                    if (mission.GetMissionStarted())
                        canStopMission = true;
                    currentTick++;
                }
                else if (mission.GetMissionStarted())
                {
                    mission.MissionEnd(true);
                }

            }
        }


        public void beginHitman()
        {
            if (isRunning)
            {
                isRunning = false;
                UI.Notify("Hitman Mod Deactivated");
                if (mission.GetMissionStarted())
                    mission.MissionEnd(true);
                if (middlemanContact)
                {
                    hitmanContact.Dispose();
                    hitmanContact = null;
                }
            }
            else
            {
                mission = new HitmanMission(inVehicle, 0, nearDistance);
                isRunning = true;
                if (shouldSpawnHitmanVehicle)
                    World.CreateVehicle(VehicleHash.Buffalo, Game.Player.Character.Position.Around(5.0F));
                if (middlemanContact)
                    hitmanContact = new Hitman.iFruitHitman();

                UI.Notify("Hitman Mod Activated");
            }
        }
        void OnKeyDown(object sender, KeyEventArgs e)
        {

            switch (e.KeyCode)
            {
                case Keys.F10: //Edit the key that you have to press here
                    beginHitman();
                    break;
            }

        }
    }
}