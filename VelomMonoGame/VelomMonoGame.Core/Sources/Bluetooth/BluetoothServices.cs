
using System;

namespace VelomMonoGame.Core.Sources.Bluetooth;

public static class BluetoothServices
{
    public static readonly Guid FitnessMachineServiceUuid = new Guid("00001826-0000-1000-8000-00805f9b34fb"); // Fitness Machine Service
    public static readonly Guid HeartRateServiceUuid = new Guid("0000180D-0000-1000-8000-00805f9b34fb"); // Heart Rate Service
}
