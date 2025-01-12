using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Lockstep.Game;
using Lockstep.Logging;
using Lockstep.Network;
using Lockstep.Util;
using NetMsg.Common;

namespace Lockstep.FakeServer
{
    public class Server : IMessageDispatcher
    {
        // ����
        public static IPEndPoint serverIpPoint = NetworkUtil.ToIPEndPoint("127.0.0.1", 10083);
        private NetOuterProxy _netProxy = new NetOuterProxy();

        // ����
        private const double UpdateInterval = NetworkDefine.UPDATE_DELTATIME / 1000.0f; // ֡�� = 30
        private DateTime _lastUpdateTimeStamp;
        private DateTime _startUpTimeStamp;
        private double _deltaTime;
        private double _timeSinceStartUp;

        // �û�������
        private Game _game;
        private Dictionary<long, Player> _id2Player = new Dictionary<long, Player>();

        // ID
        private static int _idCounter = 0;
        private int _curCount = 0;

        // ����������
        public void Start()
        {
            _netProxy.MessageDispatcher = this;
            _netProxy.MessagePacker = MessagePacker.Instance;
            _netProxy.Awake(NetworkProtocol.TCP, serverIpPoint);
            _startUpTimeStamp = _lastUpdateTimeStamp = DateTime.Now;
        }

        // ��Ϣ�ַ�
        public void Dispatch(Session session, Packet packet)
        {
            // ��ȡ��Ϣ������
            ushort opcode = packet.Opcode();
            if (opcode == 39)
            {
                // ʾ�������������Ϊ39��ִ��ĳЩ����
                int i = 0;
            }

            // �����л���Ϣ
            var message = session.Network.MessagePacker.DeserializeFrom(opcode, packet.Bytes, Packet.Index,
                packet.Length - Packet.Index);
            // ����������Ϣ
            OnNetMsg(session, opcode, message as BaseMsg);
        }

        // ����������Ϣ
        void OnNetMsg(Session session, ushort opcode, BaseMsg msg)
        {
            // ��ȡ��Ϣ����
            var type = (EMsgSC)opcode;
            switch (type)
            {
                // ��¼��Ϣ
                // case EMsgSC.L2C_JoinRoomResult: 
                case EMsgSC.C2L_JoinRoom:
                    // �����������
                    OnPlayerConnect(session, msg);
                    return;
                case EMsgSC.C2L_LeaveRoom:
                    // ��������˳�
                    OnPlayerQuit(session, msg);
                    return;
                    // ������Ϣ
            }
            var player = session.GetBindInfo<Player>();
            _game?.OnNetMsg(player, opcode, msg);
        }

        // ���·�����״̬
        public void Update()
        {
            var now = DateTime.Now;
            _deltaTime = (now - _lastUpdateTimeStamp).TotalSeconds;
            if (_deltaTime > UpdateInterval)
            {
                _lastUpdateTimeStamp = now;
                _timeSinceStartUp = (now - _startUpTimeStamp).TotalSeconds;
                // ִ�и��²���
                DoUpdate();
            }
        }

        // ִ�и��²���
        public void DoUpdate()
        {
            // ���֡����
            var fDeltaTime = (float)_deltaTime;
            var fTimeSinceStartUp = (float)_timeSinceStartUp;
            _game?.DoUpdate(fDeltaTime);
        }

        // �����������
        void OnPlayerConnect(Session session, BaseMsg message)
        {
            // TODO �����ݿ��������
            var info = new Player();
            info.UserId = _idCounter++;
            info.PeerTcp = session;
            info.PeerUdp = session;
            _id2Player[info.UserId] = info;
            session.BindInfo = info;
            _curCount++;
            if (_curCount >= Game.MaxPlayerCount)
            {
                // TODO ��ʱ����
                _game = new Game();
                var players = new Player[_curCount];
                int i = 0;
                foreach (var player in _id2Player.Values)
                {
                    player.LocalId = (byte)i;
                    player.Game = _game;
                    players[i] = player;
                    i++;
                }
                _game.DoStart(0, 0, 0, players, "123");
            }

            Debug.Log("OnPlayerConnect count:" + _curCount + " ");
        }

        // ��������˳�
        void OnPlayerQuit(Session session, BaseMsg message)
        {
            var player = session.GetBindInfo<Player>();
            if (player == null)
                return;
            _curCount--;
            Debug.Log("OnPlayerQuit count:" + _curCount);
            _id2Player.Remove(player.UserId);
            if (_curCount == 0)
            {
                _game = null;
            }
        }
    }
}

