using AluminiumCallouts.util;
using LSPD_First_Response.Mod.API;
using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AluminiumCallouts.Callouts.Investigate.Drug_Deal
{
    [CalloutInfo("DD-Strawberry", CalloutProbability.Low)]
    class Strawberry : Callout
    {
        private Ped dealer, buyer;
        private String[] models = { "a_f_m_eastsa_01", "a_f_m_eastsa_02", "a_f_m_business_02_p", "a_f_m_tourist_01", "a_f_y_business_01", "a_f_y_hippie_01", "a_f_y_rurmeth_01", "a_m_m_hillbilly_01", "csb_grove_str_dlr", "csb_grove_str_dlr_p" };
        private Vector3[] bins = { new Vector3(-26.755f, -1499.665f, 32.340f), new Vector3(-23.444f, -1492.641f, 30.362f), new Vector3(-21.946f, -1493.594f, 30.362f)};
        private Vector3 spawnpoint = new Vector3(-27.251f, -1494.527f, 30.362f), deliverPoint = new Vector3(452.0358f, -982.4515f, 30.6896f);
        private Blip spawnPoint;
        private List<Blip> binBlips = new List<Blip>();
        private String[] drugs = { };
        private int times = 0, bin = 0;
        private AlertState als = AlertState.UNSEPECTING;
        private Boolean investigate = false, blips = false, markerIsSet = false, attacks = false, deliverToStation = false, showHelp = false, showHelp1 = false, showHelp2 = false;
        private LHandle chase;
        private Marker m;

        public override bool OnBeforeCalloutDisplayed()
        {
            try
            {
                ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 30f);
                AddMinimumDistanceCheck(300f, spawnpoint);

                CalloutMessage = "~b~Drug deal spotted near ~o~Strawberry~b~.";
                CalloutPosition = spawnpoint;
            } catch(Exception e)
            {
                Main.GetStats().updateStats(false, 0, 0, 0, 0);
                Game.LogTrivial(e.Source + " caused an error:");
                Game.LogTrivial(e.Message);
                Game.LogTrivial(e.StackTrace);
            }

            
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            Game.DisplaySubtitle("~W~Attend to the scene; ~b~Code 2~w~.", 5000);
            dealer = new Ped(models[Main.getMath().getRandomInt(0, models.Length)], spawnpoint.Around(2f), 0f);
            buyer = new Ped(models[Main.getMath().getRandomInt(0, models.Length)], spawnpoint.Around(2f), 180f);
            spawnPoint = new Blip(spawnpoint)
            {
                Color = Color.Yellow
            };
            spawnPoint.EnableRoute(Color.Yellow);

            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();
            {
                if(Game.LocalPlayer.Character.DistanceTo2D(spawnpoint) < 200f && als != AlertState.SPOOKED)
                {
                    if(Game.LocalPlayer.Character.IsInAnyPoliceVehicle && !Game.LocalPlayer.Character.CurrentVehicle.IsSirenSilent && Game.LocalPlayer.Character.CurrentVehicle.IsSirenOn)
                        {
                            als = AlertState.SPOOKED;
                            dealer.Dismiss();
                            buyer.Dismiss();

                        if (Main.getMath().getRandomInt(0, 2) > 1)
                        {
                            CalloutUtil.DisplayInformation("Dispatch", "~o~Callout Update", "~w~The caller has informed the drug deal was disrupted, 10-4.");
                            End();
                        }
                        else
                        {

                            investigate = true;
                            CalloutUtil.DisplayInformation("Dispatch", "~o~Callout Update", "~w~Caller says that they were spooked and that they threw something in one of the bins.");
                            spawnPoint.Delete();
                        }
                        
                    }
                }

                if (Game.LocalPlayer.Character.DistanceTo2D(spawnpoint) < 20f)
                {
                    if (!investigate && als.Equals(AlertState.UNSEPECTING) && !attacks)
                    {
                        if (Game.LocalPlayer.Character.DistanceTo2D(spawnpoint) < 8f)
                        {
                            als = AlertState.BLOWN;
                            int attack = Main.getMath().getRandomInt(0, 3);
                            if (attack == 1)
                            {
                                attacks = true;
                                buyer.Inventory.GiveNewWeapon("WEAPON_PISTOL", 9999, true);
                                dealer.Inventory.GiveNewWeapon("WEAPON_KNIFE", 9999, true);
                                buyer.Tasks.FightAgainst(Game.LocalPlayer.Character);
                                dealer.Tasks.FightAgainst(Game.LocalPlayer.Character);
                                Game.DisplaySubtitle("~r~?: ~w~Its the pigs, get them.", 2000);
                            }
                            else if (attack == 0)
                            {
                                int run = Main.getMath().getRandomInt(0, 2);
                                switch (run) {
                                    case 0:
                                    buyer.Tasks.PutHandsUp(9999, Game.LocalPlayer.Character);
                                    dealer.Tasks.Flee(Game.LocalPlayer.Character, 100f, 5000);
                                        break;
                                    case 1:
                                        buyer.Tasks.PutHandsUp(9999, Game.LocalPlayer.Character);
                                        dealer.Tasks.PutHandsUp(9999, Game.LocalPlayer.Character);
                                        break;
                                    case 2:
                                    dealer.Tasks.PutHandsUp(9999, Game.LocalPlayer.Character);
                                    buyer.Tasks.Flee(Game.LocalPlayer.Character, 100f, 5000);
                                        break;
                            }
                            }
                            else if (attack >= 2)
                            {
                                chase = Functions.CreatePursuit();
                                Functions.AddPedToPursuit(chase, dealer);
                                Functions.AddPedToPursuit(chase, buyer);
                                // Make sure they flee
                                dealer.Tasks.Flee(Game.LocalPlayer.Character, 100f, 5000);
                                buyer.Tasks.Flee(Game.LocalPlayer.Character, 100f, 5000);
                                Functions.SetPursuitIsActiveForPlayer(chase, true);
                                Game.DisplaySubtitle("~r~?: ~w~What was that, leg it.", 2000);
                            }
                            spawnPoint.Delete();
                        }
                    }
                }

                if(Game.LocalPlayer.Character.DistanceTo(spawnpoint) < 100f) {
                 if(!investigate && (Functions.IsPedArrested(buyer) && Functions.IsPedArrested(dealer) || Functions.IsPedArrested(dealer) && buyer.IsDead || Functions.IsPedArrested(buyer) && dealer.IsDead) || dealer.IsDead && buyer.IsDead)
                 {
                        if(chase != null)
                        {
                            if(Functions.IsPursuitStillRunning(chase))
                            {
                                Functions.ForceEndPursuit(chase);
                            }
                        }
                        investigate = true;
                 }

                  if (investigate)
                    {
                        if (!blips) {
                            foreach (Vector3 value in bins)
                            {
                                binBlips.Add(new Blip(value)
                                {
                                    Color = Color.Yellow,
                                    Scale = 0.5f
                                });
                                bin += 1;
                            };
                            blips = true;
                        }


                        if (!showHelp)
                        {
                            Game.DisplayHelp("~b~You should check out one of the bins by walking up and pressing E to investigate", false);
                            showHelp = true;
                        }
                        if (times < bin) {
                            if (Game.LocalPlayer.Character.DistanceTo(bins[0]) < 4f || Game.LocalPlayer.Character.DistanceTo(bins[1]) < 4f || Game.LocalPlayer.Character.DistanceTo(bins[2]) < 4f) {
                                if (!showHelp1)
                                {
                                    Game.DisplayHelp("~b~Press ~w~E ~b~to search the bin for drugs.", 3000);
                                    showHelp1 = true;
                                }
                                if (Game.IsKeyDown(Keys.E))
                                {
                                    Game.LocalPlayer.Character.Tasks.PlayAnimation("amb@code_human_police_investigate@idle_a", "idle_a", 4, AnimationFlags.Loop);
                                    GameFiber.Sleep(5000);
                                    String drugFound = CalloutUtil.getRandomDrugType();
                                    Game.DisplayNotification("~b~You found some ~o~" + drugFound + ".");
                                    drugs.ToList().Add(drugFound);
                                    Game.LocalPlayer.Character.Tasks.Clear();
                                    times += 1;
                                }
                            }
                        }
                        else
                        {
                            investigate = false;
                            showHelp = false;
                            if (!deliverToStation)
                            {
                                if(!showHelp2)
                                {
                                    Game.DisplayHelp("~b~Return to the police station to deliver the drugs into the locker room.", false);
                                    showHelp2 = true;
                                }

                                while(deliverToStation)
                                {
                                    m = new Marker(new Vector3(452.0358f, -982.4515f, 30.6896f), Color.Aqua, true);
                                }

                                if(Game.LocalPlayer.Character.DistanceTo(deliverPoint) < 3)
                                {
                                    Game.DisplayHelp("~b~Press ~w~Y ~b~to put the drugs in the evidence locker.");
                                    if(Game.IsKeyDown(Keys.Y))
                                    {
                                        Game.FadeScreenOut(1500);
                                        GameFiber.Yield();
                                        m.Dispose();
                                        Game.FadeScreenIn(1500);
                                        deliverToStation = true;
                                    }
                                }
                            }
                            else
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
            Main.GetStats().updateStats(true, 2, 2, 0, 0);
            base.End();
            {
                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", "Dispatch", "~o~Callout Ended", "You found all the evidence.");
                if (spawnPoint != null & spawnPoint.Exists())
                {
                    spawnPoint.Delete();
                }

                if (binBlips != null) {
                    foreach (Blip a in binBlips)
                    {
                        if (a.Exists())
                        {
                            a.Delete();
                        }
                    }
                }

                if (dealer.Exists())
                {
                    dealer.Dismiss();
                }
                if(buyer.Exists())
                {
                    buyer.Dismiss();
                }
                deliverToStation = false;
                drugs = null;
                investigate = false;
                blips = false;
                attacks = false;
                showHelp = false;
                showHelp1 = false;
                showHelp2 = false;
            }
            Main.GetStats().printStatistics();
        }

        public override void OnCalloutNotAccepted()
        {
            base.OnCalloutNotAccepted();
        }
    }
}
