using AluminiumCallouts.util;
using LSPD_First_Response.Mod;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AluminiumCallouts.Callouts.Crash
{
    [CalloutInfo("Crash", CalloutProbability.Medium)]
    class Crash : Callout
    {

        private Vector3[] spawns = { //La Puerta
            new Vector3(-380.653f, -1663.196f, 18.654f),
        };

        private Vector3[] witness =
        {
            new Vector3(-313.388f, -1561.458f, 34.345f)
        };

        private Vector3[] witnessStop = {

            new Vector3(-362.020f, -1632.579f, 17.870f)

         };

        private float[] witnessStopHeadings =
        {
            147.768f
        };
        

        private bool hasHelperSpawned = false, hasHelperTaskSet = false;

        private float[] headings = { //La Puerta
            92.614f };

        private float[] witnessHeadings =
        {
            148.359f
        };
        private Vector3 spawnpoint;

        private float heading = 0f;

        public List<Vehicle> vehicles = new List<Vehicle>();
        public List<Ped> peds = new List<Ped>();
        public List<Blip> blips = new List<Blip>();
        private Ped witnesses;

        private Vehicle crashed;
        private Blip route;
        private CrashState crashState;
        private RunCause runCause;
        private bool hasDealtWith = false, hasCalledEMS = false, hasShownHelp = false, addedBlips = false;

        private LHandle handle;

        public override bool OnBeforeCalloutDisplayed()
        {
            int number = Main.getMath().getRandomInt(0, 0);
            spawnpoint = spawns[number];
            heading = headings[number];
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 30f);
            CalloutMessage = "~b~Reports of an RTC near ~o~" + World.GetStreetName(spawnpoint) + "~b~.";
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Vehicle crashed = new Vehicle(CalloutUtil.getRandomCar(), spawnpoint, heading)
            {
                IsPersistent = true,
                IsDriveable = false
            };
            crashed.Deform(crashed.Position, 20f, 30f);
            crashed.EngineHealth = 400f;
            crashed.PunctureFuelTank();
            this.crashed = crashed;
           
            NativeFunction.CallByName<uint>("SMASH_VEHICLE_WINDOW", crashed, 0);
            int damage = Main.getMath().getRandomInt(0, 2);
            if(damage == 0)
            {
                crashed.DirtLevel = 3;
                crashed.SetRotationRoll(180f);
                crashState = CrashState.DEAD;
            } else if(damage == 1)
            {
                crashState = CrashState.SHOCKED;
                crashed.DirtLevel = 2;
                NativeFunction.CallByName<uint>("SET_VEHICLE_DOOR_OPEN", crashed, true, true);
            } else if(damage == 2)
            {
                crashState = CrashState.RUNNING;
                crashed.DirtLevel = 1;
                NativeFunction.CallByName<uint>("SET_VEHICLE_TYRE_BURST", crashed, 0, true, 1000);
                NativeFunction.CallByName<uint>("SET_VEHICLE_DOOR_OPEN", crashed, true, true);
            }

            GameFiber.Yield();

            Ped driver = crashed.CreateRandomDriver();
            driver.IsPersistent = true;

            peds.Add(driver);
            if(damage == 0)
            {
                driver.WarpIntoVehicle(crashed, -1);
                driver.Kill();
            }
            if(damage == 1)
            {
                driver.WarpIntoVehicle(crashed, 0); // Passenger seat (wait for police/medic)
            }
            if(damage == 2)
            {
                driver.Tasks.Flee(driver, 100f, 10000); // They ran, 
            }

            route = new Blip(spawnpoint)
            {
                Color = Color.Yellow
            };
            route.EnableRoute(Color.Yellow);
            
            Game.DisplaySubtitle("~w~Attend to the scene; ~b~Code 3~w~.", 4000);

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();
            {
                if(!hasHelperSpawned)
                {
                    GameFiber.Yield();
                    Vehicle help = new Vehicle(CalloutUtil.getRandomCar(), witness[0], witnessHeadings[0]);
                    vehicles.Add(help);
                    Ped ped = help.CreateRandomDriver();
                    witnesses = ped;
                    peds.Add(ped);

                    GameFiber.Yield();
                    ped.Tasks.DriveToPosition(witnessStop[0], 20f, VehicleDrivingFlags.Emergency);
                    hasHelperSpawned = true;
                }


                if (Game.LocalPlayer.Character.DistanceTo(spawnpoint) < 50f)
                {
                    if (route.Exists())
                    {
                        route.DisableRoute();
                        route.Delete();
                    }

                    if(!hasHelperTaskSet && witnesses.Tasks.CurrentTaskStatus == Rage.TaskStatus.NoTask)
                    {
                        witnesses.Tasks.LeaveVehicle(witnesses.CurrentVehicle, LeaveVehicleFlags.LeaveDoorOpen);
                        hasHelperTaskSet = true;
                    }


                    if (!addedBlips)
                    {
                        try
                        {
                            Blip one = witnesses.AttachBlip();
                            one.Color = Color.Aqua;
                            one.Scale = 0.7f;
                            blips.Add(one);

                            Blip car = crashed.AttachBlip();
                            car.Color = Color.Yellow;
                            car.Scale = 0.8f;
                            blips.Add(car);
                        }
                        catch (Exception e)
                        {
                            Game.LogTrivial("Error while adding blip: " + e.Source);
                            Game.LogTrivial(e.Message);
                            Game.LogTrivial(e.StackTrace);
                        }
                        addedBlips = true;
                    }

                    int random = Main.getMath().getRandomInt(0, 3);
                    String looks = "";
                    if(random == 0)
                    {
                        looks = "spun out";
                    } else if(random == 1)
                    {
                        looks = "lost control";
                    } else if(random == 2)
                    {
                        looks = "drove into the wall";
                    }

                    if (!hasDealtWith)
                    {
                        if (!hasShownHelp)
                        {
                            Game.DisplayHelp("~b~Looks like the car ~w~" + looks + "~b~. You should after you have dealt with the scene, speak to the witness in the car that stopped.");
                            hasShownHelp = true;
                        }
                        try {
                            if (Game.LocalPlayer.Character.DistanceTo(crashed.Position) < 10f)
                            {
                                if (crashState.Equals(CrashState.RUNNING))
                                {
                                    Game.DisplayHelp("~b~Press ~w~E ~b~near the car to investigate for clues.");
                                    if (Game.IsKeyDown(System.Windows.Forms.Keys.E))
                                    {
                                        int chance = Main.getMath().getRandomInt(0, 4);
                                        String ran = "It seems the driver ran because";
                                        if (chance <= 2)
                                        {
                                            runCause = RunCause.SCARED;
                                            ran += " they were scared, there's nothing illegal";
                                        }
                                        else if (chance == 3)
                                        {
                                            runCause = RunCause.DRUGS;
                                            ran += " they had drugs, " + CalloutUtil.getRandomDrugType() + " in their " + CalloutUtil.getRandomPlaceInCar();
                                        }
                                        else if (chance == 4)
                                        {
                                            runCause = RunCause.GUNS;
                                            ran += "they had a gun in their car.";
                                        }

                                        Game.LocalPlayer.Character.Tasks.PlayAnimation("amb@code_human_police_investigate@idle_a", "idle_a", 4, AnimationFlags.Loop);
                                        GameFiber.Sleep(5000);

                                        Game.DisplaySubtitle("~g~You: ~w~" + ran, 2000);
                                        GameFiber.Sleep(3000);



                                    }
                                    hasDealtWith = true;
                                }

                                if (crashState.Equals(CrashState.DEAD))
                                {
                                    Game.DisplaySubtitle("~g~You: ~w~Looks like the, uh, crash killed them. Jesus.", 3000);
                                    GameFiber.Sleep(3000);
                                    Game.DisplaySubtitle("~g~You: ~w~Dispatch, I need EMS in ~o~La Puerta ~w~on the bridge.", 2000);
                                    GameFiber.Sleep(2000);
                                    CalloutUtil.DisplayInformation("Dispatch", "~o~Callout Update", "EMS has been called to your location from Davis.");
                                    hasDealtWith = true;
                                }

                                if (crashState.Equals(CrashState.SHOCKED))
                                {
                                    if (Game.LocalPlayer.Character.Position.DistanceTo(vehicles[0]) < 10f)
                                    {
                                        Ped victim = peds[0];
                                        if (victim.IsInAnyVehicle(false))
                                        {
                                            victim.Tasks.LeaveVehicle(victim.CurrentVehicle, LeaveVehicleFlags.LeaveDoorOpen);
                                        }
                                        if (Game.LocalPlayer.Character.Position.DistanceTo(victim.Position) < 10f)
                                        {
                                            Game.DisplayHelp("~b~Press ~w~E ~b~to speak to the driver of the crashed car.");
                                            if (Game.IsKeyDown(System.Windows.Forms.Keys.E))
                                            {
                                                Game.DisplaySubtitle("~g~You: ~w~Are you alright there?", 3000);
                                                GameFiber.Sleep(3000);
                                                Game.DisplaySubtitle("~b~Driver: ~w~I, I.. don't know what happened I just span.", 3000);
                                                GameFiber.Sleep(3000);
                                                Game.DisplaySubtitle("~g~You: ~w~Are you hurt at all, is there anyone else?");
                                                GameFiber.Sleep(3000);
                                                Game.DisplaySubtitle("~b~Driver: ~w~My back hurts and I think I broke my nose.");
                                                GameFiber.Sleep(3000);
                                                Game.DisplaySubtitle("~g~You: ~w~Just sit down for a second, I'll get an ambulance to check you over.");
                                                GameFiber.Sleep(3000);
                                                GameFiber.Sleep(3000);
                                                Game.DisplaySubtitle("~g~You: ~w~Dispatch, I need EMS in ~o~La Puerta ~w~on the bridge.", 2000);
                                                GameFiber.Sleep(2000);
                                                CalloutUtil.DisplayInformation("Dispatch", "~o~Callout Update", "EMS has been called to your location from Davis Hospital and Firestation.");
                                                hasDealtWith = true;
                                            }
                                        }
                                    }
                                }

                            }
                        } catch(Exception e)
                        {
                            Main.GetStats().updateStats(false, 0, 0, 0, 0);
                            Game.LogTrivial(e.Source + " caused an error:");
                            Game.LogTrivial(e.Message);
                            Game.LogTrivial(e.StackTrace);
                            End();
                        }
                        }
                    else
                    {
                        if (crashState.Equals(CrashState.RUNNING))
                        {
                            if (handle == null)
                            {
                                handle = Functions.CreatePursuit();
                                Functions.AddPedToPursuit(handle, peds[0]);
                                Functions.SetPursuitIsActiveForPlayer(handle, true);
                            }

                        }
                        else
                        {
                            Game.DisplayHelp("~b~Press ~w~End ~b~to end the callout once you are done.");

                            if (crashState.Equals(CrashState.DEAD))
                            {
                                if (!hasCalledEMS)
                                {
                                    if (CalloutUtil.IsBetterEMSRunning())
                                    {
                                        BetterEMS.API.EMSFunctions.OverridePedDeathDetails(peds[0], "", "Internal Bleeding", Game.GameTime, 0.2f);
                                        BetterEMS.API.EMSFunctions.RespondToLocation(peds[0].Position, true);
                                    } else
                                    {
                                        
                                    }
                                    hasCalledEMS = true;
                                }
                            } else if (crashState.Equals(CrashState.SHOCKED))
                            {
                                if (!hasCalledEMS)
                                {
                                    if (CalloutUtil.IsBetterEMSRunning())
                                    {
                                        BetterEMS.API.EMSFunctions.PickUpPatient(peds[0], peds[0].Position);
                                    }
                                    hasCalledEMS = true;
                                }
                            }

                            if (Game.IsKeyDown(System.Windows.Forms.Keys.End))
                            {
                                End();
                            }
                        }
                    }
                }
            }
        }

        public override void End()
        {
            base.End();
            {
                GameFiber.Yield();
                foreach(Ped p in peds)
                {
                    if(p != null && p.Exists())
                    {
                        p.IsPersistent = false;
                        p.Delete();
                    }
                    peds.Remove(p);
                }

                GameFiber.Yield();

                foreach(Vehicle v in vehicles)
                {
                    if (v != null && v.Exists())
                    {
                        v.Delete();
                    }
                    vehicles.Remove(v);
                }

                foreach(Blip b in blips)
                {
                    if(b != null && b.Exists())
                    {
                        b.Delete();
                    }
                    blips.Remove(b);
                }

                handle = null;
                witnesses = null;
                hasDealtWith = false;
                hasHelperSpawned = false;
                hasCalledEMS = false;
                hasHelperTaskSet = false;

            }
            CalloutUtil.DisplayInformation("Dispatch", "~o~Callout Ended", "Situation is dealt with.");
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
        }

    }

    public enum CrashState
    {
        RUNNING, DEAD, SHOCKED
    }

    public enum RunCause
    {
        SCARED, DRUGS, GUNS, WANTED, INSURANCE
    }
}
