using Lockstep.Network;
using Lockstep.Util;
using NetMsg.Common;


namespace Lockstep.FakeServer
{
    public class Player : BaseRecyclable
    {
        // �û�ID
        public long UserId;

        // �û��˺�
        public string Account;

        // ��¼��ϣֵ
        public string LoginHash;

        // ����ID
        public byte LocalId;

        // TCP����
        public Session PeerTcp;

        // UDP����
        public Session PeerUdp;

        // ���������Ϸ
        public Game Game;

        // �����Ϸ����
        public GameData GameData;

        // ���������Ϸ��ID
        public int GameId => Game?.GameId ?? -1;

        // �뿪��Ϸʱ���������
        public void OnLeave()
        {
            PeerTcp = null;
            PeerUdp = null;
        }

        // ����TCP��Ϣ
        public void SendTcp(EMsgSC type, BaseMsg msg)
        {
            PeerTcp?.Send(0x00, (ushort)type, msg.ToBytes());
        }

        // ����TCP��Ϣ����ԭʼ���ݣ�
        public void SendTcp(EMsgSC type, byte[] data)
        {
            PeerTcp?.Send(0x00, (ushort)type, data);
        }

        // ����UDP��Ϣ����ԭʼ���ݣ�
        public void SendUdp(EMsgSC type, byte[] data)
        {
            PeerUdp?.Send(0x00, (ushort)type, data);
        }

        // ����UDP��Ϣ
        public void SendUdp(EMsgSC type, BaseMsg msg)
        {
            PeerUdp?.Send(0x00, (ushort)type, msg.ToBytes());
        }

        // ���ն���ʱ���������
        public override void OnRecycle()
        {
            PeerTcp = null;
            PeerUdp = null;
        }
    }
}
