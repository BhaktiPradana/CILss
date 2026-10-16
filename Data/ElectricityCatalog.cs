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

            void AddActual(string meterName, string building, string floor, string unit, string panelType)
            {
                string title = meterName.Contains(".") ? meterName.Substring(meterName.IndexOf(".") + 1) : meterName;
                _allMeters.Add(new MeterDefinition
                {
                    MeterName = meterName,
                    CardTitle = title,
                    Subtitle = panelType == "SSB" ? "Sub-Switchboard" : panelType == "MDB" ? "Main Distribution Board" : panelType == "PL" ? "Power line / circuit" : "Distribution Board",
                    Building = building,
                    Floor = floor,
                    Unit = unit,
                    PanelType = panelType,
                    SortOrder = order++
                });
            }

            // Exact meter names from the latest actual list. Keep names unchanged for database matching.
            AddActual("PM_LOT207_LT1.DB_AC_C10K", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_AC_GL", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_AC_OFFICE", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_AC_TESTING", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_C10K_1", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_C10K_2", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_CANTEEN", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_CANTEEN-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.DB_CANTEEN-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.DB_FAC_LT1", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_FFU_LT1", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_FFU_LT1-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.DB_FFU_LT1-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.DB_LIGHTING", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_LL_250A", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_NEWRO_300A", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_OFFICE", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_ROWATER", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_TESTING", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_WAREHOUSE", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CHILLER", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CHILLER_2-3", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CHILLER_2-3-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CHILLER_2-3-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_COMPRESSOR_LT1", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR1", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR1-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR1-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR2", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR2-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR2-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR4", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR4-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR4-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR5", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR5-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR5-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_CPCOMAIR-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_POWER_GL2", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_POWER_GL2-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_POWER_GL2-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_VACUMPUMP", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_VACUMPUMP-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DB_VACUMPUMP-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DBPOWER_1808EXL", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DBPOWER_1808EXL-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DBPOWER_1808EXL-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DBPOWER_1809EXL", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DBPOWER_1809EXL-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DBPOWER_1809EXL-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DBPOWER_GL", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DBPOWER_GL-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DBPOWER_GL-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DBWASHING", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_DBWASHING-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_DBWASHING-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PMDB_AC320A", Building207, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT207_LT1.PMDB_COMPAIR1", Building207, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT207_LT1.PMDB_LIGHTING", Building207, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT207_LT1.PMDB_POWER_EXHAUST", Building207, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT207_LT2.DB_2Z4", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_2ZA", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_AHU21", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_AHU21-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_AHU21-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_AHU22", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_AHU22-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_AHU22-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_FFU_LT2", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_JOGAN", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_JOGAN-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_JOGAN-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_LVP", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_NCR1", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_NCR1-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_NCR1-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_NCR2", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_NCR2-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_NCR2-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_NEWPOLARIS", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_NEWPOLARIS-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_NEWPOLARIS-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_PHASE6", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_PHASE6-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_PHASE6-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_POL1011", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_POL1011-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_POL1011-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_POL1517", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_POL1820", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_POL1820-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_POL1820-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_PYRO", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_PYRO-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_PYRO-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_SAWING", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_SAWING-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_SAWING-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.PM_DB_AC_IRD", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.PM_DB_AC_IRD-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.PM_DB_AC_IRD-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.PM_DB_P1", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.PM_DB_P1-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.PM_DB_P1-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.PM_DB_P2", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.PM_DB_P2-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.PM_DB_P2-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.PM_DB4", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.PM_DB4-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.PM_DB4-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_3Z1A", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_3Z1A-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_3Z1A-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_AC_LT3", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_AC_LT3-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_AC_LT3-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_AC_SL", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_AC_SL-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_AC_SL-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_ACSL_3RDFLOOR", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_CNC", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_EXSMD", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_EXSMD-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_EXSMD-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_H", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_H-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_H-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_LIGHTING_FTC", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_LIGHTING_FTC-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_LIGHTING_FTC-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_RDRY", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_RDRY-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_RDRY-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_REALIBILITY", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_REALIBILITY-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_REALIBILITY-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_SL200A", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_SL200A-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_SL200A-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_WORKSHOP", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_WORKSHOP-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_WORKSHOP-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DBFFU", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DBFFU-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DBFFU-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.MDB350A", Building207, "3rd floor", "kWh", "MDB");
            AddActual("PM_LOT207_LT3.MDB350A-thd_VoltageL-L", Building207, "3rd floor", "V", "MDB");
            AddActual("PM_LOT207_LT3.MDB350A-thd_VoltageL-N", Building207, "3rd floor", "V", "MDB");
            AddActual("PM_LOT207_LT3.PM_DB_A", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.PM_DB_A-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.PM_DB_A-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.PM_DB_PHASE1", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.PM_DB_PHASE1-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.PM_DB_PHASE1-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.PM_DB_PHASE2", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.PM_DB_PHASE2-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.PM_DB_PHASE2-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.PM_DBCOMPAIR7", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.PM_DBCOMPAIR7-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.PM_DBCOMPAIR7-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.PMDB_CPCOMPAIR", Building207, "3rd floor", "kWh", "MDB");
            AddActual("PM_LOT207_LT3.PMDBUV", Building207, "3rd floor", "kWh", "MDB");
            AddActual("PM_LOT207_ROOFTOP.DB_CHILLER_AHU", Building207, "Rooftop", "kWh", "PM");
            AddActual("PM_LOT207_ROOFTOP.DB_CHILLER_AHU-thd_VoltageL-L", Building207, "Rooftop", "V", "PM");
            AddActual("PM_LOT207_ROOFTOP.DB_CHILLER_AHU-thd_VoltageL-N", Building207, "Rooftop", "V", "PM");
            AddActual("PM_LOT207_ROOFTOP.DB_CHILLER150A", Building207, "Rooftop", "kWh", "PM");
            AddActual("PM_LOT207_ROOFTOP.DB_INHOUSELIFT", Building207, "Rooftop", "kWh", "PM");
            AddActual("PM_LOT207_ROOFTOP.DB_INHOUSELIFT-thd_VoltageL-L", Building207, "Rooftop", "V", "PM");
            AddActual("PM_LOT207_ROOFTOP.DB_INHOUSELIFT-thd_VoltageL-N", Building207, "Rooftop", "V", "PM");
            AddActual("PM_LOT209_LT1.DB_COMPRESSOR", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.DB_COMPRESSOR-thd_VoltageL-L", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.DB_COMPRESSOR-thd_VoltageL-N", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.MDB_320A", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.MDB_320A-thd_VoltageL-L", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.MDB_320A-thd_VoltageL-N", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PM_DB_CHILLER", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.PM_DB_CHILLER-thd_VoltageL-L", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PM_DB_CHILLER-thd_VoltageL-N", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PM_DB_COMPRESSOR_DIWATER", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.PM_DB_COMPRESSOR500A", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.PM_DBAC320", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.PM_DBCanteen", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.PM_DBCanteen-thd_VoltageL-L", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PM_DBCanteen-thd_VoltageL-N", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PM_DBRO", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.PM_DBRO-thd_VoltageL-L", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PM_DBRO-thd_VoltageL-N", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PM_LOT209_DBMC4_LT1", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.PM_MDB_400A", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PM_MDB400A_CHILLER", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PM_MDB400A_CHILLER-thd_VoltageL-L", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PM_MDB400A_CHILLER-thd_VoltageL-N", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_COATING", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_COATING-thd_VoltageL-L", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_COATING-thd_VoltageL-N", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_COMPRESSOR_500A", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_COMPRESSOR500A", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_LGHTNG", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_LGHTNG-thd_VoltageL-L", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_LGHTNG-thd_VoltageL-N", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_LIGHTING", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_LIGHTING-thd_VoltageL-L", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_LIGHTING-thd_VoltageL-N", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_MC1", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_MC1-thd_VoltageL-L", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_MC1-thd_VoltageL-N", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_MC2", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_MC2-thd_VoltageL-L", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_MC2-thd_VoltageL-N", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_MC3", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_MC3-thd_VoltageL-L", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_MC3-thd_VoltageL-N", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_POWER_100A", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_POWER_100A-thd_VoltageL-L", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_POWER_100A-thd_VoltageL-N", Building209, "1st floor", "V", "MDB");
            AddActual("PM_LOT209_LT1.RGU_DB_COMPRESSOR_500A", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.DB1_B14_ABRAZING", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PM_DB_NEW_OFFICE", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PM_DB200A_CBR", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PM_DBASTRO", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PM_DBMC630A_280V_CBR", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PMDB_AGING1", Building209, "2nd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT2.PMDB_AGING1-thd_VoltageL-L", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB_AGING1-thd_VoltageL-N", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB_AGING2", Building209, "2nd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT2.PMDB_AGING2-thd_VoltageL-L", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB_AGING2-thd_VoltageL-N", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB_L3", Building209, "2nd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT2.PMDB1_250A", Building209, "2nd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT2.PMDB1_250A-thd_VoltageL-L", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB1_250A-thd_VoltageL-N", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB1_320A", Building209, "2nd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT2.PMDB1_320A-thd_VoltageL-L", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB1_320A-thd_VoltageL-N", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB2_320A", Building209, "2nd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT2.PMDB2_320A-thd_VoltageL-L", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB2_320A-thd_VoltageL-N", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB2_500A", Building209, "2nd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT2.PMDB2_500A-thd_VoltageL-L", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB2_500A-thd_VoltageL-N", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB3_320A", Building209, "2nd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT2.PMDB3_320A-thd_VoltageL-L", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB3_320A-thd_VoltageL-N", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB3_500A", Building209, "2nd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT2.PMDB3_500A-thd_VoltageL-L", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB3_500A-thd_VoltageL-N", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB4_LIGHTING", Building209, "2nd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT2.PMDB4_LIGHTING-thd_VoltageL-L", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.PMDB4_LIGHTING-thd_VoltageL-N", Building209, "2nd floor", "V", "MDB");
            AddActual("PM_LOT209_LT2.RGU_DB1_250A", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2_DB2_500A.B1_TIG", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2_DB2_500A.R2_TIG", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT3.MDB_320A", Building209, "3rd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT3.MDB_320A-thd_VoltageL-L", Building209, "3rd floor", "V", "MDB");
            AddActual("PM_LOT209_LT3.MDB_320A-thd_VoltageL-N", Building209, "3rd floor", "V", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_COMPRESSOR_LT3", Building209, "3rd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_COMPRESSOR_LT3-thd_VoltageL-L", Building209, "3rd floor", "V", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_COMPRESSOR_LT3-thd_VoltageL-N", Building209, "3rd floor", "V", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_EXHAUST", Building209, "3rd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_EXHAUST-thd_VoltageL-L", Building209, "3rd floor", "V", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_EXHAUST-thd_VoltageL-N", Building209, "3rd floor", "V", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_LIGHTING_LT3", Building209, "3rd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_LIGHTING_LT3-thd_VoltageL-L", Building209, "3rd floor", "V", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_LIGHTING_LT3-thd_VoltageL-N", Building209, "3rd floor", "V", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_MC_LT3", Building209, "3rd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_MC2_LT3", Building209, "3rd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_MC2_LT3-thd_VoltageL-L", Building209, "3rd floor", "V", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_MC2_LT3-thd_VoltageL-N", Building209, "3rd floor", "V", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_POWER_LT3", Building209, "3rd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_POWER_LT3-thd_VoltageL-L", Building209, "3rd floor", "V", "MDB");
            AddActual("PM_LOT209_LT3.PMDB_POWER_LT3-thd_VoltageL-N", Building209, "3rd floor", "V", "MDB");
            AddActual("PM_LOT209_LT3.PMMDB_400A", Building209, "3rd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT3.PMMDB_400A-thd_VoltageL-L", Building209, "3rd floor", "V", "MDB");
            AddActual("PM_LOT209_LT3.PMMDB_400A-thd_VoltageL-N", Building209, "3rd floor", "V", "MDB");
            AddActual("ELM_LOT207_LT1.RGU_AC_GL", Building207, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT1.RGU_AC_OFFICE", Building207, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT1.RGU_AC_TESTING", Building207, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT1.RGU_DB_POWER_GL2", Building207, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT1.RGU_POWER_GL", Building207, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT1.RGU_WAREHOUSE", Building207, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.2Z4", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.AHU21", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.AHU22", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.CBS4_DB_MIELE200A_CH2", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.CBS4_DB_MEILE", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.CBS4_DB_MIELE200A_CH1", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.CBS4_DB_MIELE200A_CH3", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.CBS4_DB_MIELE200A_CH4", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.DB_FFU_LT2", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.DB_POL1820", Building207, "2nd floor", "kWh", "DB");
            AddActual("PM_LOT207_LT2.DB_T1", Building207, "2nd floor", "kWh", "PM");
            AddActual("ELM_LOT207_LT2.JOGAN", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.LVP_A", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.LVP_B", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.NCR1", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.NCR2", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.NEWPOLARIS", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.PMDB_L3", Building207, "2nd floor", "kWh", "MDB");
            AddActual("ELM_LOT207_LT2.POL1011", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.POL1517", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.RGU_DB_4", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.RGU_DB_NEW_OFFICE", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.RGU_DB_2Z4", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.RGU_DB_L3", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.RGU_DB_P1", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.RGU_DB_P2", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.RGU_DB_PYRO", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.SAWING", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT2.T1", Building207, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT3.DB_SL200A", Building207, "3rd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT3.DB_A", Building207, "3rd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT3.DB_H", Building207, "3rd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT3.DB_REALIBILITY", Building207, "3rd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT3.DB_WORKSHOP", Building207, "3rd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT3.RGU_DB_PHASE1", Building207, "3rd floor", "kWh", "DB");
            AddActual("ELM_LOT207_LT3.RGU_DB_PHASE2", Building207, "3rd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_COMPRESSOR_DIWATER", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DB_CHILLER", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DB_COATING", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DB_COMPRESSOR", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DB_LIGHTING", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DB_MC1", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DB_COATING_A", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DB_MC2", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DB_POWER100A", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DBAC320A_2", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DBAC320A_A", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DBLIGHTING", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DBMC3_1", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DBMC3_2", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DBMC3_3", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DBMC3_4", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DBMC3_5", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DBMC3_MAIN", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_DBPOWEREXHAUST", Building209, "1st floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT1.RGU_MDB_320A", Building209, "1st floor", "kWh", "MDB");
            AddActual("ELM_LOT209_LT2.ELMDB1_250A", Building209, "2nd floor", "kWh", "MDB");
            AddActual("ELM_LOT209_LT2.RGU_AGING1", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_AGING2", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB_EXHAUST", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB1_2501_1", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB1_250A_2", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB1_320A", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB1_250A_3", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB2_320A", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB2_320A_1", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB2_320A_2", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB1_250A", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB2_500A", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB3_320A", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB3_500A", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB3_500A_1", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB3_500A_2", Building209, "2nd floor", "kWh", "DB");
            AddActual("ELM_LOT209_LT2.RGU_DB4_LIGHTING", Building209, "2nd floor", "kWh", "DB");
            AddActual("LOGINSERTER.ETCBTMBMS01", Building207, "General / Other", "kWh", "DB");
            AddActual("LOT209_LT1.PL_MC1", Building209, "1st floor", "kWh", "PL");
            AddActual("LOT209_LT1.PL_MC2", Building209, "1st floor", "kWh", "PL");
            AddActual("LOT209_LT1.PL_MC2_A", Building209, "1st floor", "kWh", "PL");
            AddActual("LOT209_LT1.PL_MDB_320A", Building209, "1st floor", "kWh", "MDB");
            AddActual("LOT209_LT1.PMLT11", Building209, "1st floor", "kWh", "DB");
            AddActual("LOT209_LT1.PMLT12", Building209, "1st floor", "kWh", "DB");
            AddActual("LOT209_LT2.AG2_RYB19", Building209, "2nd floor", "kWh", "DB");
            AddActual("LOT209_LT2.PL_AGING1", Building209, "2nd floor", "kWh", "PL");
            AddActual("LOT209_LT2.PL_AGING1_A", Building209, "2nd floor", "kWh", "PL");
            AddActual("LOT209_LT2.PL_AGING2", Building209, "2nd floor", "kWh", "PL");
            AddActual("LOT209_LT2.PL_DB1_320A", Building209, "2nd floor", "kWh", "PL");
            AddActual("LOT209_LT2.PL_DB2_320A", Building209, "2nd floor", "kWh", "PL");
            AddActual("LOT209_LT2.PL_DB2_500A_A", Building209, "2nd floor", "kWh", "PL");
            AddActual("LOT209_LT2.PL_DB3_320A", Building209, "2nd floor", "kWh", "PL");
            AddActual("LOT209_LT2.PL_DB3_500A", Building209, "2nd floor", "kWh", "PL");
            AddActual("LOT209_LT2.PL_DB3_500A_A", Building209, "2nd floor", "kWh", "PL");
            AddActual("LOT209_LT2.PL_DB1_250A", Building209, "2nd floor", "kWh", "PL");
            AddActual("LOT209_LT2.PL_DB1_250A_A", Building209, "2nd floor", "kWh", "PL");
            AddActual("LOT209_LT2.PL_DB2_500A", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.1_B3", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.1_B5", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.1_B6", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.1_R3", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.1_R5", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.1_Y3", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.1_Y4", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.1_Y6", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.2_B1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.2_B3", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.2_B5", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.2_R1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.2_R2", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.2_R5", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.2_RYB1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.2_Y1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.2_Y3", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_2Z4.2_Y5", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_AC_TESTING.1_RYB1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_AC_TESTING.1_RYB3", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_AC_TESTING.2_RYB1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_AC_TESTING.2_RYB2", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_AC_TESTING.2_RYB3", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_AC_TESTING.2_RYB4", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_AC_TESTING.2_RYB5", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_AC_TESTING.EP_CHARGER", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_AC_TESTING.TESTING_1_RYB1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBREFLOW.1_RYB1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBREFLOW.3_B2", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBREFLOW.3_B3", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBREFLOW.3_Y1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBREFLOW.4_B1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBREFLOW.4_R2", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBREFLOW.4_Y1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.1_RYB1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.1_RYB2", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.1_RYB3", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.1_RYB4", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.1_RYB5", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.1_RYB6", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.2_RYB1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.2_RYB2", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.2_RYB3", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.2_RYB4", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.2_RYB5", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.3_RYB1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.3_RYB4", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.3_RYB5", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.3_RYB6", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.4_RYB1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.3_RYB2", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_DBSAWING.3_RYB3", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA3.B1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA3.B7", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA3.B8", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA3.R1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA3.RYB14", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA3.RYB15", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA3.RYB17", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA3.RYB18", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA3.RYB22", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA3.RYB23", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA3.RYB24", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA3.RYB25", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA3.Y8", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA3.Y9", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA4.RYB1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA4.RYB11", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA4.RYB12", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA4.RYB14", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA4.RYB17", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA4.RYB2", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA4.RYB7", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_HYDRA4.RYB8", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.1_B2", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.1_R2", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.1_R3", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.1_R4", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.1_R5", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.1_RYB1", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.1_Y2", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.1_Y3", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.1_Y4", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.2_B1", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.2_B2", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.2_B3", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.2_R1", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.2_R2", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.2_Y1", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.2_Y2", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.2_Y3", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.3_B1", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.3_B5", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.3_R1", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.3_R3", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.3_B4", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.4_B3", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.4_B4", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.4_B5", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.4_B6", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.4_R1", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.4_R4", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.4_Y1", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.4_Y3", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.4_Y4", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT1_LIGHTINGPOINT.4_Y5", Building207, "1st floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.1_B1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.1_B2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.1_B7", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.1_B4", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.1_R1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.1_R2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.1_R4", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.1_R5", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.1_Y1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.1_Y2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.1_Y3", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.1_Y7", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.2_B2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.2_R1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.2_B3", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.2_B1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.2_Y1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.2_Y2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.3_MCB1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.3_MCB3", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.3_RYB1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.4_RYB2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.4_RYB3", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.5_B1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.5_B2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.5_R1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.5_R2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBPYRO.5_RYB5", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_DBSAWING.1_RYB1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1011.1_1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1011.2_1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1011.3_1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1011.4_B1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1011.4_R1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1011.4_Y1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1011.5_B1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1011.5_R1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1011.5_Y1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1314.1_1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1314.2_1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1314.3_R", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1314.4_B1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1314.4_R1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1314.4_Y1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1314.5_B2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1314.5_R2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1314.5_Y2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1517.1_1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1517.2_1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1517.3_1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1517.4_B2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1517.4_R2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1517.4_Y2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1517.5_B1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1517.5_R3", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1517.5_Y3", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1820.1_B1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1820.1_R1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1820.1_Y1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1820.2_B2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1820.2_R2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1820.2_Y2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1820.3_B3", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1820.3_R3", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_POL_1820.3_Y3", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_VACUM.VACUM_PUMP1", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_VACUM.VACUM_PUMP2", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_VACUM.VACUM_PUMP3", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT2_VACUM.VACUM_PUMP4", Building207, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA1.B1_OFFICE", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA1.R7_FLASH", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA1.RYB18", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA1.RYB20", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA1.RYB25", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA1.RYB26_SLUGH", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA1.RYB31", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA1.Y6_NIPLATING", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA2.1_B1", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA2.1_Y4", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA2.CITYZEN", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA2.RYB12_SPARK", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA2.RYB13_SPARK", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA2.RYB14_SPARK", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA2.RYB15_SPARK", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA2.RYB18_SPARK", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA2.RYB9_SPARK", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA2.TESTING_ROOM", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA2.Y1", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA3.B7", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA3.B8", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA3.RYB27", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA3.Y8", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_LT3_HYDRA3.Y9", Building207, "3rd floor", "kWh", "PL");
            AddActual("PL_LOT207_SMD.1_B4", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_SMD.3_B5", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_SMD.2_B2", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_SMD.2_B4", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_SMD.2_B5", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_SMD.2_R1", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_SMD.2_R3", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_SMD.2_R4", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_SMD.2_R5", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_SMD.3_B3", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_SMD.2_Y2", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_SMD.2_Y3", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_SMD.3_B4", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT207_SMD.2_Y5", Building207, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_DB_MC2.PL_LOT209_MC2_1RYB3_NAKAMURATOMELC50", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_DB_MC2.PL_LOT209_MC2_2RYB3_NAKAMURATOMELC50", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_DB_MC2.PL_LOT209_MC2_2RYB4_NAKAMURATOMELC50", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_DBCAMCO.BTU_BELT_FURNACE1", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_DBCAMCO.BTU_BELT_FURNACE2", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_DBCAMCO.CAMCO4", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.SAND_BLASTING", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.KEYENCE", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.RADIUS_POLLISING_A", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.RADIUS_POLLISING_B", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.GSP60_01", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.DOUBLE_SIDE_LAPPING", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.NAKAMURA_TOME_LC50", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.TRAINING_CENTRE_A", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.TRAINING_CENTRE_B", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.NAKAMURA_TOME_GSP60", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.PRESSING_MACHINE", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.HUMIDIFIER_PACKINGROOM", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.HUMIDITY_CEILINGROOM", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.NAKAMURA_LC50", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.NAKAMURA_TOME_LC5011AB", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.NAKAMURA_TOME_LC508AB", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.NAKAMURA_TOME_LC509AB", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.NAKAMURATOME_LC50", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC1.WASHING", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.RADIUS_POLISHING_MAIJUN", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.DOUBLESIDE_LAPPING", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.RADIUS_POLISHING", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.LC50+RP01", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.PACKING", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.NAKAMURATOME_LC50", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.PINAIS_INSPECTION", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.TRAINING", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.CONFOCAL", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.RADIUS_POLLISHING", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.UVSTERILIZER", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.CENTERING_EDGING", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.NAKAMURA_LC50", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.ESHAWER_DOOR", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.TRIOPTICS", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.RADIUS_POLISHING_5RYB3", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.4_B4", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.4_B5", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.PACKING_4Y5", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.5_Y2", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.TRAINING_5RYB6", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.4_Y5", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.5_B1", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.5_RYB3", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.5_RYB4", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_DB_MC2.5_Y1", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_LT1_MDB_320A.1QF2_PRODUCTION", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.1QF3_PRODUCTION", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.2QF1_PRODUCTION", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.2QF2_PRODUCTION", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.2QF3_PRODUCTION", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.2QF4", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.2QF5_MEETING", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.2QF6_FQA", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.3QF1_PRODUCTION", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.3QF2_FQA", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.3QF3_OFFICE", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.3QF4_MEETING", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.3QF5_SHOWROOM", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.3QF6_GUEST", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.3QF7_MANAGER", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.ACPROD09", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB_320A.ACSPLIT2HP_MANAGER", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB320A.PL_LOT209_MDB320A_1RYB1_ACTRAINING", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB320A.PL_LOT209_MDB320A_1RYB2_AC38", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB320A.PL_LOT209_MDB320A_1RYB3_AC39", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB320A.PL_LOT209_MDB320A_1RYB3_AC41", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB320A.PL_LOT209_MDB320A_1RYB4_AC40", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB320A.PL_LOT209_MDB320A_2RYB2_ACCLEANROOM", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB320A.PL_LOT209_MDB320A_2RYB4_ACPREPINSPECTION2", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT1_MDB320A.PL_LOT209_MDB320A_2RYB5_ACSANDBLAST", Building209, "1st floor", "kWh", "MDB");
            AddActual("PL_LOT209_LT2.AGING2_RYB1", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB10", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB11", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB12", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB13", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB14", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB15", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB16", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB17", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB18", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB19", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB2", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB20", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB3", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB4", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB5", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB6", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB7", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB8", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.AGING2_RYB9", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_RYB1_TOREX", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_B1_FILL", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_B12_TIG", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_B13_NIPLATING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_B14A_BRAZING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_B2_SANDBLASTING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_B3_SERIALIZING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_B4_METALIZED", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1-250A_R1_FILL", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_R11_TIG", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_R12_BRAZING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_R14_DARKROOM", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_R2_COAT", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_R3_TIG", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_R4_METALIZED", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_RYB2_SANDBLASTING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_RYB4_NIPLATING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_RYB5_COATING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_Y12_SANDBLASTING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_Y14_TIG", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_Y2_SANDBLASTING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_Y3_TIG", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_Y4_METALIZED", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2.DB1_Y5_FILL", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.B1_VQ_RACK_D", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.B2_C14_RACK_F", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.B3_C14_RACK_K", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.B4_S_RACKM", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.B5_G3_RACK_5", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.B6_G3_RACK_6", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.B7_G3_RACK_3", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.R1_C1_RACK_G", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.R2_VQ_RACK_B", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.R3_C14_RACK_G", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.R4_S_RACK_AA", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.R5_S_RACK_N", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.RYB21_S_RACK_I", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.RYB22_S_RACK_J", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.RYB23_S_RACK_L", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.RYB24_S_RACK_O", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.RYB25_S_RACK_P", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.RYB26_S_RACK_Q", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.RYB27_S_RACK_T", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.Y1_VQ_RACK_A", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.Y2_VQ_RACK_C", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.Y3_C14_RACK_J", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.Y4_S_RACK_K", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.Y5_S_RACK_A1", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_AGING1.Y6_G3_RACK_4", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB1_320A.1QF3_AC35", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB1_320A.1QF4_AC35", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB1_320A.2QF2_AC1", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB1_320A.2QF3_AC3", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB1_320A.2QF7_AC2", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB1_320A.3QF1", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB1_320A.3QF3_AC1", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB1_320A.3QF4_AC1", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB1_320A.3QF5_AC1", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB1_320A.3QF6_AC2", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB1_320A.AC_BRAZZING1", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB1_320A.AC_BRAZZING2", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB1_320A.AC_BRAZZING3", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB1_320A.AC_BRAZZING4", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.1QF1_NIPLATING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.1QF2_MAIN_OFFICE", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.1QF3_OFFICE", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.1QF4_MAINTENANCE", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.2QF1_NIPLATING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.2QF2_ASSEMBLY", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.2QF3_TIG", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.2QF4_SYSTEM_AGING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.2QF5_SYSTEM_AGING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.3QF1_GUEST", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.3QF10_NURSE", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.3QF2_COAT", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.3QF3_CANTIN", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.3QF4_TRAINING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.3QF5_TRAINING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.3QF6_CANTIN", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.3QF7_CANTIN", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.3QF8_SUB_STORE", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_320A.3QF9_CLINIC", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.B2_COAT", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.B3_TIG", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.B6_COAT", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.B8_FILL", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.B9_FILL", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.R10_FILL", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.R3_COAT", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.R5_COAT", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.R9_FILL", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.RYB1_TIG", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.RYB2_BRAZ", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.RYB3_BRAZING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.RYB4_BRAZING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.RYB5_SER", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.RYB6_SER", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.Y10_FILL", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.Y2_COAT", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.Y3_TIG", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.Y5_FILL", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.Y8_FILL", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.Y9_FILL", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.1QF1_AC_ASSEMBLY", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.1QF2_FILL_STORE", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.1QF3_SEREALIZING_PACKING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.2QF1_AC_ASSEMBLY", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.2QF2_FILL_STORE", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.2QF3_SEREALIZING_PACKING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.2QF4", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.2QF5", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.3QF1_AC_PUMPING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.3QF2_AC_PRODUCTION", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.3QF3_AC_SANDBLAST", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.AC_FEMALEPRAYING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.AC_HUMIDITYROOM_A", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.AC_HUMIDITYROOM_B", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.AC_MAINSTORE", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.ACSPLIT_1HPA", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_320A.ACSPLIT_1HPB", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.B17_NIPLATING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.B18_NIPLATING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.B2_BRAZING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.B4_PACKING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.B5_SYSTEM", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.R1_NIPLATING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.R14_NIPLATING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.R2_BRAZING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.R3_BRAZING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.R4_IQA", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.RYB2_CANTEEN", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.RYB3_FACILITY", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.RYB4_NIPLATING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.RYB5_NIPLATING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.RYB6_NIPLATING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.RYB7_NIPLATING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.SERVER", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.Y1_NIPLATING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.Y14", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.Y2_BRAZING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB3_500A.Y5_SERIALIZING", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_MC3.PL_LOT209_MC3_3RYB6_RADLAPPING", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC3.PL_LOT209_MC3_4RYB3_NTGSP60", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_1RYB1_NAKAMURAGS60", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_1RYB2_NAKAMURA", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_1RYB3_NAKAMURA", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_1RYB4_RADPOLISHING", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_1RYB5_RADPOLISHING", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_1RYB6_RADPOLISHING", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_1RYB7_POMPAAIR", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_1RYB8_NAKAMURA", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_2RYB1_NAKAMURAGSP60", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_2RYB1_NTGSP60", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_2RYB3_NAKAMURA", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_2RYB4_RADPOLISHING", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_2RYB5_RADPOLISHING", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_2RYB6_RADPOLISHING", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_2RYB7_RADPOLISHING", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_2RYB8_RADPOLISHING", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_3RYB1_NAKAMURA", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_3RYB3_NAKAMURA", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_3RYB6_RADPOLISHING", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_3RYB8_RADPOLISHING", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_4RYB3_NTLC50", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_4RYB4_RADPOLISHING", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_5RYB1_RADPOLISHING", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_5RYB3_NAKAMURA", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_5RYB4_NAKAMURA", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MC4.PL_LOT209_MC4_5RYB5_NAKAMURA", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_MDB320A.PL_LOT209_MDB320A_1RYB1ACCOAT1", Building209, "General / Other", "kWh", "MDB");
            AddActual("PL_LOT209_MDB320A.PL_LOT209_MDB320A_1RYB4_ACCOAT2", Building209, "General / Other", "kWh", "MDB");
            AddActual("PL_LOT209_POWER_L1.B1_Y1", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_POWER_L1.B2_B1", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_POWER_L1.B2_R1", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_POWER_L1.B2_Y1", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_POWER_L1.B3_B1", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_POWER_L1.B3_R1", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_POWER_L1.B3_Y2", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_POWER_L1.B5_R1", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_POWER100A.PL_LOT209_POWER100A_2B5_DDCLT1", Building209, "1st floor", "kWh", "PL");
            AddActual("PL_LOT209_POWER100A.PL_LOT209_POWER100A_2RYB1_HMDTYCOAT", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_POWER100A.PL_LOT209_POWER100A_2RYB2_EXHAUSTTOILET", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_POWER100A.PL_LOT209_POWER100A_3RYB1_EXHAUSTFAN", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_POWER100A.PL_LOT209_POWER100A_3RYB8_CHILLER", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_POWER100A.PL_LOT209_POWER100A_4RYB10_HUMIDITY", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT209_POWER100A.PL_LOT209_POWER100A_1RYB1_NEWOFFICE", Building209, "General / Other", "kWh", "PL");
            AddActual("PL_LOT238_DBFURNACE.RYB1", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_DBFURNACE.RYB10", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_DBFURNACE.RYB11", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_DBFURNACE.RYB2", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_DBFURNACE.RYB3", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_DBFURNACE.RYB4", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_DBFURNACE.RYB5", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_DBFURNACE.RYB6", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_DBFURNACE.RYB7", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_DBFURNACE.RYB8", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_DBFURNACE.RYB9", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_DBSPUTTERING.1_RYB1", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_DBSPUTTERING.2_RYB1", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_LT1_DBMC1.1_RYB2", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_LT1_DBMC1.1_RYB5", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_LT1_DBMC2.1_Y1", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_LT1_DBMC2.1_Y2", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_LT1_DBMC2.2_B14", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_LT1_DBMC2.2_B19", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_LT1_DBMC2.2_R20", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_LT1_DBMC2.2_R22", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_LT1_DBMC2.2_R23", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_LT1_DBMC2.2_Y15", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_LT1_DBMC2.2_Y16", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_LT1_DBMC2.2_Y18", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PL_LOT238_LT1_DBMC2.2_Y20", Building238, "Overall / Main", "kWh", "PL");
            AddActual("PLC_LOT238.TOTALIZER_AIRFLOW", Building238, "Overall / Main", "kWh", "PLC");
            AddActual("PLC_LOT238.WAGES238", Building238, "Overall / Main", "kWh", "PLC");
            AddActual("PM_LOT207_LT1.207_SSB1T", Building207, "1st floor", "kWh", "SSB");
            AddActual("PM_LOT207_LT1.DB_CHILLER_1-4", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_CHILLER_2-3", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_CPCOMAIR3", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_CPCOMAIR8", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_CPCOMAIR2", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_CPCOMAIR4", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_CPCOMAIR5", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_Reflow3", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_VACUUMP", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_Reflow2", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_Reflow1", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_GL", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.WASHING", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.DB_SOLARPANEL_LT1", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_SOLARPANEL_LT11", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.WETDUST", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.PM_WETDUST-thd_VoltageL-L", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT207_LT1.PM_WETDUST-thd_VoltageL-N", Building207, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PMDB_AC320A", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT207_LT1.DB_HITACHI", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.PMDB_LIGHTING_2", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_POWER_EXHAUST", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT207_LT1.SSB1T", Building207, "1st floor", "kWh", "SSB");
            AddActual("PM_LOT207_LT1.PMSSB1T-thd_VoltageL-L", Building207, "1st floor", "V", "SSB");
            AddActual("PM_LOT207_LT1.PMSSB1T-thd_VoltageL-N", Building207, "1st floor", "V", "SSB");
            AddActual("PM_LOT207_LT2.DB_AC_OFFICE_DETECTION", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.PM_AC_NEW_OFFICE-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.PM_AC_NEW_OFFICE-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_AC_IRD", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_P1", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_P2", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_4", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_MIELE", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.DB_POL1314", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT2.PM_POL1314-thd_VoltageL-L", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.PM_POL1314-thd_VoltageL-N", Building207, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.SSB2TB", Building207, "2nd floor", "kWh", "SSB");
            AddActual("PM_LOT207_LT2.PMSSB2TB-thd_VoltageL-L", Building207, "2nd floor", "V", "SSB");
            AddActual("PM_LOT207_LT2.PMSSB2TB-thd_VoltageL-N", Building207, "2nd floor", "V", "SSB");
            AddActual("ELM_LOT207_LT2.POL1314", Building207, "2nd floor", "kWh", "DB");
            AddActual("PM_LOT207_LT2.SSB2TA", Building207, "2nd floor", "kWh", "SSB");
            AddActual("PM_LOT207_LT2.SSB2TA-thd_VoltageL-L", Building207, "2nd floor", "V", "SSB");
            AddActual("PM_LOT207_LT2.SSB2TA-thd_VoltageL-N", Building207, "2nd floor", "V", "SSB");
            AddActual("PM_LOT207_LT3.DB_AC_FTC", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_AC3_3RDFLOOR", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_3Z1", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_LIGHTING_HYDRA", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.HYDRA1", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.HYDRA1-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.HYDRA1-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.HYDRA2", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.HYDRA2-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.HYDRA2-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.HYDRA3", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.HYDRA3-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.HYDRA3-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.HYDRA4", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.HYDRA4-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.HYDRA4-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.DB_AC_HYDRA", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_A", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_PHASE1", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_PHASE2", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_CPCOMPAIR7", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.PM_RDRY_TRIAL", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DB_CPCOMPAIR6", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.DBUV", Building207, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT207_LT1.SOLAR_PANEL", Building207, "1st floor", "kWh", "PM");
            AddActual("PM_LOT207_LT3.PMSOLAR_PANEL-thd_VoltageL-L", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.PMSOLAR_PANEL-thd_VoltageL-N", Building207, "3rd floor", "V", "PM");
            AddActual("PM_LOT207_LT3.SSB3TA", Building207, "3rd floor", "kWh", "SSB");
            AddActual("PM_LOT207_LT3.PMSSB3TA-thd_VoltageL-L", Building207, "3rd floor", "V", "SSB");
            AddActual("PM_LOT207_LT3.PMSSB3TA-thd_VoltageL-N", Building207, "3rd floor", "V", "SSB");
            AddActual("PM_LOT207_LT3.SSB3TB", Building207, "3rd floor", "kWh", "SSB");
            AddActual("PM_LOT207_LT3.PMSSB3TB-thd_VoltageL-L", Building207, "3rd floor", "V", "SSB");
            AddActual("PM_LOT207_LT3.PMSSB3TB-thd_VoltageL-N", Building207, "3rd floor", "V", "SSB");
            AddActual("PM_LOT207_ROOFTOP.CHILLER6", Building207, "Rooftop", "kWh", "PM");
            AddActual("PM_LOT207_ROOFTOP.CHILLER6-thd_VoltageL-L", Building207, "Rooftop", "V", "PM");
            AddActual("PM_LOT207_ROOFTOP.CHILLER6-thd_VoltageL-N", Building207, "Rooftop", "V", "PM");
            AddActual("PM_LOT207_ROOFTOP.HYPERCHILL", Building207, "Rooftop", "kWh", "PM");
            AddActual("PM_LOT207_ROOFTOP.HYPERCHILL-thd_VoltageL-L", Building207, "Rooftop", "V", "PM");
            AddActual("PM_LOT207_ROOFTOP.HYPERCHILL-thd_VoltageL-N", Building207, "Rooftop", "V", "PM");
            AddActual("PM_LOT209_LT1.PMDB_COMPRESSOR", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PMDB_320A", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PM_AC_CL2", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.PM_CHILLER", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.PM_CHILLER-thd_VoltageL-L", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PM_CHILLER-thd_VoltageL-N", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PMDB_MC4", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PM_MDB400A", Building209, "1st floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT2.PM_MDB400A_CHILLER", Building209, "2nd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PM_ULTRASONIC1", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.PM_ULTRASONIC1-thd_VoltageL-L", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PM_ULTRASONIC1-thd_VoltageL-N", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PM_ULTRASONIC2", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.PM_ULTRASONIC2-thd_VoltageL-L", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PM_ULTRASONIC2-thd_VoltageL-N", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PM_ULTRASONIC3", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.PM_ULTRASONIC3-thd_VoltageL-L", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PM_ULTRASONIC3-thd_VoltageL-N", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PM_ULTRASONIC4", Building209, "1st floor", "kWh", "PM");
            AddActual("PM_LOT209_LT1.PM_ULTRASONIC4-thd_VoltageL-L", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT1.PM_ULTRASONIC4-thd_VoltageL-N", Building209, "1st floor", "V", "PM");
            AddActual("PM_LOT209_LT2.PM_WWTP", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PMDB_COMPRESSOR500A", Building209, "2nd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT1.PMSSB1TB", Building209, "1st floor", "kWh", "SSB");
            AddActual("PM_LOT209_LT1.PMSSB1TA", Building209, "1st floor", "kWh", "SSB");
            AddActual("ELM_LOT209_LT2.RGU_DB_COMPRESSOR_500A", Building209, "2nd floor", "kWh", "DB");
            AddActual("PM_LOT209_LT2.PM_ASTROFURNACE2", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PM_ASTROFURNACE2-thd_VoltageL-L", Building209, "2nd floor", "V", "PM");
            AddActual("PM_LOT209_LT2.PM_ASTROFURNACE2-thd_VoltageL-N", Building209, "2nd floor", "V", "PM");
            AddActual("PM_LOT209_LT2.PM_ASTROFURNACE4", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PM_ASTROFURNACE4-thd_VoltageL-L", Building209, "2nd floor", "V", "PM");
            AddActual("PM_LOT209_LT2.PM_ASTROFURNACE4-thd_VoltageL-N", Building209, "2nd floor", "V", "PM");
            AddActual("PM_LOT209_LT2.PM_CAMCOFURNACE7", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PM_CAMCOFURNACE7-thd_VoltageL-L", Building209, "2nd floor", "V", "PM");
            AddActual("PM_LOT209_LT2.PM_CAMCOFURNACE7-thd_VoltageL-N", Building209, "2nd floor", "V", "PM");
            AddActual("PM_LOT209_LT2.PM_CAMCOFURNACE8", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PM_CAMCOFURNACE8-thd_VoltageL-L", Building209, "2nd floor", "V", "PM");
            AddActual("PM_LOT209_LT2.PM_CAMCOFURNACE8-thd_VoltageL-N", Building209, "2nd floor", "V", "PM");
            AddActual("PM_LOT209_LT2.PM_CHILLER13", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PM_CHILLER13-thd_VoltageL-L", Building209, "2nd floor", "V", "PM");
            AddActual("PM_LOT209_LT2.PM_CHILLER13-thd_VoltageL-N", Building209, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_OFFICE_DETECTION", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PMDB_ASTRO", Building209, "2nd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT2.PM_LOT209_EXHAUST2_LT2", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PM_NIPLATING", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PM_NIPLATING-thd_VoltageL-L", Building209, "2nd floor", "V", "PM");
            AddActual("PM_LOT209_LT2.PM_NIPLATING-thd_VoltageL-N", Building209, "2nd floor", "V", "PM");
            AddActual("PM_LOT207_LT2.DB_L3", Building207, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PMPANEL_EXHAUST", Building209, "2nd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT2.PMPANEL_EXHAUST-thd_VoltageL-L", Building209, "2nd floor", "V", "PM");
            AddActual("PM_LOT209_LT2.PMPANEL_EXHAUST-thd_VoltageL-N", Building209, "2nd floor", "V", "PM");
            AddActual("PM_LOT209_LT2.PMSSB2TB", Building209, "2nd floor", "kWh", "SSB");
            AddActual("PM_LOT209_LT2.PMSSB2TA", Building209, "2nd floor", "kWh", "SSB");
            AddActual("PL_LOT209_LT2_DB2_500A.B1_TIG", Building209, "2nd floor", "kWh", "PL");
            AddActual("PL_LOT209_LT2_DB2_500A.R2_TIG", Building209, "2nd floor", "kWh", "PL");
            AddActual("PM_LOT209_LT3.L_P", Building209, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT3.L_P-thd_VoltageL-L", Building209, "3rd floor", "V", "PM");
            AddActual("PM_LOT209_LT3.L_P-thd_VoltageL-N", Building209, "3rd floor", "V", "PM");
            AddActual("PM_LOT209_LT3.MC_TC", Building209, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT3.MC_TC-thd_VoltageL-L", Building209, "3rd floor", "V", "PM");
            AddActual("PM_LOT209_LT3.MC_TC-thd_VoltageL-N", Building209, "3rd floor", "V", "PM");
            AddActual("PM_LOT209_LT3.MDB_200A_AC_TC", Building209, "3rd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT3.P", Building209, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT3.PM_CHILLER_ROOFTOP", Building209, "3rd floor", "kWh", "PM");
            AddActual("PM_LOT209_LT3.PMDB_EXHAUST_LT3", Building209, "3rd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT3.PMMDB_400A_LT3", Building209, "3rd floor", "kWh", "MDB");
            AddActual("PM_LOT209_LT3.PMSSB3TB", Building209, "3rd floor", "kWh", "SSB");
            AddActual("PM_LOT209_LT3.P-thd_VoltageL-L", Building209, "3rd floor", "V", "PM");
            AddActual("PM_LOT209_LT3.P-thd_VoltageL-N", Building209, "3rd floor", "V", "PM");
            AddActual("PM_LOT238.COMPRESSOR", Building238, "Overall / Main", "kWh", "PM");
            AddActual("PM_LOT238.COMPRESSOR-thd_VoltageL-L", Building238, "Overall / Main", "V", "PM");
            AddActual("PM_LOT238.COMPRESSOR-thd_VoltageL-N", Building238, "Overall / Main", "V", "PM");
            AddActual("PM_LOT238.MAIN", Building238, "Overall / Main", "kWh", "PM");
            AddActual("PM_LOT238.PM_AIRCOMPRS", Building238, "Overall / Main", "kWh", "PM");
            AddActual("PM_LOT238.PM_AIRCOMPRS-thd_VoltageL-L", Building238, "Overall / Main", "V", "PM");
            AddActual("PM_LOT238.PM_AIRCOMPRS-thd_VoltageL-N", Building238, "Overall / Main", "V", "PM");
            AddActual("PM_LOT238.PM_MAIN", Building238, "Overall / Main", "kWh", "PM");
            AddActual("PM_LOT238.PM_MAIN-thd_VoltageL-L", Building238, "Overall / Main", "V", "PM");
            AddActual("PM_LOT238.PM_MAIN-thd_VoltageL-N", Building238, "Overall / Main", "V", "PM");
            AddActual("PM_LOT238.trial", Building238, "Overall / Main", "kWh", "PM");
            AddActual("QUERYSERVER.ETCBTMBMS01", Building207, "General / Other", "kWh", "DB");
            AddActual("THDEVICE_LOT238.THDEVICE1", Building238, "Overall / Main", "kWh", "DB");
            AddActual("THDEVICE_LOT238.THDEVICE2", Building238, "Overall / Main", "kWh", "DB");
            AddActual("THDEVICE_LOT238.THDEVICE3", Building238, "Overall / Main", "kWh", "DB");
            AddActual("VIP.DDD", Building207, "General / Other", "kWh", "DB");
            AddActual("VIP.DEFAULT", Building207, "General / Other", "kWh", "DB");
            AddActual("VIP.DEFAULT-ART3Result4", Building207, "General / Other", "kWh", "DB");
            AddActual("VIP.PQADVISOR", Building207, "General / Other", "kWh", "DB");
            AddActual("Wages207.207_WAGES_LT2_FR2", Building207, "2nd floor", "kWh", "DB");
            AddActual("Wages207.AIRFLOW_LT3_MAIN", Building207, "3rd floor", "kWh", "DB");
            AddActual("Wages207.AIRFLOW_LT3_SL", Building207, "3rd floor", "kWh", "DB");
            AddActual("Wages209.CDA209_LT2", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages207.flow_lt1", Building207, "1st floor", "kWh", "DB");
            AddActual("Wages207.FM_OWS", Building207, "General / Other", "kWh", "DB");
            AddActual("Wages207.FR_OWS", Building207, "General / Other", "kWh", "DB");
            AddActual("Wages207.Wages_main", Building207, "General / Other", "kWh", "DB");
            AddActual("Wages207.N2_AUTOVALVE", Building207, "General / Other", "kWh", "DB");
            AddActual("Wages207.N2_AUTOVALVE_7", Building207, "General / Other", "kWh", "DB");
            AddActual("Wages207.N2_AUTOVALVE_34", Building207, "General / Other", "kWh", "DB");
            AddActual("Wages207.N2_AUTOVALVE_56", Building207, "General / Other", "kWh", "DB");
            AddActual("Wages207.OWS", Building207, "General / Other", "kWh", "DB");
            AddActual("Wages207.ROOFTOP1", Building207, "Rooftop", "kWh", "DB");
            AddActual("Wages207.Wages_LT2_FR2", Building207, "2nd floor", "kWh", "DB");
            AddActual("Wages207.Wages_LT1_FR1", Building207, "1st floor", "kWh", "DB");
            AddActual("Wages207.207_Wages_LT3_FR3", Building207, "3rd floor", "kWh", "DB");
            AddActual("Wages207.Wages_Totalizer", Building207, "General / Other", "kWh", "DB");
            AddActual("Wages209.CDA_LT3", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.CDA_LT3_TC", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.FLOWMETER_LT1", Building209, "1st floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209_Lt1", Building209, "1st floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209_Lt1-Register1", Building209, "1st floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209_Lt1-Register1~1", Building209, "1st floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209_Lt1-Register2", Building209, "1st floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209_Lt1-Register2~1", Building209, "1st floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209_Lt1-Register3", Building209, "1st floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209_Lt1-Register3~1", Building209, "1st floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209_Lt1-Register4", Building209, "1st floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209_Lt1-Register4~1", Building209, "1st floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209_Lt1-Register5", Building209, "1st floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209_Lt1-Register5~1", Building209, "1st floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209_Lt2", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt2-Register1", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt2-Register1~1", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt2-Register2", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt2-Register2~1", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt2-Register3", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt2-Register3~1", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt2-Register4", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt2-Register4~1", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt2-Register5", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt2-Register5~1", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209_Lt3", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt3-Register1", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt3-Register1~1", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt3-Register2", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt3-Register2~1", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt3-Register3", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt3-Register3~1", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt3-Register4", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt3-Register4~1", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt3-Register5", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.FlowMeterAir_209Lt3-Register5~1", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.FM_LT1", Building209, "1st floor", "kWh", "DB");
            AddActual("Wages209.GEN_N2_LT2", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.GEN_N2_LT3", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.Generator_N2_lt2", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.Generator_N2_lt3", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.SOXAL_LT2", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.SOXAL_LT3", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.Soxal_N2_lt2", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.SOXAL_N2_LT3", Building209, "3rd floor", "kWh", "DB");
            AddActual("Wages209.trial", Building209, "General / Other", "kWh", "DB");
            AddActual("Wages209.trialvolume", Building209, "General / Other", "kWh", "DB");
            AddActual("Wages209.VALVE", Building209, "General / Other", "kWh", "DB");
            AddActual("Wages209.VOLUME_LT2", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.WAGES209_LT2", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.WAGES209_LT2-Register1", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.WAGES209_LT2-Register1~1", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.WAGES209_LT2-Register2", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.WAGES209_LT2-Register2~1", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.WAGES209_LT2-Register3", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.WAGES209_LT2-Register3~1", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.WAGES209_LT2-Register4", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.WAGES209_LT2-Register4~1", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.WAGES209_LT2-Register5", Building209, "2nd floor", "kWh", "DB");
            AddActual("Wages209.WAGES209_LT2-Register5~1", Building209, "2nd floor", "kWh", "DB");
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


    // Explicit dashboard card configuration using exact database keys.
    public sealed class ElectricityCardItem
    {
        public string DisplayName { get; set; } = string.Empty;
        public string MeterName { get; set; } = string.Empty;
    }

    public sealed class ElectricityCardDefinition
    {
        public string Title { get; set; } = string.Empty;
        public string Building { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;
        public List<ElectricityCardItem> Items { get; set; } = new();
    }

    public static class ElectricityCardCatalog
    {
        private static readonly List<ElectricityCardDefinition> _cards = new()
        {
            new ElectricityCardDefinition { Title = "Overall Building 207", Building = "Building 207", Floor = "Overall", Items = new List<ElectricityCardItem>
            {
                new ElectricityCardItem { DisplayName = "PM DB SSB1T", MeterName = "PM_LOT207_LT1.SSB1T" },
                new ElectricityCardItem { DisplayName = "PM DB SSB2TA", MeterName = "PM_LOT207_LT2.SSB2TA" },
                new ElectricityCardItem { DisplayName = "PM DB SSB2TB", MeterName = "PM_LOT207_LT2.SSB2TB" },
                new ElectricityCardItem { DisplayName = "PM DB SSB3TA", MeterName = "PM_LOT207_LT3.SSB3TA" },
                new ElectricityCardItem { DisplayName = "PM DB SSB3TB", MeterName = "PM_LOT207_LT3.SSB3TB" },
            }},
            new ElectricityCardDefinition { Title = "Overall Building 209", Building = "Building 209", Floor = "Overall", Items = new List<ElectricityCardItem>
            {
                new ElectricityCardItem { DisplayName = "PM SSB1TA", MeterName = "PM_LOT209_LT1.PMSSB1TA" },
                new ElectricityCardItem { DisplayName = "PM SSB1TB", MeterName = "PM_LOT209_LT1.PMSSB1TB" },
                new ElectricityCardItem { DisplayName = "PM SSB2TA", MeterName = "PM_LOT209_LT2.PMSSB2TA" },
                new ElectricityCardItem { DisplayName = "PM SSB2TB", MeterName = "PM_LOT209_LT2.PMSSB2TB" },
                new ElectricityCardItem { DisplayName = "PM SSB3TB", MeterName = "PM_LOT209_LT3.PMSSB3TB" },
            }},
            new ElectricityCardDefinition { Title = "Building 207 - Detection 1st floor", Building = "Building 207", Floor = "1st floor", Items = new List<ElectricityCardItem>
            {
                new ElectricityCardItem { DisplayName = "DB Testing 1st Floor", MeterName = "PM_LOT207_LT1.DB_TESTING" },
                new ElectricityCardItem { DisplayName = "DB AC Testing", MeterName = "PM_LOT207_LT1.DB_AC_TESTING" },
                new ElectricityCardItem { DisplayName = "DB SMT ( GL )", MeterName = "PM_LOT207_LT1.DB_GL" },
                new ElectricityCardItem { DisplayName = "Reflow #1", MeterName = "PM_LOT207_LT1.DB_Reflow1" },
                new ElectricityCardItem { DisplayName = "Reflow #2", MeterName = "PM_LOT207_LT1.DB_Reflow2" },
                new ElectricityCardItem { DisplayName = "DB Reflow #3", MeterName = "PM_LOT207_LT1.DB_Reflow3" },
                new ElectricityCardItem { DisplayName = "Washing", MeterName = "PM_LOT207_LT1.WASHING" },
                new ElectricityCardItem { DisplayName = "AC SMT (GL)", MeterName = "PM_LOT207_LT1.DB_AC_GL" },
                new ElectricityCardItem { DisplayName = "Compressor SMT", MeterName = "PM_LOT207_LT1.PM_DB_COMPRESSOR_LT1" },
                new ElectricityCardItem { DisplayName = "DETECTION 1ST FLOOR Real Energy Into the Load", MeterName = "" },
                new ElectricityCardItem { DisplayName = "WAREHOUSE", MeterName = "PM_LOT207_LT1.DB_WAREHOUSE" },
                new ElectricityCardItem { DisplayName = "AC_C10K", MeterName = "PM_LOT207_LT1.DB_AC_C10K" },
                new ElectricityCardItem { DisplayName = "C10K_1", MeterName = "PM_LOT207_LT1.DB_C10K_1" },
                new ElectricityCardItem { DisplayName = "C10K_2", MeterName = "PM_LOT207_LT1.DB_C10K_2" },
                new ElectricityCardItem { DisplayName = "L-L250A", MeterName = "PM_LOT207_LT1.DB_LL_250A" },
            }},
            new ElectricityCardDefinition { Title = "Building 207 - Detection 2nd floor", Building = "Building 207", Floor = "2nd floor", Items = new List<ElectricityCardItem>
            {
                new ElectricityCardItem { DisplayName = "2Z4", MeterName = "PM_LOT207_LT2.DB_2Z4" },
                new ElectricityCardItem { DisplayName = "2ZA", MeterName = "PM_LOT207_LT2.DB_2ZA" },
                new ElectricityCardItem { DisplayName = "AC AHU 2-1", MeterName = "ELM_LOT207_LT2.AHU21" },
                new ElectricityCardItem { DisplayName = "AC AHU 2-2", MeterName = "ELM_LOT207_LT2.AHU22" },
                new ElectricityCardItem { DisplayName = "DB_FFU_LT2", MeterName = "PM_LOT207_LT2.DB_FFU_LT2" },
                new ElectricityCardItem { DisplayName = "DB JOGAN", MeterName = "PM_LOT207_LT2.DB_JOGAN" },
                new ElectricityCardItem { DisplayName = "DB_LVP", MeterName = "PM_LOT207_LT2.DB_LVP" },
                new ElectricityCardItem { DisplayName = "DB_NCR1", MeterName = "PM_LOT207_LT2.DB_NCR1" },
                new ElectricityCardItem { DisplayName = "DB_NCR2", MeterName = "PM_LOT207_LT2.DB_NCR2" },
                new ElectricityCardItem { DisplayName = "DB_PHASE 6", MeterName = "PM_LOT207_LT2.DB_PHASE6" },
                new ElectricityCardItem { DisplayName = "DB_NEW POLARIS", MeterName = "PM_LOT207_LT2.DB_NEWPOLARIS" },
                new ElectricityCardItem { DisplayName = "DB_POL 10 11", MeterName = "PM_LOT207_LT2.DB_POL1011" },
                new ElectricityCardItem { DisplayName = "DB_POL 13-14", MeterName = "PM_LOT207_LT2.DB_POL1314" },
                new ElectricityCardItem { DisplayName = "DB_POL 15-17", MeterName = "PM_LOT207_LT2.DB_POL1517" },
                new ElectricityCardItem { DisplayName = "DB_POL18-20", MeterName = "PM_LOT207_LT2.DB_POL1820" },
                new ElectricityCardItem { DisplayName = "DB_PYRO 1.6", MeterName = "PM_LOT207_LT2.DB_PYRO" },
                new ElectricityCardItem { DisplayName = "DB_SAWING", MeterName = "PM_LOT207_LT2.DB_SAWING" },
                new ElectricityCardItem { DisplayName = "DB_T1", MeterName = "PM_LOT207_LT2.DB_T1" },
                new ElectricityCardItem { DisplayName = "DB_AC_OFFICE_DETECTION", MeterName = "PM_LOT207_LT2.DB_AC_OFFICE_DETECTION" },
                new ElectricityCardItem { DisplayName = "PM_DB_OFFICE_DETECTION", MeterName = "PM_LOT207_LT2.DB_OFFICE_DETECTION" },
                new ElectricityCardItem { DisplayName = "DB_P1", MeterName = "PM_LOT207_LT2.DB_P1" },
                new ElectricityCardItem { DisplayName = "DB_P2", MeterName = "PM_LOT207_LT2.DB_P2" },
                new ElectricityCardItem { DisplayName = "DB4", MeterName = "PM_LOT207_LT2.DB_4" },
                new ElectricityCardItem { DisplayName = "DB_MIELE", MeterName = "PM_LOT207_LT2.DB_MIELE" },
                new ElectricityCardItem { DisplayName = "AC IRD", MeterName = "PM_LOT207_LT2.DB_AC_IRD" },
                new ElectricityCardItem { DisplayName = "CHILLER_1-4", MeterName = "PM_LOT207_LT1.DB_CHILLER_1-4" },
                new ElectricityCardItem { DisplayName = "DB L3", MeterName = "PM_LOT207_LT2.DB_L3" },
                new ElectricityCardItem { DisplayName = "DB_REALIBILITY", MeterName = "PM_LOT207_LT3.DB_REALIBILITY" },
                new ElectricityCardItem { DisplayName = "VACUMP", MeterName = "PM_LOT207_LT1.DB_VACUUMP" },
            }},
            new ElectricityCardDefinition { Title = "Building 207 - Detection 3rd floor", Building = "Building 207", Floor = "3rd floor", Items = new List<ElectricityCardItem>
            {
                new ElectricityCardItem { DisplayName = "3Z1 ( ex SMD )", MeterName = "PM_LOT207_LT3.DB_EXSMD" },
                new ElectricityCardItem { DisplayName = "3Z1A (ex-Testing)", MeterName = "" },
                new ElectricityCardItem { DisplayName = "AC 3rd Floor", MeterName = "PM_LOT207_LT3.DB_AC3_3RDFLOOR" },
                new ElectricityCardItem { DisplayName = "LIGHTING FTC", MeterName = "PM_LOT207_LT3.DB_LIGHTING_FTC" },
                new ElectricityCardItem { DisplayName = "WORKSHOP", MeterName = "PM_LOT207_LT3.DB_WORKSHOP" },
                new ElectricityCardItem { DisplayName = "DB_REALIBILITY", MeterName = "PM_LOT207_LT3.DB_REALIBILITY" },
            }},
            new ElectricityCardDefinition { Title = "Building 207 - Detection Special Lighting floor", Building = "Building 207", Floor = "Special Lighting floor", Items = new List<ElectricityCardItem>
            {
                new ElectricityCardItem { DisplayName = "AC SL", MeterName = "PM_LOT207_LT3.DB_AC_SL" },
                new ElectricityCardItem { DisplayName = "Lighting Hydra", MeterName = "PM_LOT207_LT3.DB_LIGHTING_HYDRA" },
                new ElectricityCardItem { DisplayName = "DB SL200A", MeterName = "PM_LOT207_LT3.DB_SL200A" },
                new ElectricityCardItem { DisplayName = "HYD #1", MeterName = "PM_LOT207_LT3.HYDRA1" },
                new ElectricityCardItem { DisplayName = "HYD #2", MeterName = "PM_LOT207_LT3.HYDRA2" },
                new ElectricityCardItem { DisplayName = "HYD #3", MeterName = "PM_LOT207_LT3.HYDRA3" },
                new ElectricityCardItem { DisplayName = "HYD #4", MeterName = "PM_LOT207_LT3.HYDRA4" },
                new ElectricityCardItem { DisplayName = "DB A", MeterName = "PM_LOT207_LT3.DB_A" },
                new ElectricityCardItem { DisplayName = "DB UV", MeterName = "PM_LOT207_LT3.DBUV" },
                new ElectricityCardItem { DisplayName = "DB_H", MeterName = "PM_LOT207_LT3.DB_H" },
                new ElectricityCardItem { DisplayName = "DB_PHASE 1", MeterName = "PM_LOT207_LT3.DB_PHASE1" },
                new ElectricityCardItem { DisplayName = "DB_PHASE 2", MeterName = "PM_LOT207_LT3.DB_PHASE2" },
                new ElectricityCardItem { DisplayName = "AC 250A LSM", MeterName = "" },
                new ElectricityCardItem { DisplayName = "CNC", MeterName = "PM_LOT207_LT3.DB_CNC" },
            }},
            new ElectricityCardDefinition { Title = "Building 209 - 1st floor", Building = "Building 209", Floor = "1st floor", Items = new List<ElectricityCardItem>
            {
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PM_AC_CL2", MeterName = "PM_LOT209_LT1.PM_AC_CL2" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PM_CHILLER", MeterName = "PM_LOT209_LT1.PM_CHILLER" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PM_DB_CHILLER", MeterName = "PM_LOT209_LT1.PM_DB_CHILLER" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PM_DBCanteen", MeterName = "PM_LOT209_LT1.PM_DBCanteen" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PM_DBRO", MeterName = "PM_LOT209_LT1.PM_DBRO" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PM_MDB400A", MeterName = "PM_LOT209_LT1.PM_MDB400A" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PM_ULTRASONIC1", MeterName = "PM_LOT209_LT1.PM_ULTRASONIC1" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PM_ULTRASONIC2", MeterName = "PM_LOT209_LT1.PM_ULTRASONIC2" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PM_ULTRASONIC3", MeterName = "PM_LOT209_LT1.PM_ULTRASONIC3" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PM_ULTRASONIC4", MeterName = "PM_LOT209_LT1.PM_ULTRASONIC4" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PM_WWTP", MeterName = "PM_LOT209_LT2.PM_WWTP" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PMDB_AC320A", MeterName = "PM_LOT209_LT1.PMDB_AC320A" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PMDB_COATING", MeterName = "PM_LOT209_LT1.PMDB_COATING" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PMDB_COMPRESSOR", MeterName = "PM_LOT209_LT1.PMDB_COMPRESSOR" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PMDB_LGHTNG", MeterName = "PM_LOT209_LT1.PMDB_LGHTNG" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PMDB_LIGHTING_2", MeterName = "PM_LOT209_LT1.PMDB_LIGHTING_2" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PMDB_MC1", MeterName = "PM_LOT209_LT1.PMDB_MC1" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PMDB_MC2", MeterName = "PM_LOT209_LT1.PMDB_MC2" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PMDB_MC3", MeterName = "PM_LOT209_LT1.PMDB_MC3" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PMDB_POWER_100A", MeterName = "PM_LOT209_LT1.PMDB_POWER_100A" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PMDB_POWER_EXHAUST", MeterName = "PM_LOT209_LT1.PMDB_POWER_EXHAUST" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT1.PMMDB_320A", MeterName = "PM_LOT209_LT1.MDB_320A" },
                new ElectricityCardItem { DisplayName = "Total SSB Lt# 1 209", MeterName = "" },
            }},
            new ElectricityCardDefinition { Title = "Building 209 - 2nd floor", Building = "Building 209", Floor = "2nd floor", Items = new List<ElectricityCardItem>
            {
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PM_ASTROFURNACE2", MeterName = "PM_LOT209_LT2.PM_ASTROFURNACE2" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PM_ASTROFURNACE4", MeterName = "PM_LOT209_LT2.PM_ASTROFURNACE4" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PM_CAMCOFURNACE7", MeterName = "PM_LOT209_LT2.PM_CAMCOFURNACE7" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PM_CAMCOFURNACE8", MeterName = "PM_LOT209_LT2.PM_CAMCOFURNACE8" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PM_CHILLER13", MeterName = "PM_LOT209_LT2.PM_CHILLER13" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PM_LOT209_EXHAUST2_LT2", MeterName = "PM_LOT209_LT2.PM_LOT209_EXHAUST2_LT2" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PM_NIPLATING", MeterName = "PM_LOT209_LT2.PM_NIPLATING" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PMDB_AGING1", MeterName = "PM_LOT209_LT2.PMDB_AGING1" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PMDB_AGING2", MeterName = "PM_LOT209_LT2.PMDB_AGING2" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PMDB_ASTRO", MeterName = "PM_LOT209_LT2.PMDB_ASTRO" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PMDB1_250A", MeterName = "PM_LOT209_LT2.PMDB1_250A" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PMDB1_320A", MeterName = "PM_LOT209_LT2.PMDB1_320A" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PMDB2_320A", MeterName = "PM_LOT209_LT2.PMDB2_320A" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PMDB2_500A", MeterName = "PM_LOT209_LT2.PMDB2_500A" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PMDB3_320A", MeterName = "PM_LOT209_LT2.PMDB3_320A" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PMDB3_500A", MeterName = "PM_LOT209_LT2.PMDB3_500A" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PMDB4_LIGHTING", MeterName = "PM_LOT209_LT2.PMDB4_LIGHTING" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT2.PMPANEL_EXHAUST", MeterName = "PM_LOT209_LT2.PMPANEL_EXHAUST" },
                new ElectricityCardItem { DisplayName = "Total SSB Lt# 2 209", MeterName = "" },
            }},
            new ElectricityCardDefinition { Title = "Building 209 - 3rd floor", Building = "Building 209", Floor = "3rd floor", Items = new List<ElectricityCardItem>
            {
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT3.PM_CHILLER_ROOFTOP", MeterName = "PM_LOT209_LT3.PM_CHILLER_ROOFTOP" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT3.PMDB_COMPRESSOR_LT3", MeterName = "PM_LOT209_LT3.PMDB_COMPRESSOR_LT3" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT3.PMDB_EXHAUST_LT3", MeterName = "PM_LOT209_LT3.PMDB_EXHAUST_LT3" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT3.PMDB_LIGHTING_LT3", MeterName = "PM_LOT209_LT3.PMDB_LIGHTING_LT3" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT3.PMDB_MC_LT3", MeterName = "PM_LOT209_LT3.PMDB_MC_LT3" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT3.PMDB_MC2_LT3", MeterName = "PM_LOT209_LT3.PMDB_MC2_LT3" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT3.PMDB_POWER_LT3", MeterName = "PM_LOT209_LT3.PMDB_POWER_LT3" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT3.PMMDB_400A_LT3", MeterName = "PM_LOT209_LT3.PMMDB_400A_LT3" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT3.PMSSB3TB", MeterName = "PM_LOT209_LT3.PMSSB3TB" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT3.MDB_200A_AC_TC", MeterName = "PM_LOT209_LT3.MDB_200A_AC_TC" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT3.L_P", MeterName = "PM_LOT209_LT3.L_P" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT3.MC_TC", MeterName = "PM_LOT209_LT3.MC_TC" },
                new ElectricityCardItem { DisplayName = "PM_LOT209_LT3.P", MeterName = "PM_LOT209_LT3.P" },
            }},
            new ElectricityCardDefinition { Title = "Building 238", Building = "Building 238", Floor = "Overall / Main", Items = new List<ElectricityCardItem>
            {
                new ElectricityCardItem { DisplayName = "PM_LOT238.PM_MAIN", MeterName = "PM_LOT238.PM_MAIN" },
            }},
        };

        public static IReadOnlyList<ElectricityCardDefinition> AllCards => _cards;

        public static IReadOnlyList<ElectricityCardDefinition> GetCards(string? building = null)
        {
            if (string.IsNullOrWhiteSpace(building) || building.Equals("All", StringComparison.OrdinalIgnoreCase))
                return _cards;
            return _cards.Where(c => c.Building.Equals(building, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public static ElectricityCardDefinition? FindCard(string title) =>
            _cards.FirstOrDefault(c => c.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
    }
}
