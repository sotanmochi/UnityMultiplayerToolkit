using UnityEngine;
using UniRx;
using UnityMultiplayerToolkit.Shared;
using UnityMultiplayerToolkit.MLAPIExtension;

namespace UnityMultiplayerToolkit.PerformanceTest
{
    public class NetworkPerformanceMonitor : MonoBehaviour
    {
        [SerializeField] GameObject _networkManagerObject;

        public float ProcessedEventsPerSecond => _processedEventsPerSecond;
        private float _processedEventsPerSecond;
        public float ReceivedDataKiloBytesPerSecond => _receivedDataKiloBytesPerSecond;
        private float _receivedDataKiloBytesPerSecond;

        public int ProcessedEvents => _processedEvents;
        private int _processedEvents;
        public float ReceivedDataKiloBytes => _receivedDataKiloBytes;
        private float _receivedDataKiloBytes;

        public int MaxReceiveEventsPerTickRate => _networkManager.MaxReceiveEventsPerTickRate;

        private readonly FixedSizeQueue<(float networkTime, int value)> _processedEventsQueue
            = new FixedSizeQueue<(float networkTime, int value)>(64);
        private readonly FixedSizeQueue<(float networkTime, ulong value)> _receivedDataBytesQueue
            = new FixedSizeQueue<(float networkTime, ulong value)>(64);
        private float _previousTickTime;

        private INetworkManager _networkManager;

        void Awake()
        {
            _networkManager = _networkManagerObject.GetComponent<INetworkManager>();
            if (_networkManager != null)
            {
                _networkManager.OnNetworkEarlyUpdatedAsObservable()
                .Subscribe(data => 
                {
                    _processedEvents += data.processedEventsPerTick;
                    _receivedDataKiloBytes += data.receivedDataBytesPerTick / 1024f;
                    _processedEventsQueue.Enqueue((data.networkTime, data.processedEventsPerTick));
                    _receivedDataBytesQueue.Enqueue((data.networkTime, data.receivedDataBytesPerTick));
                })
                .AddTo(this);
            }
        }

        void LateUpdate()
        {
            float currentTime = Time.unscaledTime;
            if (currentTime - _previousTickTime > 1.0f)
            {
                Tick();
                _previousTickTime = currentTime;
            }
        }

        private void Tick()
        {
            _processedEventsPerSecond = CalculateProcessedEventsPerSecond();
            _receivedDataKiloBytesPerSecond = CalculateReceivedDataKiloBytesPerSecond();
        }

        private float CalculateProcessedEventsPerSecond()
        {
            float timeDiff = 1;
            float processedEvents = 0;

            var processedEventsPerTick = _processedEventsQueue.ToArray();
            int dataCount = processedEventsPerTick.Length;
            for (int k = 0; k < dataCount; k++)
            {
                processedEvents += processedEventsPerTick[k].value;
            }
            if (dataCount > 0)
            {
                timeDiff = processedEventsPerTick[dataCount-1].networkTime - processedEventsPerTick[0].networkTime;
            }

            // int dataCount = _processedEventsQueue.Count;
            // if (dataCount > 2)
            // {
            //     var firstData = _processedEventsQueue.Dequeue();
            //     for (int k = 1; k < dataCount - 1; k++)
            //     {
            //         processedEvents += _processedEventsQueue.Dequeue().value;
            //     }
            //     var latestData = _processedEventsQueue.Dequeue();
            //     timeDiff = latestData.networkTime - firstData.networkTime;
            // }

            if (Mathf.Abs(timeDiff) > 0.001f)
            {
                processedEvents /= timeDiff;
            }

            return processedEvents;
        }

        private float CalculateReceivedDataKiloBytesPerSecond()
        {
            float timeDiff = 1;
            float receivedDataKiloBytes = 0.0f;

            var receivedDataBytesPerTick = _receivedDataBytesQueue.ToArray();
            int dataCount = receivedDataBytesPerTick.Length;
            for (int k = 0; k < dataCount; k++)
            {
                receivedDataKiloBytes += (float)receivedDataBytesPerTick[k].value / 1024f;
            }
            if (dataCount > 0)
            {
                timeDiff = receivedDataBytesPerTick[dataCount-1].networkTime - receivedDataBytesPerTick[0].networkTime;
            }

            // int dataCount = _receivedDataBytesQueue.Count;
            // if (dataCount > 2)
            // {
            //     var firstData = _receivedDataBytesQueue.Dequeue();
            //     for (int k = 1; k < dataCount - 1; k++)
            //     {
            //         receivedDataKiloBytes += (float)_receivedDataBytesQueue.Dequeue().value / 1024f;
            //     }
            //     var latestData = _receivedDataBytesQueue.Dequeue();
            //     timeDiff = latestData.networkTime - firstData.networkTime;
            // }

            if (Mathf.Abs(timeDiff) > 0.001f)
            {
                receivedDataKiloBytes /= timeDiff;
            }

            return receivedDataKiloBytes;
        }
    }
}