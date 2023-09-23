
List<IMyAirVent> _airVentList;
List<IMyAirVent> airVentList
{
    get
    {
        if (_airVentList != null) return _airVentList;
        _airVentList = new List<IMyAirVent>();
        GridTerminalSystem.GetBlocksOfType<IMyAirVent>(_airVentList, null);
        return _airVentList;
    }
}


List<IMyCockpit> _cockpitList;
List<IMyCockpit> cockpitList
{
    get
    {
        if (_cockpitList != null) return _cockpitList;
        _cockpitList = new List<IMyCockpit>();
        GridTerminalSystem.GetBlocksOfType<IMyCockpit>(_cockpitList, null);
        return _cockpitList;
    }
}


List<IMyGasTank> _gasTankList;
List<IMyGasTank> GasTankList
{
    get
    {
        if (_gasTankList != null && _gasTankList.Count != 0) return _gasTankList;
        _gasTankList = new List<IMyGasTank>();
        GridTerminalSystem.GetBlocksOfType<IMyGasTank>(_gasTankList, null);
        return _gasTankList;
    }
}


List<IMyGasTank> _gasTankO2List;
List<IMyGasTank> gasTankO2List
{
    get
    {
        if (_gasTankO2List != null) return _gasTankO2List;
        _gasTankO2List = new List<IMyGasTank>();
        GridTerminalSystem.GetBlocksOfType<IMyGasTank>(_gasTankO2List, x => x.DetailedInfo.Contains("Oxygen"));
        return _gasTankO2List;
    }
}


List<IMyGasTank> _gasTankH2List;
List<IMyGasTank> gasTankH2List
{
    get
    {
        if (_gasTankH2List != null) return _gasTankH2List;
        _gasTankH2List = new List<IMyGasTank>();
        GridTerminalSystem.GetBlocksOfType<IMyGasTank>(_gasTankH2List, x => x.DetailedInfo.Contains("Hydrogen"));
        return _gasTankH2List;
    }
}


List<IMyGasGenerator> _gasGeneratorList;
List<IMyGasGenerator> gasGeneratorList
{
    get
    {
        if (_gasGeneratorList != null) return _gasGeneratorList;
        _gasGeneratorList = new List<IMyGasGenerator>();
        GridTerminalSystem.GetBlocksOfType<IMyGasGenerator>(_gasGeneratorList, null);
        return _gasGeneratorList;
    }
}

float LowOxygenAirVent = 0.75f;
float FullOxygenAirVent = 0.9f;
double LowGasTanks = (double)0.45;
double FullGasTanks = (double)0.9;
double LowCockpit = (double)0.75;
double FullCockpit = (double)0.9;

bool LowCockpitDetected => CheckAnyCockpitLow(cockpitList);
bool LowHydrogenDetected => CheckAnyGasTankLow(gasTankH2List);
bool LowOxygenDetected => CheckAnyGasTankLow(gasTankO2List) || CheckAnyAirVentLow(airVentList);
bool LowGasDetected => CheckAnyGasTankLow(GasTankList) || CheckAnyAirVentLow(airVentList);
bool GeneratorsRequested = false;
bool OxygenCapacityFull => CheckALLCockpitFull(cockpitList);
bool HydrogenFull => CheckAllGasTanksFull(gasTankH2List);
bool OxygenFull => CheckAllGasTanksFull(gasTankO2List) && CheckAllAirVentsFull(airVentList);
bool GasFull => CheckAllGasTanksFull(GasTankList) && CheckAllAirVentsFull(airVentList);

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void Main(string argument, UpdateType updateSource)
{

    if (LowGasDetected)
    {
        foreach (var gg in gasGeneratorList)
        {
            gg.Enabled = true;
        }
        GeneratorsRequested = true;
    }

    if (GasFull)
    {
        foreach (var gg in gasGeneratorList)
        {
            gg.Enabled = false;
        }
        GeneratorsRequested = false;
    }

    Echo($"================================");
    Echo($"GasTank count :: {GasTankList.Count}.");
    Echo($"OxygenTank count :: {gasTankO2List.Count}.");
    Echo($"HydrogenTank count :: {gasTankH2List.Count}.");
    Echo($"GasGenerator count :: {gasGeneratorList.Count}.");
    Echo($"================================");
    Echo($"LowOxygenDetected :: {LowOxygenDetected}.");
    Echo($"LowHydrogenDetected :: {LowHydrogenDetected}.");
    Echo($"================================");
    Echo($"GeneratorsRequested :: {GeneratorsRequested}.");
    Echo($"================================");
    Echo($"OxygenFull :: {OxygenFull}.");
    Echo($"HydrogenFull :: {HydrogenFull}.");
    Echo($"================================");

}


bool CheckAnyAirVentLow(List<IMyAirVent> _myAirVents)
{
    bool LowVentDetected = false;

    foreach (var av in _myAirVents)
    {
        if (av.GetOxygenLevel() <= LowOxygenAirVent)
        {
            string toLog = av.GetOxygenLevel().ToString();
            Echo("NEW-OBJECT-DETECTED-------------------------");
            Echo("Detected Low AirVent OxygenLevel::" + toLog);
            toLog = "CustomName::" + av.CustomName;
            Echo(toLog);
            toLog = "EntityId::" + av.CubeGrid.EntityId.ToString();
            Echo(toLog);
            toLog = av.DetailedInfo;
            Echo("DetailedInfo::" + toLog);
            Echo("--------------------------------------------");
            LowVentDetected = true;
        }
    }

    return LowVentDetected;
}

bool CheckAnyGasTankLow(List<IMyGasTank> _myGasTanks)
{
    bool LowTanksDetected = false;

    foreach (var gt in _myGasTanks)
    {
        if (gt.FilledRatio <= LowGasTanks)
        {
            string toLog = gt.FilledRatio.ToString();
            Echo("NEW-OBJECT-DETECTED-------------------------");
            Echo("Detected Low GasTank FilledRatio::" + toLog);
            toLog = "CustomName::" + gt.CustomName;
            Echo(toLog);
            toLog = "EntityId::" + gt.CubeGrid.EntityId.ToString();
            Echo(toLog);
            toLog = gt.DetailedInfo;
            Echo("DetailedInfo::" + toLog);
            Echo("--------------------------------------------");
            LowTanksDetected = true;
        }
    }

    return LowTanksDetected;
}

bool CheckAllAirVentsFull(List<IMyAirVent> _myAirVents)
{
    bool bAllVentsFull = true;

    foreach (var av in _myAirVents)
    {
        if (av.GetOxygenLevel() < FullOxygenAirVent)
        {
            return false;
        }
    }

    return bAllVentsFull;
}

bool CheckAllGasTanksFull(List<IMyGasTank> _myGasTanks)
{
    foreach (var gt in _myGasTanks)
    {
        if (gt.FilledRatio < FullGasTanks)
        {
            return false;
        }
    }

    return true;
}