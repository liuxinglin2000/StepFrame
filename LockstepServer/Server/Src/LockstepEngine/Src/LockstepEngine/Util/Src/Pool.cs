using System;
using System.Collections.Generic;
namespace Lockstep.Util
{
    // 回收利用接口
    public interface IRecyclable
    {
        void OnReuse();   // 在对象被重新使用时调用的方法
        void OnRecycle();  // 在对象被回收时调用的方法
    }

    // 可回收对象的基类
    public class BaseRecyclable : IRecyclable
    {
        public virtual void OnReuse() { }    // 默认为空实现
        public virtual void OnRecycle() { }  // 默认为空实现

        // 重写ToString方法，输出对象的Json格式字符串
        public override string ToString()
        {
            return JsonUtil.ToJson(this);
        }
    }

    // 对象池管理类
    public class Pool
    {
        // 将对象归还到对象池
        public static void Return<T>(T val) where T : IRecyclable, new()
        {
            Pool<T>.Return(val);
        }

        // 从对象池获取对象
        public static T Get<T>() where T : IRecyclable, new()
        {
            return Pool<T>.Get();
        }
    }

    // 泛型对象池类
    public class Pool<T> where T : IRecyclable, new()
    {
        private static Stack<T> pool = new Stack<T>();

        // 从对象池获取对象
        public static T Get()
        {
            if (pool.Count == 0)
            {
                return new T();
            }
            else
            {
                return pool.Pop();
            }
        }

        // 将对象归还到对象池
        public static void Return(T val)
        {
            if (val == null) return;
            val.OnRecycle();  // 调用对象的回收方法
            pool.Push(val);
        }
    }
}
