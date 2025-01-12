using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Lockstep.Math;
using Lockstep.Util;
using Lockstep.Game;
using Lockstep.Network;
using NetMsg.Common;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Lockstep.Game
{
    /// <summary>
    /// 游戏的启动器，负责整个游戏框架的生命周期管理。
    /// 它包含了服务容器、管理器容器和时间机器容器等，用于管理各种服务、管理器以及支持时光机制。
    /// 此外，它还包含了一些与游戏状态相关的信息，例如是否为视频模式、是否在回放模式等。
    /// 在游戏的不同生命周期阶段，它会执行相应的初始化、更新和清理操作。
    /// </summary>
    [Serializable]
    public class Launcher : ILifeCycle
    {
        // 获取当前帧数
        public int CurTick => _serviceContainer.GetService<ICommonStateService>().Tick;
        // 静态实例，用于全局访问
        public static Launcher Instance { get; private set; }

        // 服务容器、管理器容器和时间机器容器
        private ServiceContainer _serviceContainer;
        private ManagerContainer _mgrContainer;
        private TimeMachineContainer _timeMachineContainer;
        private IEventRegisterService _registerService;

        // 记录路径、最大运行帧数、游戏启动信息和帧信息
        public string RecordPath; // 记录文件的路径
        public int MaxRunTick = int.MaxValue; // 最大运行帧数
        public Msg_G2C_GameStartInfo GameStartInfo; // 游戏启动信息
        public Msg_RepMissFrame FramesInfo; // 帧信息

        // 跳转到指定帧数
        public int JumpToTick = 10;

        // 模拟器服务和网络服务
        private SimulatorService _simulatorService = new SimulatorService();
        private NetworkService _networkService = new NetworkService();

        // 常量状态服务
        private IConstStateService _constStateService;
        public bool IsRunVideo => _constStateService.IsRunVideo;
        public bool IsVideoMode => _constStateService.IsVideoMode;
        public bool IsClientMode => _constStateService.IsClientMode;

        // 用于暂时存储Transform对象的变量
        public object transform;
        // 同步上下文，用于协程等异步操作
        private OneThreadSynchronizationContext _syncContext;

        // 在游戏对象被唤醒时执行的方法
        public void DoAwake(IServiceContainer services)
        {
            // 创建用于协程等异步操作的同步上下文
            _syncContext = new OneThreadSynchronizationContext();
            // 设置同步上下文，确保协程等异步操作在正确的上下文中执行
            SynchronizationContext.SetSynchronizationContext(_syncContext);
            // 启动一些服务，可能包括初始化日志服务等
            Utils.StartServices();

            // 如果实例已存在，则输出错误日志并返回
            if (Instance != null)
            {
                Debug.LogError("LifeCycle Error: Awake more than once!!");
                return;
            }

            // 设置实例为当前对象，以便全局访问
            Instance = this;

            // 获取服务容器
            _serviceContainer = services as ServiceContainer;

            // 创建事件注册服务实例
            _registerService = new EventRegisterService();

            // 创建管理器容器实例
            _mgrContainer = new ManagerContainer();

            // 创建时间机器容器实例
            _timeMachineContainer = new TimeMachineContainer();

            // 遍历所有服务，并注册到时间机器和管理器容器中
            var svcs = _serviceContainer.GetAllServices();
            foreach (var service in svcs)
            {
                _timeMachineContainer.RegisterTimeMachine(service as ITimeMachine);

                // 如果是 BaseService 类型的服务，则注册到管理器容器中
                if (service is BaseService baseService)
                {
                    _mgrContainer.RegisterManager(baseService);
                }
            }

            // 注册时间机器和事件注册服务到服务容器中
            _serviceContainer.RegisterService(_timeMachineContainer);
            _serviceContainer.RegisterService(_registerService);
        }


        // 在游戏开始时执行
        public void DoStart()
        {
            foreach (var mgr in _mgrContainer.AllMgrs)
            {
                mgr.InitReference(_serviceContainer, _mgrContainer);
            }

            // 绑定事件
            foreach (var mgr in _mgrContainer.AllMgrs)
            {
                _registerService.RegisterEvent<EEvent, GlobalEventHandler>("OnEvent_", "OnEvent_".Length,
                    EventHelper.AddListener, mgr);
            }

            foreach (var mgr in _mgrContainer.AllMgrs)
            {
                mgr.DoAwake(_serviceContainer);
            }

            _DoAwake(_serviceContainer);

            foreach (var mgr in _mgrContainer.AllMgrs)
            {
                mgr.DoStart();
            }

            _DoStart();
        }

        // 私有方法，用于在DoStart中执行一些初始化操作
        private void _DoAwake(IServiceContainer serviceContainer)
        {
            _simulatorService = serviceContainer.GetService<ISimulatorService>() as SimulatorService;
            _networkService = serviceContainer.GetService<INetworkService>() as NetworkService;
            _constStateService = serviceContainer.GetService<IConstStateService>();
            _constStateService = serviceContainer.GetService<IConstStateService>();

            // 如果是视频模式，设置快照帧间隔
            if (IsVideoMode)
            {
                _constStateService.SnapshotFrameInterval = 20;
                // 打开记录文件
                // OpenRecordFile(RecordPath);
            }
        }

        // 私有方法，用于在DoStart中执行一些初始化操作
        private void _DoStart()
        {
            if (IsVideoMode)
            {
                EventHelper.Trigger(EEvent.BorderVideoFrame, FramesInfo);
                EventHelper.Trigger(EEvent.OnGameCreate, GameStartInfo);
            }
            else if (IsClientMode)
            {
                GameStartInfo = _serviceContainer.GetService<IGameConfigService>().ClientModeInfo;
                EventHelper.Trigger(EEvent.OnGameCreate, GameStartInfo);
                EventHelper.Trigger(EEvent.LevelLoadDone, GameStartInfo);
            }
        }

        // 在每一帧更新时执行
        public void DoUpdate(float fDeltaTime)
        {
            _syncContext.Update();
            Utils.UpdateServices();
            var deltaTime = fDeltaTime.ToLFloat();
            _networkService.DoUpdate(deltaTime);
            if (IsVideoMode && IsRunVideo && CurTick < MaxRunTick)
            {
                _simulatorService.RunVideo();
                return;
            }

            if (IsVideoMode && !IsRunVideo)
            {
                _simulatorService.JumpTo(JumpToTick);
            }

            _simulatorService.DoUpdate(fDeltaTime);
        }

        // 在游戏对象被销毁时执行
        public void DoDestroy()
        {
            if (Instance == null) return;
            foreach (var mgr in _mgrContainer.AllMgrs)
            {
                mgr.DoDestroy();
            }

            Instance = null;
        }

        // 在应用程序退出时执行
        public void OnApplicationQuit()
        {
            DoDestroy();
        }
    }
}
