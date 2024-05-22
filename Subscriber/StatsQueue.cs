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

        public double StandardDeviation => Math.Sqrt(Variance);

        public bool IsValueAnOutlier(double newValue, double zScoreThreshold)
        {
            double mean = Mean;
            double stdDev = StandardDeviation;
        
            // Compute the Z-score for newValue
            double zScore = (newValue - mean) / stdDev;
        
            return Math.Abs(zScore) > zScoreThreshold; // Check if the absolute Z-score is greater than the threshold
        }
    }

}