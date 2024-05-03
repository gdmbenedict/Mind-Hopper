using Accord.Signal
using Accord.Math

void Update()
{
    if (inlet != null)
    {
        int samples_returned = inlet.pull_chunk(data_buffer, timestamp_buffer);
        Debug.Log("Samples returned: " + samples_returned);
        if (samples_returned > 0)
        {
            // Create a bandpass filter
            double fs = 1000.0; // replace with your actual sample rate
            double f1 = 0.1;
            double f2 = 30.0;
            var bandpass = new BandPass(f1, f2, fs);

            // Apply the filter to each channel of the data
            for (int i = 0; i < data_buffer.GetLength(1); i++)
            {
                double[] channel_data = new double[samples_returned];
                for (int j = 0; j < samples_returned; j++)
                {
                    channel_data[j] = data_buffer[j, i];
                }

                // Apply the filter
                double[] filtered_data = bandpass.Apply(channel_data);

                // Put the filtered data back into the buffer
                for (int j = 0; j < samples_returned; j++)
                {
                    data_buffer[j, i] = (float)filtered_data[j];
                }
            }

        }
        else
        {
            Debug.LogError("No consumers available or inlet is null.");
        }
    }
}