using UnityEngine;
using UnityEngine.Profiling;

namespace UnityMultiplayerToolkit.PerformanceTest
{
    public class MemoryMonitor : MonoBehaviour
    {
        public int GCCollectCount => _gcCollectCount;
        private int _gcCollectCount;

        public long TotalReservedMemoryLong => _totalReservedMemoryLong;
        private long _totalReservedMemoryLong;

        public long TotalAllocatedMemoryLong => _totalAllocatedMemoryLong;
        private long _totalAllocatedMemoryLong;

        public long TotalUnusedReservedMemoryLong => _totalUnusedReservedMemoryLong;
        private long _totalUnusedReservedMemoryLong;

        public long TotalMonoHeapSizeLong => _totalMonoHeapSizeLong;
        private long _totalMonoHeapSizeLong;

        public long TotalMonoUsedSizeLong => _totalMonoUsedSizeLong;
        private long _totalMonoUsedSizeLong;

        void LateUpdate()
        {
            Tick();
        }

        private void Tick()
        {
            _gcCollectCount = System.GC.CollectionCount(0);
            _totalAllocatedMemoryLong = Profiler.GetTotalAllocatedMemoryLong();
            _totalReservedMemoryLong = Profiler.GetTotalReservedMemoryLong();
            _totalUnusedReservedMemoryLong = Profiler.GetTotalUnusedReservedMemoryLong();
            _totalMonoHeapSizeLong = Profiler.GetMonoHeapSizeLong();
            _totalMonoUsedSizeLong = Profiler.GetMonoUsedSizeLong();
        }
    }
}
