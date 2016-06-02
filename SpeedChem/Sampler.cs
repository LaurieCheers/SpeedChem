using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedChem
{
    namespace Sampler
    {
        class Float
        {
            int numSamples;
            List<float> samples;
            int nextSampleIdx;
            public float average { get; private set; }

            public Float(int numSamples)
            {
                this.numSamples = numSamples;
                this.samples = new List<float>(numSamples);
                this.nextSampleIdx = 0;
            }

            public void Clear()
            {
                this.samples.Clear();
                this.nextSampleIdx = 0;
            }

            public void AddSample(float value)
            {
                if(samples.Count() <= nextSampleIdx)
                    samples.Add(value);
                else
                    samples[nextSampleIdx] = value;
                nextSampleIdx = (nextSampleIdx + 1) % numSamples;
                average = CalcAverage();
            }

            float CalcAverage()
            {
                float result = 0;
                float divisor = 1.0f / samples.Count();
                foreach (float f in samples)
                {
                    result += f * divisor;
                }
                return result;
            }
        }
    }

}
