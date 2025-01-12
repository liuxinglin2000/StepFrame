using Lockstep.Network;
using Lockstep.Util;
using NetMsg.Common;


namespace Lockstep.FakeServer
{
    public class Player : BaseRecyclable
    {
        // 用户ID
        public long UserId;

        // 用户账号
        public string Account;

        // 登录哈希值
        public string LoginHash;

        // 本地ID
        public byte LocalId;

        // TCP连接
        public Session PeerTcp;

        // UDP连接
        public Session PeerUdp;

        // 玩家所在游戏
        public Game Game;

        // 玩家游戏数据
        public GameData GameData;

        // 玩家所在游戏的ID
        public int GameId => Game?.GameId ?? -1;

        // 离开游戏时的清理操作
        public void OnLeave()
        {
            PeerTcp = null;
            PeerUdp = null;
        }

        // 发送TCP消息
        public void SendTcp(EMsgSC type, BaseMsg msg)
        {
            PeerTcp?.Send(0x00, (ushort)type, msg.ToBytes());
        }

        // 发送TCP消息（带原始数据）
        public void SendTcp(EMsgSC type, byte[] data)
        {
            PeerTcp?.Send(0x00, (ushort)type, data);
        }

        // 发送UDP消息（带原始数据）
        public void SendUdp(EMsgSC type, byte[] data)
        {
            PeerUdp?.Send(0x00, (ushort)type, data);
        }

        // 发送UDP消息
        public void SendUdp(EMsgSC type, BaseMsg msg)
        {
            PeerUdp?.Send(0x00, (ushort)type, msg.ToBytes());
        }

        // 回收对象时的清理操作
        public override void OnRecycle()
        {
            PeerTcp = null;
            PeerUdp = null;
        }
    }
}
