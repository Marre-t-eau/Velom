using System;
using System.Buffers.Binary;

namespace VelomMonoGame.Core.Sources.Bluetooth.Characteristics;

public class FitnessMachineControlPoint
{
    public static readonly Guid guid = new Guid("00002AD9-0000-1000-8000-00805f9b34fb"); // Fitness Machine Control Point Characteristic

    public byte Opcode { get; private set; }
    public byte[] Parameters { get; private set; }

    private FitnessMachineControlPoint(byte opcode, byte[] parameters)
    {
        Opcode = opcode;
        Parameters = parameters;
    }

    public byte[] ToByteArray()
    {
        byte[] data = new byte[1 + Parameters.Length];
        data[0] = Opcode;
        Array.Copy(Parameters, 0, data, 1, Parameters.Length);
        return data;
    }

    // Opcodes for Fitness Machine Control Point
    internal static class Opcodes
    {
        public const byte RequestControl = 0x00;
        public const byte Reset = 0x01;
        public const byte SetTargetSpeed = 0x02;
        public const byte SetTargetInclination = 0x03;
        public const byte SetTargetResistanceLevel = 0x04;
        public const byte SetTargetPower = 0x05;
        public const byte SetTargetHeartRate = 0x06;
        public const byte StartOrResume = 0x07;
        public const byte StopOrPause = 0x08;
    }

    public static FitnessMachineControlPoint CreateSetTargetPowerCommand(ushort powerLevel)
    {
        byte[] parameters = new byte[2];
        BinaryPrimitives.WriteUInt16LittleEndian(parameters, powerLevel);
        return new FitnessMachineControlPoint(Opcodes.SetTargetPower, parameters);
    }

    public static FitnessMachineControlPoint CreateResetCommand()
    {
        return new FitnessMachineControlPoint(Opcodes.Reset, Array.Empty<byte>());
    }

    public static FitnessMachineControlPoint CreateStartOrResumeCommand()
    {
        return new FitnessMachineControlPoint(Opcodes.StartOrResume, Array.Empty<byte>());
    }

    public static FitnessMachineControlPoint CreateStopOrPauseCommand()
    {
        return new FitnessMachineControlPoint(Opcodes.StopOrPause, Array.Empty<byte>());
    }

    public static FitnessMachineControlPoint CreateRequestControlCommand()
    {
        return new FitnessMachineControlPoint(Opcodes.RequestControl, Array.Empty<byte>());
    }

    public static FitnessMachineControlPoint CreateStopAndResetCommand()
    {
        return new FitnessMachineControlPoint(Opcodes.StopOrPause | Opcodes.Reset, Array.Empty<byte>());
    }
}
