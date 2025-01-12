using Lockstep.Math;

namespace Lockstep.Game
{
    // 定义生命周期接口，规定了游戏生命周期中的关键方法
    public interface ILifeCycle
    {
        // 在游戏对象被唤醒时执行的方法，用于初始化一些必要的操作
        void DoAwake(IServiceContainer services);

        // 在游戏开始时执行的方法，用于执行游戏逻辑的初始化操作
        void DoStart();

        // 在游戏对象被销毁时执行的方法，用于清理资源和执行必要的清理操作
        void DoDestroy();

        // 在应用程序退出时执行的方法，用于处理应用程序退出逻辑
        void OnApplicationQuit();
    }
}
