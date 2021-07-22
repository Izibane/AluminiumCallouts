using AluminiumCallouts.util;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System.Collections.Generic;
using System.Drawing;

namespace AluminiumCallouts.Callouts.Pursuit
{

    [CalloutInfo("AC-Pursuit", CalloutProbability.Medium)]
    class Pursuit : Callout
    {
        private Vehicle vehicle;
        private Vector3 spawnpoint;
        private Blip carBlip;
        private List<Blip> pedBlips = new List<Blip>();
        private List<Ped> pedsSpawned = new List<Ped>();
        private Ped driver, p;
        private long lastShot = 0L;

        private LHandle handle;
        private int suspects = 2, calloutState = 0;

        private bool displayedHelp = false, pursuitOver = false, addedToPursuit = false, shooting = false;

        //1 - reported, 2 - backup, 3 - 
        public override bool OnBeforeCalloutDisplayed()
        {
            suspects = Main.getMath().getRandomInt(1, 4);
            calloutState = Main.getMath().getRandomInt(0, 3);
            CalloutUtil.DebugInformation("[AC] Pursuit: ", "~b~Chose state", "State was set as " + calloutState);
            shooting = Main.getMath().getRandomInt(0, 10) <= 3;
            spawnpoint = Game.LocalPlayer.Character.Position.Around(150f, 550f);
            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 50f);
            switch (calloutState) {
                case 0:
                    CalloutMessage = "~b~Driver failing to stop for police near ~o~" + World.GetStreetName(spawnpoint) + "~b~.";
                    break;
                case 1:
                    CalloutMessage = "~b~Unit requesting backup in pursuit near ~o~" + World.GetStreetName(spawnpoint) + "~b~.";
                    break;
                case 2:
                    CalloutMessage = "~b~Reports of a wreckless driver near ~o~" + World.GetStreetName(spawnpoint) + "~b~.";
                    break;
                case 3:
                    CalloutMessage = "~b~Underage driver stolen parents car near ~o~" + World.GetStreetName(spawnpoint) + "~b~.";
                    break;
            }
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Vehicle v = new Vehicle(CalloutUtil.getRandomCar(), World.GetNextPositionOnStreet(spawnpoint))
            {
                IsPersistent = true
            };

            GameFiber.Yield();
            vehicle = v;

            for(int i = 0; i < suspects; i++)
            {
                CalloutUtil.DebugInformation("[AC] Pursuit: ", "~b~Spawning ~o~ped ~b~(~g~" + i + "~b~)", "~b~Warping into seat ~g~" + (i - 1) + "~b~.");

                p = new Ped(spawnpoint)
                {
                    IsPersistent = true
                };

                p.WarpIntoVehicle(vehicle, i - 1); // first ped = (1- 2) -1 (drivers)
                p.RelationshipGroup = "FLEEING";
                pedsSpawned.Add(p);
                GameFiber.Yield();
            }
            CalloutUtil.DebugInformation("[AC] Pursuit: ", "~b~Spawned peds", "~b~Spawned ~w~" + pedsSpawned.Count + " ~b~peds.");

            driver = vehicle.Driver;

            CalloutUtil.DebugInformation("[AC] Pursuit: ", "~b~Task Set", "~b~Set task for vehicle with speed of: " + vehicle.TopSpeed + "f");
            carBlip = vehicle.AttachBlip();
            carBlip.Color = Color.Red;
            carBlip.Flash(1000, 1000);
            carBlip.EnableRoute(Color.Yellow);

