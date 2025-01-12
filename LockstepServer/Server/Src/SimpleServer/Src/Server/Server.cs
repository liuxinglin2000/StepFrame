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
        // 网络
        public static IPEndPoint serverIpPoint = NetworkUtil.ToIPEndPoint("127.0.0.1", 10083);
        private NetOuterProxy _netProxy = new NetOuterProxy();

        // 更新
        private const double UpdateInterval = NetworkDefine.UPDATE_DELTATIME / 1000.0f; // 帧率 = 30
        private DateTime _lastUpdateTimeStamp;
        private DateTime _startUpTimeStamp;
        private double _deltaTime;
        private double _timeSinceStartUp;

        // 用户管理器
        private Game _game;
        private Dictionary<long, Player> _id2Player = new Dictionary<long, Player>();

        // ID
        private static int _idCounter = 0;
        private int _curCount = 0;

        // 启动服务器
        public void Start()
        {
            _netProxy.MessageDispatcher = this;
            _netProxy.MessagePacker = MessagePacker.Instance;
            _netProxy.Awake(NetworkProtocol.TCP, serverIpPoint);
            _startUpTimeStamp = _lastUpdateTimeStamp = DateTime.Now;
        }

        // 消息分发
        public void Dispatch(Session session, Packet packet)
        {
            // 获取消息操作码
            ushort opcode = packet.Opcode();
            if (opcode == 39)
            {
                // 示例：如果操作码为39，执行某些操作
                int i = 0;
            }

            // 反序列化消息
            var message = session.Network.MessagePacker.DeserializeFrom(opcode, packet.Bytes, Packet.Index,
                packet.Length - Packet.Index);
            // 处理网络消息
            OnNetMsg(session, opcode, message as BaseMsg);
        }

        // 处理网络消息
        void OnNetMsg(Session session, ushort opcode, BaseMsg msg)
        {
            // 获取消息类型
            var type = (EMsgSC)opcode;
            switch (type)
            {
                // 登录消息
                // case EMsgSC.L2C_JoinRoomResult: 
                case EMsgSC.C2L_JoinRoom:
                    // 处理玩家连接
                    OnPlayerConnect(session, msg);
                    return;
                case EMsgSC.C2L_LeaveRoom:
                    // 处理玩家退出
                    OnPlayerQuit(session, msg);
                    return;
                    // 房间消息
            }
            var player = session.GetBindInfo<Player>();
            _game?.OnNetMsg(player, opcode, msg);
        }

        // 更新服务器状态
        public void Update()
        {
            var now = DateTime.Now;
            _deltaTime = (now - _lastUpdateTimeStamp).TotalSeconds;
            if (_deltaTime > UpdateInterval)
            {
                _lastUpdateTimeStamp = now;
                _timeSinceStartUp = (now - _startUpTimeStamp).TotalSeconds;
                // 执行更新操作
                DoUpdate();
            }
        }

        // 执行更新操作
        public void DoUpdate()
        {
            // 检查帧输入
            var fDeltaTime = (float)_deltaTime;
            var fTimeSinceStartUp = (float)_timeSinceStartUp;
            _game?.DoUpdate(fDeltaTime);
        }

        // 处理玩家连接
        void OnPlayerConnect(Session session, BaseMsg message)
        {
            // TODO 从数据库加载数据
            var info = new Player();
            info.UserId = _idCounter++;
            info.PeerTcp = session;
            info.PeerUdp = session;
            _id2Player[info.UserId] = info;
            session.BindInfo = info;
            _curCount++;
            if (_curCount >= Game.MaxPlayerCount)
            {
                // TODO 临时代码
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

        // 处理玩家退出
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

