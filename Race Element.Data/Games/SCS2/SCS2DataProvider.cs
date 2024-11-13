using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using SCSSdkClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Games.EuroTruckSimulator2;
internal sealed class SCS2DataProvider : AbstractSimDataProvider
{
    private SCSSdkTelemetry _telemetry;
    private readonly Game _game;
    public SCS2DataProvider(Game game)
    {
        _game = game;
        _telemetry = new SCSSdkTelemetry() { };
        _telemetry.Data += OnTelemetryData;
    }

    public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        //gameData.Name 
        gameData.Name = _game.ToShortName();
    }

    internal override int PollingRate() => 3;

    internal override void Stop()
    {
        if (_telemetry != null)
        {
            _telemetry.Data -= OnTelemetryData;
            _telemetry.pause();
            _telemetry.Dispose();
        }
    }

    private void OnTelemetryData(SCSSdkClient.Object.SCSTelemetry data, bool newTimestamp)
    {
        SimDataProvider.LocalCar.Engine.MaxRpm = (int)data.TruckValues.ConstantsValues.MotorValues.EngineRpmMax;
        SimDataProvider.LocalCar.Engine.Rpm = (int)data.TruckValues.CurrentValues.DashboardValues.RPM;

        SimDataProvider.LocalCar.Engine.IsRunning = data.TruckValues.CurrentValues.EngineEnabled && !data.Paused;

        SimDataProvider.LocalCar.Inputs.Throttle = data.ControlValues.GameValues.Throttle;
        SimDataProvider.LocalCar.Inputs.Brake = data.ControlValues.GameValues.Brake;
        SimDataProvider.LocalCar.Inputs.Steering = data.ControlValues.GameValues.Steering;

        var acceleration = data.TruckValues.CurrentValues.AccelerationValues.CabinAngularAcceleration;
        SimDataProvider.LocalCar.Physics.Acceleration = new(acceleration.X , acceleration.Y , acceleration.Z );

        SimDataProvider.GameData.IsGamePaused = data.Paused;
    }

    public override List<string> GetCarClasses() => [];

    public override bool HasTelemetry() => false;

    internal override void Start()
    {

    }
}
