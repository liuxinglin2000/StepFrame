using Lockstep.Game;
using Lockstep.Math;
using Lockstep.Game;
using UnityEngine;


public class MainScript : MonoBehaviour
{
    public Launcher launcher = new Launcher(); // 游戏逻辑启动器
    public int MaxEnemyCount = 10; // 最大敌人数量
    public bool IsClientMode = false; // 是否为客户端模式
    public bool IsRunVideo; // 是否运行视频
    public bool IsVideoMode = false; // 是否为视频模式
    public string RecordFilePath; // 记录文件路径
    public bool HasInit = false; // 游戏是否已初始化

    private ServiceContainer _serviceContainer; // 服务容器实例

    private void Awake()
    {
        Application.runInBackground = true; // 启用后台运行
        gameObject.AddComponent<PingMono>(); // 添加PingMono组件
        gameObject.AddComponent<InputMono>(); // 添加InputMono组件
        _serviceContainer = new UnityServiceContainer(); // 初始化服务容器
        _serviceContainer.GetService<IConstStateService>().GameName = "ARPGDemo"; // 设置游戏名称
        _serviceContainer.GetService<IConstStateService>().IsClientMode = IsClientMode; // 设置客户端模式
        _serviceContainer.GetService<IConstStateService>().IsVideoMode = IsVideoMode; // 设置视频模式
        _serviceContainer.GetService<IGameStateService>().MaxEnemyCount = MaxEnemyCount; // 设置最大敌人数量
        Lockstep.Logging.Logger.OnMessage += UnityLogHandler.OnLog; // 注册日志处理函数
        Screen.SetResolution(1024, 768, false); // 设置屏幕分辨率

        launcher.DoAwake(_serviceContainer); // 执行启动器的初始化
    }

    private void Start()
    {
        var stateService = GetService<IConstStateService>();
        string path = Application.dataPath;
#if UNITY_EDITOR
        path = Application.dataPath + "/../../../";
#elif UNITY_STANDALONE_OSX
        path = Application.dataPath + "/../../../../../";
#elif UNITY_STANDALONE_WIN
        path = Application.dataPath + "/../../../";
#endif
        Debug.Log(path); // 输出路径信息
        stateService.RelPath = path; // 设置相对路径
        launcher.DoStart(); // 启动游戏逻辑
        HasInit = true; // 标记游戏已初始化
    }

    private void Update()
    {
        _serviceContainer.GetService<IConstStateService>().IsRunVideo = IsVideoMode; // 设置视频运行标志
        launcher.DoUpdate(Time.deltaTime); // 更新游戏逻辑
    }

    private void OnDestroy()
    {
        launcher.DoDestroy(); // 执行启动器的清理工作
    }

    private void OnApplicationQuit()
    {
        launcher.OnApplicationQuit(); // 处理应用程序退出逻辑
    }

    // 从服务容器中获取指定类型的服务
    public T GetService<T>() where T : IService
    {
        return _serviceContainer.GetService<T>();
    }
}
