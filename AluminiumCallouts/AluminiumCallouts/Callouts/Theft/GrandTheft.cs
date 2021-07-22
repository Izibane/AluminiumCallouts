using LSPD_First_Response.Mod.Callouts;
using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AluminiumCallouts.Callouts.Theft
{
    [CalloutInfo("GrandTheft", CalloutProbability.Medium)]
    class GrandTheft : Callout
    {

        private Vector3 spawnpoint;
        private Vehicle vehicle;

        public override bool OnBeforeCalloutDisplayed()
        {
            spawnpoint = Game.LocalPlayer.Character.Position.Around(50f, 300f);

            ShowCalloutAreaBlipBeforeAccepting(spawnpoint, 50f);
            return base.OnBeforeCalloutDisplayed();
        }

        public override bool OnCalloutAccepted()
        {
            return base.OnCalloutAccepted();
        }

        public override void Process()
        {
            base.Process();
        }

        public override void End()
        {

            base.End();
        }

    }
}
