using System;
using System.Collections.Generic;
using System.Linq;

namespace LssTraining.Web.Data
{
    public class MeterDefinition
    {
        public string MeterName { get; set; } = string.Empty;
        public string CardTitle { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;
        public string Unit { get; set; } = "kWh";
        public string PanelType { get; set; } = "DB";
        public int SortOrder { get; set; }
    }

    public static class ElectricityCatalog
    {
        public const string Building207 = "Building 207";
        public const string Building209 = "Building 209";
        public const string Building238 = "Building 238";

        private static readonly List<MeterDefinition> _allMeters;

        static ElectricityCatalog()
        {
            _allMeters = new List<MeterDefinition>();
            int order = 1;

            void Add(string meterName, string cardTitle, string building, string floor, string subtitle = "", string unit = "kWh", string panelType = "DB")
            {
                _allMeters.Add(new MeterDefinition
                {
                    MeterName = meterName.Trim(),
                    CardTitle = string.IsNullOrWhiteSpace(cardTitle) ? meterName.Trim() : cardTitle.Trim(),
                    Subtitle = subtitle,
                    Building = building,
                    Floor = floor,
                    Unit = unit,
                    PanelType = panelType,
                    SortOrder = order++
                });
            }

            // ══════════════════════════════════════════════════════════════════════════
            // BUILDING 207: Overall Building 207
            // ══════════════════════════════════════════════════════════════════════════
            Add("PM DB SSB1T", "PM DB SSB1T", Building207, "Overall Building 207", "Main Sub-Switchboard", panelType: "SSB");
            Add("PM DB SSB2TA", "PM DB SSB2TA", Building207, "Overall Building 207", "Main Sub-Switchboard 2TA", panelType: "SSB");
            Add("PM DB SSB2TB", "PM DB SSB2TB", Building207, "Overall Building 207", "Main Sub-Switchboard 2TB", panelType: "SSB");
            Add("PM DB SSB3TA", "PM DB SSB3TA", Building207, "Overall Building 207", "Main Sub-Switchboard 3TA", panelType: "SSB");
            Add("PM DB SSB3TB", "PM DB SSB3TB", Building207, "Overall Building 207", "Main Sub-Switchboard 3TB", panelType: "SSB");

            // ══════════════════════════════════════════════════════════════════════════
            // BUILDING 207: Detection 1st floor
            // ══════════════════════════════════════════════════════════════════════════
            Add("DB Testing 1st Floor", "DB Testing 1st Floor", Building207, "Detection 1st floor", "Testing Area Power");
            Add("DB AC Testing", "DB AC Testing", Building207, "Detection 1st floor", "Air Conditioning Testing");
            Add("DB SMT ( GL )", "DB SMT (GL)", Building207, "Detection 1st floor", "Surface Mount Tech");
            Add("Reflow #1", "Reflow #1", Building207, "Detection 1st floor", "SMT Reflow Oven 1");
            Add("Reflow #2", "Reflow #2", Building207, "Detection 1st floor", "SMT Reflow Oven 2");
            Add("DB Reflow #3", "DB Reflow #3", Building207, "Detection 1st floor", "SMT Reflow Oven 3");
            Add("Washing", "Washing", Building207, "Detection 1st floor", "Washing Station");
            Add("AC SMT (GL)", "AC SMT (GL)", Building207, "Detection 1st floor", "SMT Air Conditioning");
            Add("Compressor SMT", "Compressor SMT", Building207, "Detection 1st floor", "Air Compressor SMT");
            Add("DETECTION 1ST FLOOR Real Energy Into the Load", "DETECTION 1ST FLOOR", Building207, "Detection 1st floor", "Real Energy Incomer");
            Add("WAREHOUSE", "WAREHOUSE", Building207, "Detection 1st floor", "Warehouse Power & Light");
            Add("AC_C10K", "AC_C10K", Building207, "Detection 1st floor", "Cleanroom 10K AC");
            Add("C10K_1", "C10K_1", Building207, "Detection 1st floor", "Cleanroom 10K Branch 1");
            Add("C10K_2", "C10K_2", Building207, "Detection 1st floor", "Cleanroom 10K Branch 2");
            Add("L-L250A", "L-L250A", Building207, "Detection 1st floor", "Line Load 250A");

            // ══════════════════════════════════════════════════════════════════════════
            // BUILDING 207: Detection 2nd floor
            // ══════════════════════════════════════════════════════════════════════════
            Add("2Z4", "2Z4", Building207, "Detection 2nd floor", "Zone 2Z4 Supply");
            Add("2ZA", "2ZA", Building207, "Detection 2nd floor", "Zone 2ZA Supply");
            Add("AC AHU 2-1", "AC AHU 2-1", Building207, "Detection 2nd floor", "Air Handling Unit 2-1");
            Add("AC AHU 2-2", "AC AHU 2-2", Building207, "Detection 2nd floor", "Air Handling Unit 2-2");
            Add("DB_FFU_LT2", "DB_FFU_LT2", Building207, "Detection 2nd floor", "Fan Filter Units LT2");
            Add("DB JOGAN", "DB JOGAN", Building207, "Detection 2nd floor", "Jogan Station Power");
            Add("DB_LVP", "DB_LVP", Building207, "Detection 2nd floor", "Low Voltage Panel");
            Add("DB_NCR1", "DB_NCR1", Building207, "Detection 2nd floor", "NCR Line 1");
            Add("DB_NCR2", "DB_NCR2", Building207, "Detection 2nd floor", "NCR Line 2");
            Add("DB_PHASE 6", "DB_PHASE 6", Building207, "Detection 2nd floor", "Phase 6 Production");
            Add("DB_NEW POLARIS", "DB_NEW POLARIS", Building207, "Detection 2nd floor", "Polaris Production");
            Add("DB_POL 10 11", "DB_POL 10-11", Building207, "Detection 2nd floor", "Polaris Line 10-11");
            Add("DB_POL 13-14", "DB_POL 13-14", Building207, "Detection 2nd floor", "Polaris Line 13-14");
            Add("DB_POL 15-17", "DB_POL 15-17", Building207, "Detection 2nd floor", "Polaris Line 15-17");
            Add("DB_POL18-20", "DB_POL 18-20", Building207, "Detection 2nd floor", "Polaris Line 18-20");
            Add("DB_PYRO 1.6", "DB_PYRO 1.6", Building207, "Detection 2nd floor", "Pyro Production Line");
            Add("DB_SAWING", "DB_SAWING", Building207, "Detection 2nd floor", "Sawing Process");
            Add("DB_T1", "DB_T1", Building207, "Detection 2nd floor", "Test Station 1");
            Add("DB_AC_OFFICE_DETECTION", "DB_AC_OFFICE_DETECTION", Building207, "Detection 2nd floor", "Office AC Detection");
            Add("PM_DB_OFFICE_DETECTION", "PM_DB_OFFICE_DETECTION", Building207, "Detection 2nd floor", "Office Power Detection");
            Add("DB_P1", "DB_P1", Building207, "Detection 2nd floor", "Power Panel 1");
            Add("DB_P2", "DB_P2", Building207, "Detection 2nd floor", "Power Panel 2");
            Add("DB4", "DB4", Building207, "Detection 2nd floor", "Distribution Board 4");
            Add("DB_MIELE", "DB_MIELE", Building207, "Detection 2nd floor", "Miele Equipment");
            Add("AC IRD", "AC IRD", Building207, "Detection 2nd floor", "Air Conditioning IRD");
            Add("CHILLER_1-4", "CHILLER_1-4", Building207, "Detection 2nd floor", "Chiller System 1-4");
            Add("DB L3", "DB L3", Building207, "Detection 2nd floor", "Distribution Board L3");
            Add("DB_REALIBILITY", "DB_REALIBILITY", Building207, "Detection 2nd floor", "Reliability Test Lab");
            Add("VACUMP", "VACUMP", Building207, "Detection 2nd floor", "Vacuum Pump System");

            // ══════════════════════════════════════════════════════════════════════════
            // BUILDING 207: Detection 3rd floor
            // ══════════════════════════════════════════════════════════════════════════
            Add("3Z1 ( ex SMD )", "3Z1 (ex SMD)", Building207, "Detection 3rd floor", "Zone 3Z1 Area");
            Add("3Z1A (ex-Testing)", "3Z1A (ex-Testing)", Building207, "Detection 3rd floor", "Zone 3Z1A Area");
            Add("AC 3rd Floor", "AC 3rd Floor", Building207, "Detection 3rd floor", "Floor Air Conditioning");
            Add("LIGHTING FTC", "LIGHTING FTC", Building207, "Detection 3rd floor", "FTC Lighting System");
            Add("WORKSHOP", "WORKSHOP", Building207, "Detection 3rd floor", "Maintenance Workshop");
            Add("DB_REALIBILITY", "DB_REALIBILITY (LT3)", Building207, "Detection 3rd floor", "Reliability Chamber LT3");

            // ══════════════════════════════════════════════════════════════════════════
            // BUILDING 207: Detection Special Lighting floor
            // ══════════════════════════════════════════════════════════════════════════
            Add("AC SL", "AC SL", Building207, "Detection Special Lighting floor", "Air Conditioning Special Lighting");
            Add("Lighting Hydra", "Lighting Hydra", Building207, "Detection Special Lighting floor", "Hydra Lighting Circuit");
            Add("DB SL200A", "DB SL200A", Building207, "Detection Special Lighting floor", "Special Lighting 200A Panel");
            Add("HYD #1", "HYD #1", Building207, "Detection Special Lighting floor", "Hydraulic Machine 1");
            Add("HYD #2", "HYD #2", Building207, "Detection Special Lighting floor", "Hydraulic Machine 2");
            Add("HYD #3", "HYD #3", Building207, "Detection Special Lighting floor", "Hydraulic Machine 3");
            Add("HYD #4", "HYD #4", Building207, "Detection Special Lighting floor", "Hydraulic Machine 4");
            Add("DB A", "DB A", Building207, "Detection Special Lighting floor", "Distribution Board A");
            Add("DB UV", "DB UV", Building207, "Detection Special Lighting floor", "UV Curing Station");
            Add("DB_H", "DB_H", Building207, "Detection Special Lighting floor", "Distribution Board H");
            Add("DB_PHASE 1", "DB_PHASE 1", Building207, "Detection Special Lighting floor", "Phase 1 Lighting Production");
            Add("DB_PHASE 2", "DB_PHASE 2", Building207, "Detection Special Lighting floor", "Phase 2 Lighting Production");
            Add("AC 250A LSM", "AC 250A LSM", Building207, "Detection Special Lighting floor", "Air Conditioning 250A LSM");
            Add("CNC", "CNC", Building207, "Detection Special Lighting floor", "CNC Machinery Panel");

            // ══════════════════════════════════════════════════════════════════════════
            // BUILDING 209: Overall Building 209
            // ══════════════════════════════════════════════════════════════════════════
            Add("PM SSB1TA", "PM SSB1TA", Building209, "Overall Building 209", "Sub-Switchboard 1TA", panelType: "SSB");
            Add("PM SSB1TB", "PM SSB1TB", Building209, "Overall Building 209", "Sub-Switchboard 1TB", panelType: "SSB");
            Add("PM SSB2TA", "PM SSB2TA", Building209, "Overall Building 209", "Sub-Switchboard 2TA", panelType: "SSB");
            Add("PM SSB2TB", "PM SSB2TB", Building209, "Overall Building 209", "Sub-Switchboard 2TB", panelType: "SSB");
            Add("PM SSB3TB", "PM SSB3TB", Building209, "Overall Building 209", "Sub-Switchboard 3TB", panelType: "SSB");

            // ══════════════════════════════════════════════════════════════════════════
            // BUILDING 209: 1st floor
            // ══════════════════════════════════════════════════════════════════════════
            Add("PM_LOT209_LT1.PM_AC_CL2 Real Energy Into the Load", "PM_AC_CL2", Building209, "1st floor", "Air Conditioning Cleanroom 2");
            Add("PM_LOT209_LT1.PM_CHILLER Real Energy Into the Load", "PM_CHILLER", Building209, "1st floor", "Central Chiller LT1");
            Add("PM_LOT209_LT1.PM_DB_CHILLER Real Energy Into the Load", "PM_DB_CHILLER", Building209, "1st floor", "Chiller Power Panel");
            Add("PM_LOT209_LT1.PM_DBCanteen Real Energy Into the Load", "PM_DBCanteen", Building209, "1st floor", "Canteen Power & Kitchen");
            Add("PM_LOT209_LT1.PM_DBRO Real Energy Into the Load", "PM_DBRO", Building209, "1st floor", "Reverse Osmosis Water System");
            Add("PM_LOT209_LT1.PM_MDB400A Real Energy Into the Load", "PM_MDB400A", Building209, "1st floor", "Main Distribution Board 400A", panelType: "MDB");
            Add("PM_LOT209_LT1.PM_ULTRASONIC1 Real Energy Into the Load", "PM_ULTRASONIC1", Building209, "1st floor", "Ultrasonic Cleaning 1");
            Add("PM_LOT209_LT1.PM_ULTRASONIC2 Real Energy Into the Load", "PM_ULTRASONIC2", Building209, "1st floor", "Ultrasonic Cleaning 2");
            Add("PM_LOT209_LT1.PM_ULTRASONIC3 Real Energy Into the Load", "PM_ULTRASONIC3", Building209, "1st floor", "Ultrasonic Cleaning 3");
            Add("PM_LOT209_LT1.PM_ULTRASONIC4 Real Energy Into the Load", "PM_ULTRASONIC4", Building209, "1st floor", "Ultrasonic Cleaning 4");
            Add("PM_LOT209_LT1.PM_WWTP Real Energy Into the Load", "PM_WWTP", Building209, "1st floor", "Wastewater Treatment Plant");
            Add("PM_LOT209_LT1.PMDB_AC320A Real Energy Into the Load", "PMDB_AC320A", Building209, "1st floor", "Air Conditioning 320A");
            Add("PM_LOT209_LT1.PMDB_COATING Real Energy Into the Load", "PMDB_COATING", Building209, "1st floor", "Optical Coating Line");
            Add("PM_LOT209_LT1.PMDB_COMPRESSOR Real Energy Into the Load", "PMDB_COMPRESSOR", Building209, "1st floor", "Compressed Air System");
            Add("PM_LOT209_LT1.PMDB_LGHTNG Real Energy Into the Load", "PMDB_LGHTNG", Building209, "1st floor", "Factory Lighting Panel");
            Add("PM_LOT209_LT1.PMDB_LIGHTING_2 Real Energy Into the Load", "PMDB_LIGHTING_2", Building209, "1st floor", "Office Lighting Panel 2");
            Add("PM_LOT209_LT1.PMDB_MC1 Real Energy Into the Load", "PMDB_MC1", Building209, "1st floor", "Motor Center 1");
            Add("PM_LOT209_LT1.PMDB_MC2 Real Energy Into the Load", "PMDB_MC2", Building209, "1st floor", "Motor Center 2");
            Add("PM_LOT209_LT1.PMDB_MC3 Real Energy Into the Load", "PMDB_MC3", Building209, "1st floor", "Motor Center 3");
            Add("PM_LOT209_LT1.PMDB_POWER_100A Real Energy Into the Load", "PMDB_POWER_100A", Building209, "1st floor", "Power Panel 100A");
            Add("PM_LOT209_LT1.PMDB_POWER_EXHAUST Real Energy Into the Load", "PMDB_POWER_EXHAUST", Building209, "1st floor", "Exhaust Ventilation Power");
            Add("PM_LOT209_LT1.PMMDB_320A Real Energy Into the Load", "PMMDB_320A", Building209, "1st floor", "Main Distribution Board 320A", panelType: "MDB");
            Add("Total SSB Lt# 1 209 Real Energy Into the Load", "Total SSB Lt# 1 209", Building209, "1st floor", "Total Floor Incomer", panelType: "SSB");

            // ══════════════════════════════════════════════════════════════════════════
            // BUILDING 209: 2nd floor
            // ══════════════════════════════════════════════════════════════════════════
            Add("PM_LOT209_LT2.PM_ASTROFURNACE2 Real Energy Into the Load", "PM_ASTROFURNACE2", Building209, "2nd floor", "Astro Furnace 2");
            Add("PM_LOT209_LT2.PM_ASTROFURNACE4 Real Energy Into the Load", "PM_ASTROFURNACE4", Building209, "2nd floor", "Astro Furnace 4");
            Add("PM_LOT209_LT2.PM_CAMCOFURNACE7 Real Energy Into the Load", "PM_CAMCOFURNACE7", Building209, "2nd floor", "Camco Furnace 7");
            Add("PM_LOT209_LT2.PM_CAMCOFURNACE8 Real Energy Into the Load", "PM_CAMCOFURNACE8", Building209, "2nd floor", "Camco Furnace 8");
            Add("PM_LOT209_LT2.PM_CHILLER13 Real Energy Into the Load", "PM_CHILLER13", Building209, "2nd floor", "Chiller Unit 13");
            Add("PM_LOT209_LT2.PM_LOT209_EXHAUST2_LT2 Real Energy Into the Load", "PM_EXHAUST2_LT2", Building209, "2nd floor", "Exhaust System 2");
            Add("PM_LOT209_LT2.PM_NIPLATING Real Energy Into the Load", "PM_NIPLATING", Building209, "2nd floor", "Nickel Plating Line");
            Add("PM_LOT209_LT2.PMDB_AGING1 Real Energy Into the Load", "PMDB_AGING1", Building209, "2nd floor", "Burn-in / Aging 1");
            Add("PM_LOT209_LT2.PMDB_AGING2 Real Energy Into the Load", "PMDB_AGING2", Building209, "2nd floor", "Burn-in / Aging 2");
            Add("PM_LOT209_LT2.PMDB_ASTRO Real Energy Into the Load", "PMDB_ASTRO", Building209, "2nd floor", "Astro Line Main DB");
            Add("PM_LOT209_LT2.PMDB1_250A Real Energy Into the Load", "PMDB1_250A", Building209, "2nd floor", "Power Panel 1 (250A)");
            Add("PM_LOT209_LT2.PMDB1_320A Real Energy Into the Load", "PMDB1_320A", Building209, "2nd floor", "Power Panel 1 (320A)");
            Add("PM_LOT209_LT2.PMDB2_320A Real Energy Into the Load", "PMDB2_320A", Building209, "2nd floor", "Power Panel 2 (320A)");
            Add("PM_LOT209_LT2.PMDB2_500A Real Energy Into the Load", "PMDB2_500A", Building209, "2nd floor", "Power Panel 2 (500A)");
            Add("PM_LOT209_LT2.PMDB3_320A Real Energy Into the Load", "PMDB3_320A", Building209, "2nd floor", "Power Panel 3 (320A)");
            Add("PM_LOT209_LT2.PMDB3_500A Real Energy Into the Load", "PMDB3_500A", Building209, "2nd floor", "Power Panel 3 (500A)");
            Add("PM_LOT209_LT2.PMDB4_LIGHTING Real Energy Into the Load", "PMDB4_LIGHTING", Building209, "2nd floor", "Lighting Distribution 4");
            Add("PM_LOT209_LT2.PMPANEL_EXHAUST Real Energy Into the Load", "PMPANEL_EXHAUST", Building209, "2nd floor", "Panel Exhaust Fans");
            Add("Total SSB Lt# 2 209 Real Energy Into the Load", "Total SSB Lt# 2 209", Building209, "2nd floor", "Total Floor Incomer", panelType: "SSB");

            // ══════════════════════════════════════════════════════════════════════════
            // BUILDING 209: 3rd floor
            // ══════════════════════════════════════════════════════════════════════════
            Add("PM_LOT209_LT3.PM_CHILLER_ROOFTOP Real Energy Into the Load", "PM_CHILLER_ROOFTOP", Building209, "3rd floor", "Rooftop Chiller Plant");
            Add("PM_LOT209_LT3.PMDB_COMPRESSOR_LT3 Real Energy Into the Load", "PMDB_COMPRESSOR_LT3", Building209, "3rd floor", "Compressor Room LT3");
            Add("PM_LOT209_LT3.PMDB_EXHAUST_LT3 Real Energy Into the Load", "PMDB_EXHAUST_LT3", Building209, "3rd floor", "Exhaust Ventilation LT3");
            Add("PM_LOT209_LT3.PMDB_LIGHTING_LT3 Real Energy Into the Load", "PMDB_LIGHTING_LT3", Building209, "3rd floor", "Floor Lighting LT3");
            Add("PM_LOT209_LT3.PMDB_MC_LT3 Real Energy Into the Load", "PMDB_MC_LT3", Building209, "3rd floor", "Motor Control LT3");
            Add("PM_LOT209_LT3.PMDB_MC2_LT3 Real Energy Into the Load", "PMDB_MC2_LT3", Building209, "3rd floor", "Motor Control 2 LT3");
            Add("PM_LOT209_LT3.PMDB_POWER_LT3 Real Energy Into the Load", "PMDB_POWER_LT3", Building209, "3rd floor", "General Power LT3");
            Add("PM_LOT209_LT3.PMMDB_400A_LT3 Real Energy Into the Load", "PMMDB_400A_LT3", Building209, "3rd floor", "Main Distribution Board 400A", panelType: "MDB");
            Add("PM_LOT209_LT3.PMSSB3TB Real Energy Into the Load", "PMSSB3TB", Building209, "3rd floor", "Sub-Switchboard 3TB LT3", panelType: "SSB");
            Add("PM_LOT209_LT3.MDB_200A_AC_TC Real Energy Into the Load", "MDB_200A_AC_TC", Building209, "3rd floor", "AC & TC Main Panel 200A", panelType: "MDB");
            Add("PM_LOT209_LT3.L_P Real Energy Into the Load", "L_P", Building209, "3rd floor", "Light & Power Feed");
            Add("PM_LOT209_LT3.MC_TC Real Energy Into the Load", "MC_TC", Building209, "3rd floor", "Motor Control TC");
            Add("PM_LOT209_LT3.P Real Energy Into the Load", "P", Building209, "3rd floor", "Power Incomer");

            // ══════════════════════════════════════════════════════════════════════════
            // BUILDING 238: Main
            // ══════════════════════════════════════════════════════════════════════════
            Add("PM_LOT238.PM_MAIN Voltage A-B (V)", "PM_MAIN", Building238, "Overall / Main", "Main Voltage A-B Phase", unit: "V", panelType: "MAIN");
        }

