using System.Buffers.Binary;

namespace Velom.Platforms.Android.Sources;

internal class IndoorBikeData
{
    public ushort Flags { get; private set; }
    public ushort? InstantaneousSpeed { get; private set; }
    public ushort? AverageSpeed { get; private set; }
    public ushort? InstantaneousCadence { get; private set; }
    public ushort? AverageCadence { get; private set; }
    public ushort? TotalDistance { get; private set; }
    public ushort? ResistanceLevel { get; private set; }
    public ushort? InstantaneousPower { get; private set; }
    public ushort? AveragePower { get; private set; }
    public ushort? TotalEnergy { get; private set; }
    public ushort? EnergyPerHour { get; private set; }
    public ushort? EnergyPerMinute { get; private set; }
    public ushort? HeartRate { get; private set; }
    public ushort? MetabolicEquivalent { get; private set; }
    public ushort? ElapsedTime { get; private set; }
    public ushort? RemainingTime { get; private set; }

    public IndoorBikeData(byte[] data)
    {
        if (data == null || data.Length < 2)
            throw new ArgumentException("Invalid data");

        int offset = 0;

        Flags = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
        offset += 2;

        // Instantaneous Speed is always included
        InstantaneousSpeed = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
        offset += 2;

        if ((Flags & 0x0002) != 0 && offset + 2 <= data.Length) // Average Speed present
        {
            AverageSpeed = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
            offset += 2;
        }

        if ((Flags & 0x0004) != 0 && offset + 2 <= data.Length) // Instantaneous Cadence present
        {
            InstantaneousCadence = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
            offset += 2;
        }

        if ((Flags & 0x0008) != 0 && offset + 2 <= data.Length) // Average Cadence present
        {
            AverageCadence = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
            offset += 2;
        }

        if ((Flags & 0x0010) != 0 && offset + 2 <= data.Length) // Total Distance present
        {
            TotalDistance = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
            offset += 2;
        }

        if ((Flags & 0x0020) != 0 && offset + 2 <= data.Length) // Resistance Level present
        {
            ResistanceLevel = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
            offset += 2;
        }

        if ((Flags & 0x0040) != 0 && offset + 2 <= data.Length) // Instantaneous Power present
        {
            InstantaneousPower = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
            offset += 2;
        }

        if ((Flags & 0x0080) != 0 && offset + 2 <= data.Length) // Average Power present
        {
            AveragePower = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
            offset += 2;
        }

        if ((Flags & 0x0100) != 0 && offset + 2 <= data.Length) // Total Energy present
        {
            TotalEnergy = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
            offset += 2;
        }

        if ((Flags & 0x0200) != 0 && offset + 2 <= data.Length) // Energy per Hour present
        {
            EnergyPerHour = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
            offset += 2;
        }

        if ((Flags & 0x0400) != 0 && offset + 2 <= data.Length) // Energy per Minute present
        {
            EnergyPerMinute = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
            offset += 2;
        }

        if ((Flags & 0x0800) != 0 && offset + 1 <= data.Length) // Heart Rate present
        {
            HeartRate = data[offset];
            offset += 1;
        }

        if ((Flags & 0x1000) != 0 && offset + 1 <= data.Length) // Metabolic Equivalent present
        {
            MetabolicEquivalent = data[offset];
            offset += 1;
        }

        if ((Flags & 0x2000) != 0 && offset + 2 <= data.Length) // Elapsed Time present
        {
            ElapsedTime = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
            offset += 2;
        }

        if ((Flags & 0x4000) != 0 && offset + 2 <= data.Length) // Remaining Time present
        {
            RemainingTime = BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>(data, offset, 2));
            offset += 2;
        }
    }
}