            handle = Functions.CreatePursuit();
            for (int i = 0; i < suspects; i++) {
                Ped pl = pedsSpawned[i];
                Functions.AddPedToPursuit(handle, pl);
                Functions.SetPedResistanceChance(pl, Main.getMath().getRandomInt(0, 1) == 0 ? 100f : 8f);
                if (calloutState == 3)
                {
                    Functions.GetPedPursuitAttributes(pl).HandlingAbility = 0.5f;
                    Functions.GetPedPursuitAttributes(pl).HandlingAbilityBurstTireMult = 0.05f;
                    Functions.GetPedPursuitAttributes(pl).HandlingAbilityTurns = 0.7f;
                }
                Functions.GetPedPursuitAttributes(pl).SurrenderChanceCarBadlyDamaged = Main.getMath().getRandomInt(0, 1) == 0 ? 50f : 0f;
                Functions.GetPedPursuitAttributes(pl).SurrenderChancePitted = Main.getMath().getRandomInt(0, 1) == 0 ? 5f : 0f;
                Functions.GetPedPursuitAttributes(pl).SurrenderChancePittedAndCrashed = Main.getMath().getRandomInt(0, 1) == 0 ? 25f : 0f;
                Functions.GetPedPursuitAttributes(pl).SurrenderChancePittedAndSlowedDown = Main.getMath().getRandomInt(0, 1) == 0 ? 25f : 0f;
                Functions.GetPedPursuitAttributes(pl).SurrenderChanceTireBurst = Main.getMath().getRandomInt(0, 1) == 0 ? 25f : 0f;
                Functions.GetPedPursuitAttributes(pl).SurrenderChanceTireBurstAndCrashed = Main.getMath().getRandomInt(0, 1) == 0 ? 25f : 0f;
                CalloutUtil.DebugInformation("[AC] Pursuit: ", "~o~Pursuit", "Set ped attributes");
            }

            if(calloutState == 0 || calloutState == 1)
            {
                Vehicle pl = new Vehicle(CalloutUtil.getRandomCityUnit(), World.GetNextPositionOnStreet(vehicle.Position.Around(50f)));
                pl.CreateRandomDriver();
                Functions.AddCopToPursuit(handle, pl.Driver);
            }

            
            CalloutUtil.DisplayInformation("Dispatch", "~o~Callout Update", "Suspects are in a " + vehicle.PrimaryColor.ToKnownColor().ToString().ToLower() + " ~b~" + vehicle.Model.Name.ToLower() + "~w~.");

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();
            {
                GameFiber.Yield();

                if (Game.LocalPlayer.Character.Position.DistanceTo(vehicle.Position) < 80f)
                {
                    if (!addedToPursuit)
                    {
                        if (shooting)
                        {
                            Game.SetRelationshipBetweenRelationshipGroups("FLEEING", "COP", Relationship.Hate);
                            Game.SetRelationshipBetweenRelationshipGroups("FLEEING", RelationshipGroup.Player, Relationship.Hate);
                            foreach (Ped ped in pedsSpawned)
                            {
                                p.Inventory.GiveNewWeapon("WEAPON_PISTOL", 9999, true);
                                if(ped != vehicle.Driver)
                                {
                                    ped.Tasks.FightAgainstClosestHatedTarget(20f);
                                }
                            }
                            CalloutUtil.DisplayInformation("Dispatch", "~o~Callout Update", "Suspects are armed, proceed with caution.");
                        }
                        carBlip.DisableRoute();
                        Functions.SetPursuitIsActiveForPlayer(handle, true);
                        addedToPursuit = true;
                    }

                }
                    if(!Functions.IsPursuitStillRunning(handle))
                    {
                        if (!displayedHelp)
                        {
                            Game.DisplayHelp("~b~Once you have dealt with the callout, press ~w~End ~b~to end the callout.", -1);
                            displayedHelp = true;
                        }
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
                for(int i = 0; i < pedsSpawned.Count; i++)
                {
                    if (pedsSpawned[i].Exists())
                    {
                        pedsSpawned[i].Dismiss();
                    }
                }
                pursuitOver = false;
                addedToPursuit = false;
                shooting = false;

                if(carBlip.Exists())
                {
                    carBlip.Delete();
                }

                if (vehicle.Exists())
                {
                    vehicle.Dismiss();
                }

                vehicle = null;
                driver = null;

            }
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
        }

    }
}
