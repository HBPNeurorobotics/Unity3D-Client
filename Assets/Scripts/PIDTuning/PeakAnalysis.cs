﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PIDTuning
{
    public struct OscillationAnalysisResult
    {
        /// <summary>
        /// Average Same-Sign Peak interval in seconds
        /// </summary>
        public readonly float UltimatePeriod;

        /// <summary>
        /// Zero-to-Peak Amplitude average of all peaks
        /// </summary>
        public readonly float Amplitude;

        public OscillationAnalysisResult(float ultimatePeriod, float amplitude)
        {
            UltimatePeriod = ultimatePeriod;
            Amplitude = amplitude;
        }
    }

    public static class PeakAnalysis
    {
        public static List<int> FindPeakIndices(IList<KeyValuePair<DateTime, PidStepDataEntry>> data)
        {
            // I have found that the input data is very slightly noisy. I decided to go for a more thorough,
            // but slightly slower peak detection algorithms

            // Basically, we slide a 5-sample-wide window over the input data: [a][b][c][d][e]
            // c is a peak iff:
            // - either a <= b <= c >= d >= e && c > avg(a,b,d,e)
            // - or a >= b >= c <= d <= e && c < avg(a,b,d,e)
            // - AND the last peak was at least 1 full degree different

            List<int> peakIndices = new List<int>();

            float? lastPeak = null;

            for (int potentialPeakIdx = 2; potentialPeakIdx < data.Count - 2; potentialPeakIdx++)
            {
                var a = data[potentialPeakIdx - 2].Value.Measured;
                var b = data[potentialPeakIdx - 1].Value.Measured;
                var c = data[potentialPeakIdx].Value.Measured;
                var d = data[potentialPeakIdx + 1].Value.Measured;
                var e = data[potentialPeakIdx + 2].Value.Measured;

                if (((a <= b && b <= c && c >= d && d >= e && c > (a + b + d + e) / 4f) || // positive peaks
                    (a >= b && b >= c && c <= d && d <= e && c < (a + b + d + e) / 4f)) && // negative peaks
                    (lastPeak == null || Mathf.Abs(c - lastPeak.Value) >= 1f)) // no duplicate peaks (difference smaller than 1)
                {
                    peakIndices.Add(potentialPeakIdx);
                    lastPeak = c;
                }
            }

            return peakIndices;
        }

        public static OscillationAnalysisResult? AnalyzeOscillation(PidStepData stepData)
        {
            const int MIN_PEAKS_FOR_OSCILLATION = 6;

            var stepDataAsList = stepData.Data.ToList();
            var peaks = FindPeakIndices(stepDataAsList).Select(peak => stepDataAsList[peak]).ToList();

            if (peaks.Count < MIN_PEAKS_FOR_OSCILLATION)
            {
                return null;
            }
            else
            {
                float avgAmplitude = peaks.Average(datedEntry => datedEntry.Value.AbsoluteError);

                var posPeaks = peaks.Where(datedEntry => datedEntry.Value.SignedError >= 0f).ToList();

                var avgInterval = (float)posPeaks
                    .Skip(1)
                    .Select((datedEntry, i) => new KeyValuePair<DateTime, DateTime>(datedEntry.Key, posPeaks[i].Key))
                    .Average(nextLastPairs => (nextLastPairs.Key - nextLastPairs.Value).TotalSeconds);

                return new OscillationAnalysisResult(avgInterval, avgAmplitude);
            }
        }
    }
}       