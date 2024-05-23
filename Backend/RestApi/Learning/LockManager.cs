using RestApi.Common;

namespace RestApi.Learning
{
    public sealed class LockManager
    {
        public static Dictionary<AlgorithmName, object> Locks { get; } = new Dictionary<AlgorithmName, object>();

        public LockManager()
        {
            foreach (AlgorithmName algorithm in Enum.GetValues(typeof(AlgorithmName)))
            {
                Locks.Add(algorithm, new object());
            }
        }

        public static object GetLock(AlgorithmName key)
        {
            lock (Locks)
            {
                return Locks[key];
            }
        }
    }
}
