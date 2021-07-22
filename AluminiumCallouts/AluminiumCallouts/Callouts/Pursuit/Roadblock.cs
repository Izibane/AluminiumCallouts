using AluminiumCallouts.util;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace AluminiumCallouts.Callouts.Pursuit
{
    [CalloutInfo("RB-Highway-LPF", CalloutProbability.Medium)]
    class Roadblock : Callout
    {

        private List<Vehicle> roadblockVehicles = new List<Vehicle>();

        private List<Ped> polSpawned = new List<Ped>();

        private List<Ped> pedsSpawned = new List<Ped>();
        private Ped driver;

        private List<Blip> addedBlips = new List<Blip>();
        private Blip vehicleBlip;
        private Blip route;

        private readonly Vector3[] vbLocs = new Vector3[] { new Vector3(-1974.738f, -447.1180f, 11.521f), new Vector3(-1978.494f, -452.4144f, 11.5197f), new Vector3(-1981.742f, -456.9846f, 11.4979f) };
        private Vector3 vbSpawn = new Vector3(-1044.664f, -586.6494f, 18.1461f);
        private Vehicle vehicle;
        private float vbSpawnHeading = 122.5408f, vbRoadblockSpawn = 141.418f;

        private LHandle pursuit;

        private bool started = false;
        private int suspects;

        public override bool OnBeforeCalloutDisplayed()
        {
            suspects = Main.getMath().getRandomInt(1, 2);
            ShowCalloutAreaBlipBeforeAccepting(vbLocs[1], 50f);
            CalloutMessage = "~b~Unit requesting a roadblock near ~o~" + World.GetStreetName(vbLocs[1]);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            NativeFunction.CallByName<uint>("CLEAR_AREA", vbSpawn.X, vbSpawn.Y, vbSpawn.Z, 100f, 0, 0, 0, 0);
            

            Vehicle v = new Vehicle(CalloutUtil.getRandomCar(), vbSpawn, vbSpawnHeading)
            {
                IsPersistent = true
            };
            GameFiber.Yield();
            this.vehicle = v;

            try
            {
                for (int i = 0; i < suspects; i++)
                {
                    CalloutUtil.DisplayInformation("[AC] Roadblock: ", "~b~Spawning ~o~ped ~b~(~g~" + i + "~b~)", "~b~Warping into seat ~g~" + (i - 1) + "~b~.");
                    Ped p = new Ped(vbSpawn)
                    {
                        IsPersistent = true
                    };
                    // 0 -1 = -1 (driver side)
                    p.WarpIntoVehicle(vehicle, i - 1);
                    Blip b = p.AttachBlip();
                    b.Scale = 0.5f;
                    addedBlips.Add(b);
                    pedsSpawned.Add(p);
                    GameFiber.Yield();
                }

                string[] lspd = new string[] { "s_m_y_cop_01", "S_F_Y_COP_01" };

                for (int i = 0; i < 2; i++)
                {
                    Vehicle veh = new Vehicle(CalloutUtil.getRandomCityUnit(), vbLocs[i], vbRoadblockSpawn)
                    {
                        IsPersistent = true
                    };

                    CalloutUtil.DisplayInformation("[AC] Roadblock: ", "~b~Spawning ~o~police ped ~b~(~g~" + i + "~b~)", "~b~Warping into seat ~g~" + (i - 1) + "~b~.");
                    Ped pol = new Ped(lspd[i], veh.Position, vbSpawnHeading);

                    pol.WarpIntoVehicle(veh, -1);
                    polSpawned.Add(pol);
                }
            } catch(Exception e)
            {
                CalloutUtil.DisplayInformation("[AC]", "Error", "Crashed at: " + e.Source + ", printing trace in console.");
                Game.LogTrivialDebug(e.GetBaseException().GetType() + "  crash at:");
                Game.LogTrivialDebug(e.Message);
                Game.LogTrivialDebug(e.StackTrace);
            }

            this.driver = vehicle.Driver;

            this.route = new Blip(vbLocs[2]);
            route.Color = Color.Red;
            route.EnableRoute(Color.Red);

            addedBlips.Add(route);

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();
            {
                if(!started && Game.LocalPlayer.Character.Position.DistanceTo(vbLocs[0]) < 10f)
                {
                    route.DisableRoute();

                    driver.Tasks.CruiseWithVehicle(20f, VehicleDrivingFlags.Emergency);

                    vehicleBlip = vehicle.AttachBlip();
                    vehicleBlip.Color = Color.Red;
                    vehicleBlip.Flash(500, 2000);
                    addedBlips.Add(vehicleBlip);

                    pursuit = Functions.CreatePursuit();
                    foreach(Ped p in pedsSpawned)
                    {
                        Functions.AddPedToPursuit(pursuit, p);
                    }

                    foreach(Ped p in polSpawned)
                    {
                        Functions.AddCopToPursuit(pursuit, p);
                    }

                    started = true;
                }

                if(started)
                {
                    Game.DisplayHelp("~b~Once you have dealt with the callout, press ~w~End ~b~to end the callout.");
                    if (Game.IsKeyDown(System.Windows.Forms.Keys.End))
                    {
                        End();
                    }
                }
            }
        }

        public override void End()
        {
            base.End();
            {
                foreach(Blip b in addedBlips)
                {
                    b.Delete();
                }

                foreach(Ped p in polSpawned)
                {
                    p.Dismiss();
                }

                Main.GetStats().updateStats(true, 0, 0, 0, 0);
            }
        }


    }
}
