using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Tools
{
    public class PerformanceLogger
    {
        struct PerformanceRecord
        {
            public float pathLength;
            public double executionTimeMs;
            public int vertexCount;
            public long actionsTaken;

            public override string ToString()
            {
                return
                    $"{pathLength:F0},{executionTimeMs:F0},{vertexCount},{actionsTaken}";
            }
        }

        public double lastExecutionTimeMs => _stopwatch.ElapsedMilliseconds;

        private readonly string _logFilePath;
        private readonly StringBuilder _csvBuilder;
        private readonly List<PerformanceRecord> _records;
        private readonly Stopwatch _stopwatch;

        public PerformanceLogger(string logFileName = "performance_log.csv")
        {
            var myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string logsDirectory = Path.Combine(myDocumentsPath, "Pathfinding_Logs");
            Directory.CreateDirectory(logsDirectory);

            _stopwatch = new Stopwatch();
            _logFilePath = Path.Combine(logsDirectory, logFileName);
            _csvBuilder = new StringBuilder();
            _records = new List<PerformanceRecord>();

            _csvBuilder.AppendLine("PathLength,ExecutionTimeMs,VertexCount,ActionsTaken");
        }

        public void Start()
        {
            _stopwatch.Start();
        }

        public void Stop()
        {
            _stopwatch.Stop();
        }

        public void LogPerformance(float pathLength, int vertexCount, long actionsTaken)
        {
            var record = new PerformanceRecord
            {
                vertexCount = vertexCount,
                pathLength = pathLength,
                executionTimeMs = _stopwatch.ElapsedMilliseconds,
                actionsTaken = actionsTaken
            };

            _records.Add(record);
            _csvBuilder.AppendLine(record.ToString());

            UnityEngine.Debug.Log(
                $"Path Length: {pathLength}, Vertices: {vertexCount:N0}, Actions Taken: {actionsTaken}" +
                $"Time: {record.executionTimeMs:F2}ms ");
        }

        public void SaveToFile()
        {
            try
            {
                File.WriteAllText(_logFilePath, _csvBuilder.ToString(), Encoding.UTF8);
                UnityEngine.Debug.Log($"Log saved to: {_logFilePath}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log($"Error saving log file: {ex.Message}");
            }
        }

        ~PerformanceLogger()
        {
            SaveToFile();
        }
    }
}