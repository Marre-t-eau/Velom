using System;

namespace VelomMonoGame.Core.Sources.Bluetooth.Characteristics;

public class FitnessMachineFeature
{
    public static readonly Guid guid = new Guid("00002ACC-0000-1000-8000-00805f9b34fb");

    private readonly byte[] featureData;

    public FitnessMachineFeature(byte[] data)
    {
        featureData = data;
    }

    public bool HasAverageSpeedSupported => (featureData[0] & 0x01) != 0;
    public bool HasCadenceSupported => (featureData[0] & 0x02) != 0;
    public bool HasTotalDistanceSupported => (featureData[0] & 0x04) != 0;
    public bool HasInclinationSupported => (featureData[0] & 0x08) != 0;
    public bool HasElevationGainSupported => (featureData[0] & 0x10) != 0;
    public bool HasPaceSupported => (featureData[0] & 0x20) != 0;
    public bool HasStepCountSupported => (featureData[0] & 0x40) != 0;
    public bool HasResistanceLevelSupported => (featureData[0] & 0x80) != 0;
    public bool HasStrideCountSupported => (featureData[1] & 0x01) != 0;
    public bool HasExpendedEnergySupported => (featureData[1] & 0x02) != 0;
    public bool HasHeartRateMeasurementSupported => (featureData[1] & 0x04) != 0;
    public bool HasMetabolicEquivalentSupported => (featureData[1] & 0x08) != 0;
    public bool HasElapsedTimeSupported => (featureData[1] & 0x10) != 0;
    public bool HasRemainingTimeSupported => (featureData[1] & 0x20) != 0;
    public bool HasPowerMeasurementSupported => (featureData[1] & 0x40) != 0;
    public bool HasForceOnBeltSupported => (featureData[1] & 0x80) != 0;
    public bool HasSpeedTargetSettingSupported => (featureData[4] & 0x01) != 0;
    public bool HasInclinationTargetSettingSupported => (featureData[4] & 0x02) != 0;
    public bool HasResistanceTargetSettingSupported => (featureData[4] & 0x04) != 0;
    public bool HasPowerTargetSettingSupported => (featureData[4] & 0x08) != 0;
    public bool HasHeartRateTargetSettingSupported => (featureData[4] & 0x10) != 0;
    public bool HasTargetedExpendedEnergySupported => (featureData[4] & 0x20) != 0;
    public bool HasTargetedStepNumberSupported => (featureData[4] & 0x40) != 0;
    public bool HasTargetedStrideNumberSupported => (featureData[4] & 0x80) != 0;
    public bool HasTargetedDistanceSupported => (featureData[5] & 0x01) != 0;
    public bool HasTargetedTrainingTimeSupported => (featureData[5] & 0x02) != 0;
    public bool HasTargetedTimeInTwoHeartRateZonesSupported => (featureData[5] & 0x04) != 0;
    public bool HasTargetedTimeInThreeHeartRateZonesSupported => (featureData[5] & 0x08) != 0;
    public bool HasTargetedTimeInFiveHeartRateZonesSupported => (featureData[5] & 0x10) != 0;
}
