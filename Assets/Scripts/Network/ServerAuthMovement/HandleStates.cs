using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HandleStates
{
    public class InputState
    {
        public int tick;
        public Vector2 moveInput;
        public Vector2 lookAround;
    }

    public class TransformStateRW : INetworkSerializable
    {
        public int tick;
        public Vector3 finalPoss;
        public Quaternion finalRot;
        public bool isMoving;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter 
        {
            if(serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out tick);
                reader.ReadValueSafe(out finalPoss);
                reader.ReadValueSafe(out finalRot);
                reader.ReadValueSafe(out isMoving);
            } else { 
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(tick);
                writer.WriteValueSafe(finalPoss);
                writer.WriteValueSafe(finalRot);    
                writer.WriteValueSafe(isMoving);
            }
        }
    }
}
