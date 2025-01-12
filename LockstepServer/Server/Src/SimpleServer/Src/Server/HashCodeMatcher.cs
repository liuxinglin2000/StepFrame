namespace Lockstep.FakeServer {
    public class HashCodeMatcher {
        // 哈希码
        public long hashCode;
        // 发送结果数组
        public bool[] sendResult;
        // 已匹配的次数
        public int count;

        // 构造函数，初始化哈希码匹配器
        public HashCodeMatcher(int num)
        {
            hashCode = 0;
            sendResult = new bool[num];
            count = 0;
        }

        // 是否完全匹配
        public bool IsMatchered => count == sendResult.Length;
    }
}

