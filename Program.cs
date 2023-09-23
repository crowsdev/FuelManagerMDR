using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    internal partial class Program : MyGridProgram
    {
        #region Settings.

        private const float LowOxygenAirVent = 0.75f;
        private const float FullOxygenAirVent = 0.9f;

        private const float LowGasTanks = 0.45f;
        private const float FullGasTanks = 0.9f;

        private const float LowCockpit = 0.75f;
        private const float FullCockpit = 0.9f;

        #endregion

        #region Block Lists. Auto-properties.

        private List<IMyAirVent> _airVentList;

        private List<IMyAirVent> AirVentList
        {
            get
            {
                if (_airVentList != null) return _airVentList;
                _airVentList = new List<IMyAirVent>();
                GridTerminalSystem.GetBlocksOfType<IMyAirVent>(_airVentList, null);
                return _airVentList;
            }
        }


        private List<IMyCockpit> _cockpitList;

        private List<IMyCockpit> CockpitList
        {
            get
            {
                if (_cockpitList != null) return _cockpitList;
                _cockpitList = new List<IMyCockpit>();
                GridTerminalSystem.GetBlocksOfType<IMyCockpit>(_cockpitList, null);
                return _cockpitList;
            }
        }


        private List<IMyGasTank> _gasTankList;

        private List<IMyGasTank> GasTankList
        {
            get
            {
                if (_gasTankList != null && _gasTankList.Count != 0) return _gasTankList;
                _gasTankList = new List<IMyGasTank>();
                GridTerminalSystem.GetBlocksOfType<IMyGasTank>(_gasTankList, null);
                return _gasTankList;
            }
        }


        private List<IMyGasTank> _gasTankO2List;

        private List<IMyGasTank> GasTankO2List
        {
            get
            {
                if (_gasTankO2List != null) return _gasTankO2List;
                _gasTankO2List = new List<IMyGasTank>();
                GridTerminalSystem.GetBlocksOfType<IMyGasTank>(_gasTankO2List, x => x.DetailedInfo.Contains("Oxygen"));
                return _gasTankO2List;
            }
        }


        private List<IMyGasTank> _gasTankH2List;

        private List<IMyGasTank> GasTankH2List
        {
            get
            {
                if (_gasTankH2List != null) return _gasTankH2List;
                _gasTankH2List = new List<IMyGasTank>();
                GridTerminalSystem.GetBlocksOfType<IMyGasTank>(_gasTankH2List, x => x.DetailedInfo.Contains("Hydrogen"));
                return _gasTankH2List;
            }
        }


        private List<IMyGasGenerator> _gasGeneratorList;

        private List<IMyGasGenerator> GasGeneratorList
        {
            get
            {
                if (_gasGeneratorList != null) return _gasGeneratorList;
                _gasGeneratorList = new List<IMyGasGenerator>();
                GridTerminalSystem.GetBlocksOfType<IMyGasGenerator>(_gasGeneratorList, null);
                return _gasGeneratorList;
            }
        }

        #endregion

        #region Bools.

        private bool LowGasDetected => CheckAnyGasTankLow(GasTankList) || CheckAnyAirVentLow(AirVentList) || CheckAnyCockpitLow(CockpitList);
        private bool AllGasFull => CheckAllGasTanksFull(GasTankList) && CheckAllAirVentsFull(AirVentList) && CheckAllCockpitFull(CockpitList);
        private bool _generatorsRequested = false;

        #endregion

        #region In-Game script built-in methods.

        public Program() // Constructor. This executes when pb is started only.
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main(string argument, UpdateType updateSource) // Main method. This method executes once every 'n' ticks. n = Runtime.UpdateFrequency.
        {
            if (LowGasDetected) // Switch gas generators ON.
            {
                foreach (var gg in GasGeneratorList)
                {
                    gg.Enabled = true;
                }
                _generatorsRequested = true;
            }

            if (AllGasFull) // Switch gas generators OFF.
            {
                foreach (var gg in GasGeneratorList)
                {
                    gg.Enabled = false;
                }
                _generatorsRequested = false;
            }

            PrintStatus(_generatorsRequested); // Updates the pb text log.
        }

        public void Save() // Not currently needed.
        {
        }

        #endregion

        #region Cockpit checks O2.

        private bool CheckAllCockpitFull(List<IMyCockpit> myCockpitList) // Switch O2 production OFF condition.
        {
            if (myCockpitList == null || myCockpitList.Count == 0) return true;
            return !myCockpitList.Any(itemInList => itemInList.OxygenFilledRatio < FullCockpit);
        }

        private bool CheckAnyCockpitLow(List<IMyCockpit> myCockpitList) // Switch O2 production ON condition.
        {
            if (myCockpitList == null || myCockpitList.Count() == 0) return false;

            foreach (IMyCockpit cockpit in myCockpitList)
            {
                if (cockpit.OxygenFilledRatio <= LowCockpit)
                {
                    PrintEvent(cockpit.OxygenFilledRatio.ToString(), cockpit.CustomName, cockpit.EntityId.ToString(), cockpit.DetailedInfo);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Airvent checks O2.

        private bool CheckAnyAirVentLow(List<IMyAirVent> myAirVents)
        {
            if (myAirVents == null || myAirVents.Count == 0) return false;

            foreach (IMyAirVent airVent in myAirVents)
            {
                if (airVent.GetOxygenLevel() <= LowOxygenAirVent)
                {
                    PrintEvent(airVent.GetOxygenLevel().ToString(), airVent.CustomName, airVent.EntityId.ToString(), airVent.DetailedInfo);
                    return true;
                }
            }

            return false;
        }


        private bool CheckAllAirVentsFull(List<IMyAirVent> myAirVents)
        {
            if (myAirVents == null || myAirVents.Count == 0) return true;

            foreach (var airVent in myAirVents)
            {
                if (airVent.GetOxygenLevel() < FullOxygenAirVent)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Gas-Tank checks O2/H2.

        private bool CheckAnyGasTankLow(List<IMyGasTank> myGasTanks)
        {
            if (myGasTanks == null || myGasTanks.Count == 0) return false;

            foreach (var gt in myGasTanks)
            {
                if (gt.FilledRatio <= LowGasTanks)
                {
                    PrintEvent(gt.FilledRatio.ToString(), gt.CustomName, gt.EntityId.ToString(), gt.DetailedInfo);
                    return true;
                }
            }

            return false;
        }


        private bool CheckAllGasTanksFull(List<IMyGasTank> myGasTanks)
        {
            if (myGasTanks == null || myGasTanks.Count == 0) return true;

            foreach (var gasTank in myGasTanks)
            {
                if (gasTank.FilledRatio < FullGasTanks)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Info text display methods.

        private void PrintEvent(string level, string custName, string entId, string detailInfo)
        {
            Echo("====================MDR======================");
            Echo("|        <color=green>GAS GENERATION REQUESTED.</color>");
            Echo("| Low Gas Level Detected::" + level);
            Echo("| CustomName            ::" + custName);
            Echo("| EntityId              ::" + entId);
            Echo("| DetailedInfo          ::" + detailInfo);
            Echo("=============================================");
        }

        private void PrintStatus(bool generatorsReq)
        {
            Echo("====================MDR======================");
            Echo($"| GasTank count         :: {GasTankList.Count}.");
            Echo($"| OxygenTank count      :: {GasTankO2List.Count}.");
            Echo($"| HydrogenTank count    :: {GasTankH2List.Count}.");
            Echo($"| GasGenerator count    :: {GasGeneratorList.Count}.");
            Echo($"| Cockpit count         :: {CockpitList.Count}.");
            Echo($"| GeneratorsRequested   :: {generatorsReq}.");
            Echo("=============================================");
        }

        #endregion
    }
}
