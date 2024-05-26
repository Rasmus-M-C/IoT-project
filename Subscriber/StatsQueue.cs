using System;
using System.Collections.Generic;
using System.Linq;

namespace Subscriber
{
    public class StatsQueue
    {
        private Queue<float> values;
        private int capacity;

        public StatsQueue(int capacity)
        {
            this.values = new Queue<float>(capacity);
            this.capacity = capacity;
        }

        public void AddValue(float newValue)
        {
            if (values.Count == capacity)
            {
                values.Dequeue(); // Remove the oldest value if at capacity
            }
            values.Enqueue(newValue);
        }

        public double Mean => values.Average();

        public double Variance
        {
            get
            {
                double mean = Mean;
                return values.Sum(val => (val - mean) * (val - mean)) / values.Count;
            }
        }

        public double StandardDeviation
        {
            get
            {
                return values.Count > 1 ? Math.Sqrt(Variance) : 0.0;
            }
        }

        public bool IsValueAnOutlier(double newValue, double zScoreThreshold)
        {
            double mean = Mean;
            double stdDev = StandardDeviation;

            if (stdDev == 0)
            {
                // All values are the same, consider newValue as non-outlier if it's equal to mean
                return Math.Abs(newValue - mean) > 0;
            }

            // Compute the Z-score for newValue
            double zScore = (newValue - mean) / stdDev;

            return Math.Abs(zScore) > zScoreThreshold; // Check if the absolute Z-score is greater than the threshold
        }
        public double IsValueAnOutlierDouble(double newValue)
        {
            double mean = Mean;
            double stdDev = StandardDeviation;

            if (stdDev == 0)
            {
                // All values are the same, consider newValue as non-outlier if it's equal to mean
                return Math.Abs(newValue - mean);
            }

            // Compute the Z-score for newValue
            double zScore = (newValue - mean) / stdDev;

            return Math.Abs(zScore); // Check if the absolute Z-score is greater than the threshold
        }

        public bool IsQueueNotFull()
        {
            return values.Count < capacity;
        }
    }
}