        public static IReadOnlyList<MeterDefinition> AllMeters => _allMeters;

        public static List<string> GetBuildings()
        {
            return new List<string> { Building207, Building209, Building238 };
        }

        public static List<string> GetFloors(string? building)
        {
            if (string.IsNullOrWhiteSpace(building) || building.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                return _allMeters.Select(m => m.Floor).Distinct().ToList();
            }

            return _allMeters
                .Where(m => m.Building.Equals(building, StringComparison.OrdinalIgnoreCase))
                .Select(m => m.Floor)
                .Distinct()
                .ToList();
        }

        public static MeterDefinition? FindMeter(string meterName)
        {
            if (string.IsNullOrWhiteSpace(meterName)) return null;

            string clean = meterName.Trim();
            // 1. Exact match
            var found = _allMeters.FirstOrDefault(m => m.MeterName.Equals(clean, StringComparison.OrdinalIgnoreCase));
            if (found != null) return found;

            // 2. Partial prefix / suffix match
            found = _allMeters.FirstOrDefault(m =>
                clean.Contains(m.MeterName, StringComparison.OrdinalIgnoreCase) ||
                m.MeterName.Contains(clean, StringComparison.OrdinalIgnoreCase) ||
                clean.Contains(m.CardTitle, StringComparison.OrdinalIgnoreCase));

            return found;
        }

