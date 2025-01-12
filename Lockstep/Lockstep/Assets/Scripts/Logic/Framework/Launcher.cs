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
    /// ��Ϸ��������������������Ϸ��ܵ��������ڹ���
    /// �������˷���������������������ʱ����������ȣ����ڹ�����ַ��񡢹������Լ�֧��ʱ����ơ�
    /// ���⣬����������һЩ����Ϸ״̬��ص���Ϣ�������Ƿ�Ϊ��Ƶģʽ���Ƿ��ڻط�ģʽ�ȡ�
    /// ����Ϸ�Ĳ�ͬ�������ڽ׶Σ�����ִ����Ӧ�ĳ�ʼ�������º����������
    /// </summary>
    [Serializable]
    public class Launcher : ILifeCycle
    {
        // ��ȡ��ǰ֡��
        public int CurTick => _serviceContainer.GetService<ICommonStateService>().Tick;
        // ��̬ʵ��������ȫ�ַ���
        public static Launcher Instance { get; private set; }

        // ����������������������ʱ���������
        private ServiceContainer _serviceContainer;
        private ManagerContainer _mgrContainer;
        private TimeMachineContainer _timeMachineContainer;
        private IEventRegisterService _registerService;

        // ��¼·�����������֡������Ϸ������Ϣ��֡��Ϣ
        public string RecordPath; // ��¼�ļ���·��
        public int MaxRunTick = int.MaxValue; // �������֡��
        public Msg_G2C_GameStartInfo GameStartInfo; // ��Ϸ������Ϣ
        public Msg_RepMissFrame FramesInfo; // ֡��Ϣ

        // ��ת��ָ��֡��
        public int JumpToTick = 10;

        // ģ����������������
        private SimulatorService _simulatorService = new SimulatorService();
        private NetworkService _networkService = new NetworkService();

        // ����״̬����
        private IConstStateService _constStateService;
        public bool IsRunVideo => _constStateService.IsRunVideo;
        public bool IsVideoMode => _constStateService.IsVideoMode;
        public bool IsClientMode => _constStateService.IsClientMode;

        // ������ʱ�洢Transform����ı���
        public object transform;
        // ͬ�������ģ�����Э�̵��첽����
        private OneThreadSynchronizationContext _syncContext;

        // ����Ϸ���󱻻���ʱִ�еķ���
        public void DoAwake(IServiceContainer services)
        {
            // ��������Э�̵��첽������ͬ��������
            _syncContext = new OneThreadSynchronizationContext();
            // ����ͬ�������ģ�ȷ��Э�̵��첽��������ȷ����������ִ��
            SynchronizationContext.SetSynchronizationContext(_syncContext);
            // ����һЩ���񣬿��ܰ�����ʼ����־�����
            Utils.StartServices();

            // ���ʵ���Ѵ��ڣ������������־������
            if (Instance != null)
            {
                Debug.LogError("LifeCycle Error: Awake more than once!!");
                return;
            }

            // ����ʵ��Ϊ��ǰ�����Ա�ȫ�ַ���
            Instance = this;

            // ��ȡ��������
            _serviceContainer = services as ServiceContainer;

            // �����¼�ע�����ʵ��
            _registerService = new EventRegisterService();

            // ��������������ʵ��
            _mgrContainer = new ManagerContainer();

            // ����ʱ���������ʵ��
            _timeMachineContainer = new TimeMachineContainer();

            // �������з��񣬲�ע�ᵽʱ������͹�����������
            var svcs = _serviceContainer.GetAllServices();
            foreach (var service in svcs)
            {
                _timeMachineContainer.RegisterTimeMachine(service as ITimeMachine);

                // ����� BaseService ���͵ķ�����ע�ᵽ������������
                if (service is BaseService baseService)
                {
                    _mgrContainer.RegisterManager(baseService);
                }
            }

            // ע��ʱ��������¼�ע����񵽷���������
            _serviceContainer.RegisterService(_timeMachineContainer);
            _serviceContainer.RegisterService(_registerService);
        }


        // ����Ϸ��ʼʱִ��
        public void DoStart()
        {
            foreach (var mgr in _mgrContainer.AllMgrs)
            {
                mgr.InitReference(_serviceContainer, _mgrContainer);
            }

            // ���¼�
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

        // ˽�з�����������DoStart��ִ��һЩ��ʼ������
        private void _DoAwake(IServiceContainer serviceContainer)
        {
            _simulatorService = serviceContainer.GetService<ISimulatorService>() as SimulatorService;
            _networkService = serviceContainer.GetService<INetworkService>() as NetworkService;
            _constStateService = serviceContainer.GetService<IConstStateService>();
            _constStateService = serviceContainer.GetService<IConstStateService>();

            // �������Ƶģʽ�����ÿ���֡���
            if (IsVideoMode)
            {
                _constStateService.SnapshotFrameInterval = 20;
                // �򿪼�¼�ļ�
                // OpenRecordFile(RecordPath);
            }
        }

        // ˽�з�����������DoStart��ִ��һЩ��ʼ������
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

        // ��ÿһ֡����ʱִ��
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

        // ����Ϸ��������ʱִ��
        public void DoDestroy()
        {
            if (Instance == null) return;
            foreach (var mgr in _mgrContainer.AllMgrs)
            {
                mgr.DoDestroy();
            }

            Instance = null;
        }

        // ��Ӧ�ó����˳�ʱִ��
        public void OnApplicationQuit()
        {
            DoDestroy();
        }
    }
}
