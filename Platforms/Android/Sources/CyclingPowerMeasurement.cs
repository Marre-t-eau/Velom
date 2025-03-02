
namespace Velom.Platforms.Android.Sources;

internal class CyclingPowerMeasurement
{
    public ushort? Flags { get; private set; }
    public ushort? InstantaneousPower { get; private set; }
    public ushort? PedalPowerBalance { get; private set; }
    public ushort? AccumulatedTorque { get; private set; }
    public uint? WheelRevolutions { get; private set; }
    public ushort? LastWheelEventTime { get; private set; }
    public ushort? CrankRevolutions { get; private set; }
    public ushort? LastCrankEventTime { get; private set; }

    public CyclingPowerMeasurement(byte[] data)
    {
        if (data == null || data.Length < 2)
            throw new ArgumentException("Invalid data");

        int offset = 0;

        Flags = BitConverter.ToUInt16(data, offset);
        offset += 2;

        if ((Flags & 0x0001) != 0 && offset + 2 <= data.Length) // Instantaneous Power present
        {
            InstantaneousPower = BitConverter.ToUInt16(data, offset);
            offset += 2;
        }

        if ((Flags & 0x0002) != 0 && offset + 1 <= data.Length) // Pedal Power Balance present
        {
            PedalPowerBalance = data[offset];
            offset += 1;
        }

        if ((Flags & 0x0004) != 0 && offset + 2 <= data.Length) // Accumulated Torque present
        {
            AccumulatedTorque = BitConverter.ToUInt16(data, offset);
            offset += 2;
        }

        if ((Flags & 0x0010) != 0 && offset + 4 <= data.Length) // Wheel Revolution Data present
        {
            WheelRevolutions = BitConverter.ToUInt32(data, offset);
            offset += 4;
            LastWheelEventTime = BitConverter.ToUInt16(data, offset);
            offset += 2;
        }

        if ((Flags & 0x0020) != 0 && offset + 4 <= data.Length) // Crank Revolution Data present
        {
            CrankRevolutions = BitConverter.ToUInt16(data, offset);
            offset += 2;
            LastCrankEventTime = BitConverter.ToUInt16(data, offset);
            offset += 2;
        }
    }
}