        public static MeterDefinition ResolveMeter(string meterName)
        {
            var found = FindMeter(meterName);
            if (found != null) return found;

            // Graceful fallback parser for unlisted database meters
            string clean = meterName.Trim();
            string upper = clean.ToUpper();
            string building = Building207;
            string floor = "General Floor";

            if (upper.Contains("209") || upper.Contains("LOT209"))
            {
                building = Building209;
                if (upper.Contains("LT1") || upper.Contains("LT 1")) floor = "1st floor";
                else if (upper.Contains("LT2") || upper.Contains("LT 2")) floor = "2nd floor";
                else if (upper.Contains("LT3") || upper.Contains("LT 3")) floor = "3rd floor";
            }
            else if (upper.Contains("238") || upper.Contains("LOT238"))
            {
                building = Building238;
                floor = "Overall / Main";
            }
            else
            {
                building = Building207;
                if (upper.Contains("1ST") || upper.Contains("LT1") || upper.Contains("LT 1")) floor = "Detection 1st floor";
                else if (upper.Contains("2ND") || upper.Contains("LT2") || upper.Contains("LT 2")) floor = "Detection 2nd floor";
                else if (upper.Contains("3RD") || upper.Contains("LT3") || upper.Contains("LT 3")) floor = "Detection 3rd floor";
                else if (upper.Contains("SL") || upper.Contains("LIGHTING")) floor = "Detection Special Lighting floor";
                else if (upper.Contains("SSB")) floor = "Overall Building 207";
            }

            string unit = upper.Contains("(V)") || upper.Contains("VOLT") ? "V" : "kWh";
            string title = clean.Contains('.') ? clean.Substring(clean.IndexOf('.') + 1) : clean;
            if (title.Contains(" Real Energy", StringComparison.OrdinalIgnoreCase))
            {
                title = title.Replace(" Real Energy Into the Load", "").Trim();
            }

            return new MeterDefinition
            {
                MeterName = clean,
                CardTitle = title,
                Subtitle = "Distribution Board",
                Building = building,
                Floor = floor,
                Unit = unit,
                PanelType = "DB",
                SortOrder = 999
            };
        }
    }
}
