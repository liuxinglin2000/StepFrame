using System;
using System.Collections.Generic;
namespace Lockstep.Util
{
    // �������ýӿ�
    public interface IRecyclable
    {
        void OnReuse();   // �ڶ�������ʹ��ʱ���õķ���
        void OnRecycle();  // �ڶ��󱻻���ʱ���õķ���
    }

    // �ɻ��ն���Ļ���
    public class BaseRecyclable : IRecyclable
    {
        public virtual void OnReuse() { }    // Ĭ��Ϊ��ʵ��
        public virtual void OnRecycle() { }  // Ĭ��Ϊ��ʵ��

        // ��дToString��������������Json��ʽ�ַ���
        public override string ToString()
        {
            return JsonUtil.ToJson(this);
        }
    }

    // ����ع�����
    public class Pool
    {
        // ������黹�������
        public static void Return<T>(T val) where T : IRecyclable, new()
        {
            Pool<T>.Return(val);
        }

        // �Ӷ���ػ�ȡ����
        public static T Get<T>() where T : IRecyclable, new()
        {
            return Pool<T>.Get();
        }
    }

    // ���Ͷ������
    public class Pool<T> where T : IRecyclable, new()
    {
        private static Stack<T> pool = new Stack<T>();

        // �Ӷ���ػ�ȡ����
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

        // ������黹�������
        public static void Return(T val)
        {
            if (val == null) return;
            val.OnRecycle();  // ���ö���Ļ��շ���
            pool.Push(val);
        }
    }
}
