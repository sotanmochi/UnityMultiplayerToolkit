using UnityEngine;
using TMPro;
using Cysharp.Text;

namespace UnityMultiplayerToolkit.PerformanceTest
{
    public class PerformanceTestView : MonoBehaviour
    {
        [SerializeField] TMP_Text _applicationFrameRate;
        [SerializeField] TMP_Text _connectedClientCount;
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
            _applicationFrameRate.SetTextFormat("Application Frame Rate: {0:F2} {1}", value, unit);
        }

        public void SetConnectedClientCount(int value)
        {
            _connectedClientCount.SetTextFormat("Connected Client Count: {0}", value);
        }

        public void SetProcessedEvents(float processedEvents)
        {
            _processedEvents.SetTextFormat("Processed Events: {0}", processedEvents);
        }

        public void SetReceivedDataSize(string unitOfDataSize, float receivedDataSize)
        {
            _receivedDataSize.SetTextFormat("Received Data Size: {0} {1}", receivedDataSize, unitOfDataSize);
        }

        public void SetProcessedEventsPerUnitTime(string unitTime, float processedEvents)
        {
            _processedEventsPerUnitTime.SetTextFormat("Processed Events Per {0}: {1}", unitTime, processedEvents);
        }

        public void SetReceivedDataSizePerUnitTime(string unitTime, string unitOfDataSize, float receivedDataSize)
        {
            _receivedDataSizePerUnitTime.SetTextFormat("Received Data Size Per {0}: {1} {2}", unitTime, receivedDataSize, unitOfDataSize);
        }

        public void SetMaxReceiveEventsRate(string unitTime, float maxReceiveEventsRate)
        {
            _maxReceiveEventsRate.SetTextFormat("Max Receive Events Per {0}: {1}", unitTime, maxReceiveEventsRate);
        }

        public void SetCGCollectCount(int value)
        {
            _gcCollectCount.SetTextFormat("GC Collect Count: {0}", value);
        }

        public void SetTotalAllocatedMemory(string unitOfDataSize, float value)
        {
            _totalAllocatedMemory.SetTextFormat("Total Allocated Memory: {0} {1}", value, unitOfDataSize);
        }

        public void SetTotalUnusedReservedMemory(string unitOfDataSize, float value)
        {
            _totalUnusedReservedMemory.SetTextFormat("Total Unused Reserved Memory: {0} {1}", value, unitOfDataSize);
        }

        public void SetTotalReservedMemory(string unitOfDataSize, float value)
        {
            _totalReservedMemory.SetTextFormat("Total Reserved Memory: {0} {1}", value, unitOfDataSize);
        }

        public void SetTotalMonoUsedSize(string unitOfDataSize, float value)
        {
            _totalMonoUsedSize.SetTextFormat("Total Mono Used Size: {0} {1}", value, unitOfDataSize);
        }

        public void SetTotalMonoHeapSize(string unitOfDataSize, float value)
        {
            _totalMonoHeapSize.SetTextFormat("Total Mono Heap Size: {0} {1}", value, unitOfDataSize);
        }
    }
}