namespace Lockstep.FakeServer {
    public class HashCodeMatcher {
        // ��ϣ��
        public long hashCode;
        // ���ͽ������
        public bool[] sendResult;
        // ��ƥ��Ĵ���
        public int count;

        // ���캯������ʼ����ϣ��ƥ����
        public HashCodeMatcher(int num)
        {
            hashCode = 0;
            sendResult = new bool[num];
            count = 0;
        }

        // �Ƿ���ȫƥ��
        public bool IsMatchered => count == sendResult.Length;
    }
}

