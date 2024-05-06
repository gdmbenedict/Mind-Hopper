using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;
using Accord.Math;

namespace LSL4Unity.Samples.SimpleInlet
{ 
    // You probably don't need this namespace. We do it to avoid contaminating the global namespace of your project.
    public class SimpleInletScaleObject : MonoBehaviour
    {
        /*
         * This example shows the minimal code required to get an LSL inlet running
         * without leveraging any of the helper scripts that come with the LSL package.
         * This behaviour uses LSL.cs only. There is little-to-no error checking.
         * See Resolver.cs and BaseInlet.cs for helper behaviours to make your implementation
         * simpler and more robust.
         */

        // We need to find the stream somehow. You must provide a StreamName in editor or before this object is Started.
        public string StreamName;
        ContinuousResolver resolver;

        double max_chunk_duration = 0.2;  // Duration, in seconds, of buffer passed to pull_chunk. This must be > than average frame interval.

        // We need to keep track of the inlet once it is resolved.
        private StreamInlet inlet;

        // We need buffers to pass to LSL when pulling data.
        private float[,] data_buffer;  // Note it's a 2D Array, not array of arrays. Each element has to be indexed specifically, no frames/columns.
        private double[] timestamp_buffer;

        void Start()
        {
            if (!StreamName.Equals(""))
                resolver = new ContinuousResolver("name", StreamName);
            else
            {
                Debug.LogError("Object must specify a name for resolver to lookup a stream.");
                this.enabled = false;
                return;
            }
            StartCoroutine(ResolveExpectedStream());
        }

        IEnumerator ResolveExpectedStream()
        {

            var results = resolver.results();
            while (results.Length == 0)
            {
                Debug.Log("Waiting for stream...");
                yield return new WaitForSeconds(.1f);
                results = resolver.results();
            }

            Debug.Log("Stream found: " + results[0].name());
            inlet = new StreamInlet(results[0]);
            Debug.Log("Connected to stream.");

            // Prepare pull_chunk buffer
            int buf_samples = (int)Mathf.Ceil((float)(inlet.info().nominal_srate() * max_chunk_duration));
            Debug.Log("Allocating buffers to receive " + buf_samples + " samples.");
            int n_channels = inlet.info().channel_count();
            data_buffer = new float[buf_samples, n_channels];
            timestamp_buffer = new double[buf_samples];
        }

        public class BandpassFilter
        {
            private float sampleRate;
            private float lowFreq;
            private float highFreq;

            private float low;
            private float high;
            private float band;

            public BandpassFilter(float sampleRate, float lowFreq, float highFreq)
            {
                this.sampleRate = sampleRate;
                this.lowFreq = lowFreq;
                this.highFreq = highFreq;
            }

            public float Filter(float sample)
            {
                high = sample - ((lowFreq / sampleRate) * low) - band;
                band += (highFreq / sampleRate) * high;
                low += (highFreq / sampleRate) * band;

                return band;
            }
        }

        // Update is called once per frame
        void Update()
        {
            int samples_returned = 0;

            if (inlet != null) 
            {
                samples_returned = inlet.pull_chunk(data_buffer, timestamp_buffer);
                Debug.Log("Samples returned: " + samples_returned);
            }

            if (samples_returned > 0)
            {
                // Create a bandpass filter
                float fs = 256.0f;
                float f1 = 0.1f;
                float f2 = 30.0f;
                var bandpassFilter = new BandpassFilter(fs, f1, f2);

                // Assuming data_buffer is a 2D array with shape [samples_returned, num_channels]
                for (int channel = 0; channel < data_buffer.GetLength(1); channel++)
                {
                    float[] channel_data = new float[samples_returned];
                    for (int i = 0; i < samples_returned; i++)
                    {
                        channel_data[i] = data_buffer[i, channel];
                    }

                    // Apply the filter to each sample
                    float[] filtered_data = new float[samples_returned];
                    for (int i = 0; i < samples_returned; i++)
                    {
                        filtered_data[i] = bandpassFilter.Filter(channel_data[i]);
                    }

                    // Put the filtered data back into the buffer
                    for (int i = 0; i < samples_returned; i++)
                    {
                        data_buffer[i, channel] = filtered_data[i];
                    }
                    // continue further processing here
                }
            }
            else
            {
                Debug.LogError("No consumers available or inlet is null.");
            }

        }

    }
}