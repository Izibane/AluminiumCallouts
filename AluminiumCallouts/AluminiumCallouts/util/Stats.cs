using Rage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AluminiumCallouts
{
    public class Stats
    {
        public long started = 0;

        public int successful = 0, errors = 0, arrests = 0, suspects = 0, officers = 0, vehicles = 0, callouts = 0;

        public void updateStats(bool success, int arrests, int suspects, int officers, int vehicles)
        {
            callouts += 1;
            if(success)
            {
                successful += 1;
            } else
            {
                errors += 1;
            }

            addArrests(arrests);

            addSuspects(suspects);

            addOfficers(officers);

            addVehicles(vehicles);
        }

        public void addArrests(int toadd)
        {
            arrests += toadd;
        }

        public void addSuspects(int toadd)
        {
            suspects += toadd;
        }

        public void addOfficers(int toadd)
        {
            officers += toadd;
        }

        public void addVehicles(int toadd)
        {
            vehicles += toadd;
        }

        public void printStatistics()
        {
            Game.LogTrivial("----------------------------------");
            Game.LogTrivial("[AlumininumCallouts]" + " Statistics for session (" + ((System.DateTime.Now.Millisecond - started) / 1000) + " seconds)");
            Game.LogTrivial("Callouts Total: " + callouts);
            Game.LogTrivial("Callouts Errors: " + errors + " (epc: " + Main.getMath().divideNumber(errors, callouts) + ")");
            Game.LogTrivial("Vehicles spawned: " + vehicles + " (vpc: " + Main.getMath().divideNumber(vehicles, callouts) + ")");
            Game.LogTrivial("Suspects spawned: " + suspects + "(spc: " + Main.getMath().divideNumber(suspects, callouts) + ")");
            Game.LogTrivial("Officers spawned: " + officers + "(opc: " + Main.getMath().divideNumber(officers, callouts) + ")");
            Game.LogTrivial("----------------------------------");
        }
    }
}
