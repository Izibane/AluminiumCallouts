using LSPD_First_Response.Mod.API;
using Rage;
using System;

namespace AluminiumCallouts
{
    public class Main : Plugin
    {
        private string prefix = "AluminiumCallouts";
        private readonly string version = "1.0";
        private static VersionState state = VersionState.DEVELOPMENT;
        private static Stats stats;
        private static Math math = new Math();

        public override void Finally()
        {
            Game.LogTrivial(prefix + " AluminiumCallouts v" + version + " has been disabled.");
            if(state.Equals(VersionState.DEVELOPMENT))
            {
                Game.LogTrivial("----------------------------------");
                Game.LogTrivial(prefix + " Statistics for session (" + ((System.DateTime.Now.Millisecond - stats.started) / 1000) + " seconds)");
                Game.LogTrivial("Callouts Total: " + stats.callouts);
                Game.LogTrivial("Callouts Errors: " + stats.errors + " (epc: " + math.divideNumber(stats.errors, stats.callouts) + ")");
                Game.LogTrivial("Vehicles spawned: " + stats.vehicles + " (vpc: " + math.divideNumber(stats.vehicles, stats.callouts) + ")");
                Game.LogTrivial("Suspects spawned: " + stats.suspects + "(spc: " + math.divideNumber(stats.suspects, stats.callouts) + ")");
                Game.LogTrivial("Officers spawned: " + stats.officers + "(opc: " + math.divideNumber(stats.officers, stats.callouts) + ")");
                Game.LogTrivial("----------------------------------");
            }
        }

        public override void Initialize()
        {
            stats = new Stats
            {
                started = System.DateTime.Now.Millisecond
            };
            Functions.OnOnDutyStateChanged += OnOnDutyStateChangedHandler;
            Game.LogTrivial(prefix + " AluminiumCallouts v" + version + " has been enabled.");
            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", prefix, "~o~Enabled", "~b~Aluminium Callouts ~g~v" + version + "~b~ has been enabled.");
        }

        private static void OnOnDutyStateChangedHandler(bool OnDuty)
        {
            if (OnDuty)
            {
                RegisterCallouts();
            }
        }

        private static void RegisterCallouts()
        {
            Functions.RegisterCallout(typeof(Callouts.Investigate.Drug_Deal.Strawberry));
            Functions.RegisterCallout(typeof(Callouts.Crash.Crash));
            Functions.RegisterCallout(typeof(Callouts.Pursuit.Pursuit));
            Functions.RegisterCallout(typeof(Callouts.Pursuit.Roadblock));
        }

        public static Stats GetStats()
        {
            return stats;
        }

        public static Math getMath()
        {
            return math;
        }

        public static VersionState GetState()
        {
            return state;
        }
    }
}
