using Lockstep.Math;

namespace Lockstep.Game
{
    // �����������ڽӿڣ��涨����Ϸ���������еĹؼ�����
    public interface ILifeCycle
    {
        // ����Ϸ���󱻻���ʱִ�еķ��������ڳ�ʼ��һЩ��Ҫ�Ĳ���
        void DoAwake(IServiceContainer services);

        // ����Ϸ��ʼʱִ�еķ���������ִ����Ϸ�߼��ĳ�ʼ������
        void DoStart();

        // ����Ϸ��������ʱִ�еķ���������������Դ��ִ�б�Ҫ���������
        void DoDestroy();

        // ��Ӧ�ó����˳�ʱִ�еķ��������ڴ���Ӧ�ó����˳��߼�
        void OnApplicationQuit();
    }
}
