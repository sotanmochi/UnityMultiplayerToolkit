using UnityEngine;
using TMPro;

namespace UnityMultiplayerToolkit.PerformanceTest
{
    public class PerformanceTestView : MonoBehaviour
    {
        [SerializeField] TMP_Text _applicationFrameRate;
        [SerializeField] TMP_Text _processedEvents;
        [SerializeField] TMP_Text _receivedDataSize;
        [SerializeField] TMP_Text _processedEventsPerUnitTime;
        [SerializeField] TMP_Text _receivedDataSizePerUnitTime;
        [SerializeField] TMP_Text _maxReceiveEventsRate;

        [SerializeField] TMP_Text _gcCollectCount;
        [SerializeField] TMP_Text _totalAllocatedMemory;
        [SerializeField] TMP_Text _totalUnusedReservedMemory;
        [SerializeField] TMP_Text _totalReservedMemory;
        [SerializeField] TMP_Text _totalMonoUsedSize;
        [SerializeField] TMP_Text _totalMonoHeapSize;

        public void SetApplicationFrameRate(string unit, float value)
        {
            string text = $"Application Frame Rate: {value:F2} {unit}";
            _applicationFrameRate.SetText(text);
        }

        public void SetProcessedEvents(float processedEvents)
        {
            string text = $"Processed Events: {processedEvents}";
            _processedEvents.SetText(text);
        }

        public void SetReceivedDataSize(string unitOfDataSize, float receivedDataSize)
        {
            string text = $"Received Data Size: {receivedDataSize} {unitOfDataSize}";
            _receivedDataSize.SetText(text);
        }

        public void SetProcessedEventsPerUnitTime(string unitTime, float processedEvents)
        {
            string text = $"Processed Events Per {unitTime}: {processedEvents}";
            _processedEventsPerUnitTime.SetText(text);
        }

        public void SetReceivedDataSizePerUnitTime(string unitTime, string unitOfDataSize, float receivedDataSize)
        {
            string text = $"Received Data Size Per {unitTime}: {receivedDataSize} {unitOfDataSize}";
            _receivedDataSizePerUnitTime.SetText(text);
        }

        public void SetMaxReceiveEventsRate(string unitTime, float maxReceiveEventsRate)
        {
            string text = $"Max Receive Events Per {unitTime}: {maxReceiveEventsRate}";
            _maxReceiveEventsRate.SetText(text);
        }

        public void SetCGCollectCount(int value)
        {
            string text = $"GC Collect Count: {value}";
            _gcCollectCount.SetText(text);
        }

        public void SetTotalAllocatedMemory(string unitOfDataSize, float value)
        {
            string text = $"Total Allocated Memory: {value} {unitOfDataSize}";
            _totalAllocatedMemory.SetText(text);
        }

        public void SetTotalUnusedReservedMemory(string unitOfDataSize, float value)
        {
            string text = $"Total Unused Reserved Memory: {value} {unitOfDataSize}";
            _totalUnusedReservedMemory.SetText(text);
        }

        public void SetTotalReservedMemory(string unitOfDataSize, float value)
        {
            string text = $"Total Reserved Memory: {value} {unitOfDataSize}";
            _totalReservedMemory.SetText(text);
        }

        public void SetTotalMonoUsedSize(string unitOfDataSize, float value)
        {
            string text = $"Total Mono Used Size: {value} {unitOfDataSize}";
            _totalMonoUsedSize.SetText(text);
        }

        public void SetTotalMonoHeapSize(string unitOfDataSize, float value)
        {
            string text = $"Total Mono Heap Size: {value} {unitOfDataSize}";
            _totalMonoHeapSize.SetText(text);
        }
    }
}