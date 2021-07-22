using LSPD_First_Response.Mod.API;
using Rage;
using System;
using System.Linq;

namespace AluminiumCallouts.util
{
    class CalloutUtil
    {

        internal static bool IsBetterEMSRunning() => IsLspdfrPluginRunning("BetterEMS", new Version("3.0.6298.2858"));
        private static bool IsLspdfrPluginRunning(string plugin, Version minversion = null) => Functions.GetAllUserPlugins().Select(assembly => assembly.GetName()).Where(an => an.Name.ToLower() == plugin.ToLower()).Any(an => minversion == null || an.Version.CompareTo(minversion) >= 0);

        public static String getRandomDrugType()
        {
            String[] drugs = { "Heroin", "Weed", "Methamphetimine", "Xanax", "Fentanyl", "Amphetamine", "Opium", "Cocaine", "LSD", "MDMA", "Ketamine", "Drug Money", "White Powder" };
            return drugs[Main.getMath().getRandomInt(0, drugs.Length)];
        }

        public static void DisplayInformation(String title, String subTitle, String information)
        {
            Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", title, subTitle, information);
        }

        public static void DebugInformation(String title, String subTitle, String information)
        {
            if(Main.GetState() == VersionState.DEVELOPMENT) 
                Game.DisplayNotification("3dtextures", "mpgroundlogo_cops", title, subTitle, information);
        }

        public static String getRandomCar()
        {
            //String[] models = { "NINEF", "NINEF2", "ASEA", "ASEA2", "ASTEROPE", "BALLER", "BALLER2", "BANSHEE", "BENSON", "BISON", "BISON2", "BISON3", "BUCCANEER", "BUFFALO", "BUFFALO2", "BULLET", "COQUETTE", "GRESLEY", "ELEGY2", "FQ2", "F620", "FUSILADE", "CASCO", "FELON", "JACKAL", "ZION", "ZION2", "MESA", "XLS", "DILETTANTE", "INGOT", "INTRUDER", "PRIMO", "MINIVAN", "MINIVAN2", "YOUGA", "SPEEDO", "RUMPO", "VOLTIC", "NERO", "BTYPE", "OMNIS", "SPECTER", "SULTAN", "TROPOS", "MAMBA", "MANANA", "STINGER", "STINGERGT", "TORNADO", "PENUMBRA", "ALPHA", "FUTO", "RUSTON", "JESTER", "LYNX", "MASSACRO", "LIMO2", "XLS", "ROCOTO", "RANCHERXL", "BJXL", "SANDKING", "SANDKING2", "MARSHALL", "DUNE", "BRAWLER", "VIRGO", "VIGERO"};
            String[] models = { "ADDER", "ZENTORNO", "TYRUS", "VACCA", "VAGNER", "ITALIGTB", "OSIRIS", "REAPER", "BULLET", "CHEETAH", "ENTITYXF", "GP1", "FMJ", "INFERNUS", "RE7B", "NERO", "NERO2", "PFISTER811", "T20", "TEMPESTA" };
            return models[Main.getMath().getRandomInt(0, models.Length)];
        }

        public static String getRandomPlaceInCar()
        {
            String[] places = { "Sidedoor", "Backseat", "Glove compartment", "Footwell", "Boot"};
            return places[Main.getMath().getRandomInt(0, places.Length)];
        }

        public static String getRandomCityUnit()
        {
            String[] police = { "POLICE", "POLICE2", "POLICE3", "POLICE4" };
            return police[Main.getMath().getRandomInt(0, police.Length)];
        }

        public static String getRandomStateUnit()
        {
            String[] police = { "SHERIFF", "SHERIFF2" };
            return police[Main.getMath().getRandomInt(0, police.Length)];
        }

    }
}
