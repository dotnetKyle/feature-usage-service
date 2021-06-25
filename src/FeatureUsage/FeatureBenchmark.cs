using System;
using System.Diagnostics;

namespace FeatureUsage
{
    public class FeatureBenchmark : IDisposable
    {
        Stopwatch stopWatch;
        Action<FeatureBenchmark> _recordBenchmarkAction;

        internal FeatureBenchmark(string featureName, Action<FeatureBenchmark> recordBenchmark, DateTime? now = null)
        {
            if (now.HasValue == false)
                now = DateTime.UtcNow;

            _recordBenchmarkAction = recordBenchmark;
            FeatureName = featureName;

            stopWatch = new Stopwatch();
            stopWatch.Start();

            DtgUtc = now.Value;
        }

        public DateTime DtgUtc { get; private set; }
        public string FeatureName { get; private set; }
        public int? BenchmarkMs { get; private set; } = null;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
            }

            if(stopWatch.IsRunning)
            {
                stopWatch.Stop();
            }

            // get the running time for the stop watch
            BenchmarkMs = (stopWatch.ElapsedMilliseconds < int.MaxValue)
                ? (int)stopWatch.ElapsedMilliseconds
                : int.MaxValue;

            _recordBenchmarkAction(this);
        }
    }
}
