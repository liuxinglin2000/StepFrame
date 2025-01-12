using Lockstep.Network;
using Lockstep.Serialization;
using NetMsg.Common;

namespace Lockstep.Game
{
    public class MessagePacker : IMessagePacker
    {
        // 单例模式，用于消息序列化和反序列化
        public static MessagePacker Instance { get; } = new MessagePacker();

        // 反序列化消息
        public object DeserializeFrom(ushort opcode, byte[] bytes, int index, int count)
        {
            var type = (EMsgSC)opcode;
            switch (type)
            {
                // Ping消息
                case EMsgSC.C2G_PlayerPing: return BaseFormater.FromBytes<Msg_C2G_PlayerPing>(bytes, index, count);
                case EMsgSC.G2C_PlayerPing: return BaseFormater.FromBytes<Msg_G2C_PlayerPing>(bytes, index, count);
                // 登录消息
                case EMsgSC.L2C_JoinRoomResult: return BaseFormater.FromBytes<Msg_L2C_JoinRoomResult>(bytes, index, count);
                case EMsgSC.C2L_JoinRoom: return BaseFormater.FromBytes<Msg_C2L_JoinRoom>(bytes, index, count);
                case EMsgSC.C2L_LeaveRoom: return BaseFormater.FromBytes<Msg_C2L_LeaveRoom>(bytes, index, count);
                case EMsgSC.C2G_LoadingProgress: return BaseFormater.FromBytes<Msg_C2G_LoadingProgress>(bytes, index, count);

                // 房间消息
                case EMsgSC.G2C_Hello: return BaseFormater.FromBytes<Msg_G2C_Hello>(bytes, index, count);
                case EMsgSC.G2C_FrameData: return BaseFormater.FromBytes<Msg_ServerFrames>(bytes, index, count);
                case EMsgSC.G2C_RepMissFrame: return BaseFormater.FromBytes<Msg_RepMissFrame>(bytes, index, count);
                case EMsgSC.G2C_GameEvent: return BaseFormater.FromBytes<Msg_G2C_GameEvent>(bytes, index, count);
                case EMsgSC.G2C_GameStartInfo: return BaseFormater.FromBytes<Msg_G2C_GameStartInfo>(bytes, index, count);
                case EMsgSC.G2C_LoadingProgress: return BaseFormater.FromBytes<Msg_G2C_LoadingProgress>(bytes, index, count);
                case EMsgSC.G2C_AllFinishedLoaded: return BaseFormater.FromBytes<Msg_G2C_AllFinishedLoaded>(bytes, index, count);

                // 玩家输入消息
                case EMsgSC.C2G_PlayerInput: return BaseFormater.FromBytes<Msg_PlayerInput>(bytes, index, count);
                // 请求缺失帧消息
                case EMsgSC.C2G_ReqMissFrame: return BaseFormater.FromBytes<Msg_ReqMissFrame>(bytes, index, count);
                // 确认缺失帧消息
                case EMsgSC.C2G_RepMissFrameAck: return BaseFormater.FromBytes<Msg_RepMissFrameAck>(bytes, index, count);
                // 哈希码消息
                case EMsgSC.C2G_HashCode: return BaseFormater.FromBytes<Msg_HashCode>(bytes, index, count);
            }

            return null;
        }

        // 序列化消息为字节数组
        public byte[] SerializeToByteArray(IMessage msg)
        {
            return ((BaseFormater)msg).ToBytes();
        }
    }
}
