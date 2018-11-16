﻿using System.Diagnostics;
using System.IO;
using MessagePack;

namespace Drachenkatze.PresetMagician.NKSF.NKSF
{
    public class ControllerAssignmentsChunk : AbstractMessagePack
    {
        public const string RiffTypeID = "NICA";

        public override void Read(Stream source)
        {
            base.ReadData(source, RiffTypeID);
        }

        public override void Write(Stream target)
        {
            base.WriteData(target);
        }

        public override void DeserializeMessagePack(byte[] buffer)
        {
            Debug.WriteLine(MessagePackSerializer.ToJson(buffer));
        }

        public override string ChunkDescription
        {
            get
            {
                return "Native Instruments Controller Assignments";
            }
        }
    }
}