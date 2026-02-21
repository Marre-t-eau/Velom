using System.Buffers.Binary;

namespace Velom.Platforms.Android.Sources;

internal class HeartRateMeasurement
{
    public static readonly Guid guid = new Guid("00002A37-0000-1000-8000-00805f9b34fb"); // Heart Rate Measurement Characteristic

    public byte Flags { get; private set; }
    public bool IsSensorContactDetected => (Flags & 0x06) == 0x06;

    public ushort HeartRate { get; private set; }
    public ushort? EnergyExpended { get; private set; }
    public List<ushort>? RRIntervals { get; private set; }

    public HeartRateMeasurement(byte[] data)
    {
        if (data == null || data.Length < 2)
            throw new ArgumentException("Invalid data");

        int offset = 0;

        // Read flags
        Flags = data[offset++];

        // Heart Rate value is either 8bit or 16bit based on flag
        if ((Flags & 0x01) != 0)
        {
            HeartRate = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
            offset += 2;
        }
        else
        {
            HeartRate = data[offset++];
        }

        // Energy Expended (if present)
        if ((Flags & 0x08) != 0 && offset + 2 <= data.Length)
        {
            EnergyExpended = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
            offset += 2;
        }

        // RR Intervals (if present)
        if ((Flags & 0x10) != 0)
        {
            RRIntervals = new List<ushort>();
            // Read all remaining bytes as RR intervals (each 2 bytes)
            while (offset + 2 <= data.Length)
            {
                var rrInterval = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
                RRIntervals.Add(rrInterval);
                offset += 2;
            }
        }
    }
}
