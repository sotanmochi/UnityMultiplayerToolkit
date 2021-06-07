using UnityEngine;

namespace UnityMultiplayerToolkit.PerformanceTest
{
    public class PerformanceTestPresenter : MonoBehaviour
    {
        [SerializeField] PerformanceTestView _view;
        [SerializeField] NetworkPerformanceMonitor _networkPerformanceMonitor;
        [SerializeField] MemoryMonitor _memoryMonitor;
        [SerializeField] FrameRateMonitor _frameRateMonitor;

        private readonly float _gigaBytes = 1024f * 1024f * 1024f;
        private readonly float _megaBytes = 1024f * 1024f;

        void LateUpdate()
        {
            _view.SetApplicationFrameRate("[fps]", _frameRateMonitor.FramePerSecond);

            _view.SetConnectedClientCount(_networkPerformanceMonitor.ConnectedClientCount);
            _view.SetProcessedEvents(_networkPerformanceMonitor.ProcessedEvents);
            _view.SetReceivedDataSize("[KB]", _networkPerformanceMonitor.ReceivedDataKiloBytes);
            _view.SetProcessedEventsPerUnitTime("[Second]", _networkPerformanceMonitor.ProcessedEventsPerSecond);
            _view.SetReceivedDataSizePerUnitTime("[Second]", "[KB]", _networkPerformanceMonitor.ReceivedDataKiloBytesPerSecond);
            _view.SetMaxReceiveEventsRate("[Tick]", _networkPerformanceMonitor.MaxReceiveEventsPerTickRate);

            _view.SetCGCollectCount(_memoryMonitor.GCCollectCount);
            _view.SetTotalAllocatedMemory("[GB]", _memoryMonitor.TotalAllocatedMemoryLong / _gigaBytes);
            _view.SetTotalMonoUsedSize("[MB]", _memoryMonitor.TotalMonoUsedSizeLong / _megaBytes);
            _view.SetTotalUnusedReservedMemory("[GB]", _memoryMonitor.TotalUnusedReservedMemoryLong / _gigaBytes);
            _view.SetTotalReservedMemory("[GB]", _memoryMonitor.TotalReservedMemoryLong / _gigaBytes);
            _view.SetTotalMonoHeapSize("[MB]", _memoryMonitor.TotalMonoHeapSizeLong / _megaBytes);
        }
    }
}