import sys
sys.path.append("C:/Users/SURGE_User/Documents/gtec/Unicorn Suite/Hybrid Black/Unicorn Python/Lib")

import UnicornPy
import os
import numpy as np
import time
import shutil
import pylsl

# open device for unicorn BCI
deviceList = UnicornPy.GetAvailableDevices(True)
device = UnicornPy.Unicorn(deviceList[1])

# start test signal and signal acquisition 
TestSignalEnabled = False
device.StartAcquisition(TestSignalEnabled)
numberOfAcquiredChannels = device.GetNumberOfAcquiredChannels()

# create buffer for for unicorn BCI
FrameLength = 4
recievedBufferLength = FrameLength * numberOfAcquiredChannels * 4
recievedBuffer = bytearray(recievedBufferLength)

# create an lsl stream outlet for unicorn BCI
info = pylsl.StreamInfo('LSLExampleAmp', 'EEG', numberOfAcquiredChannels, 100, 'float32', 'myuid34234')
outlet = pylsl.StreamOutlet(info)

# create buffer for lsl stream
lslBuffer = np.zeros((8, 1))

while True:
    # Get data from Unicorn device
    device.GetData(FrameLength, recievedBuffer, recievedBufferLength)
    
    # Convert received buffer to numpy array
    data = np.frombuffer(recievedBuffer, dtype=np.float32).reshape((numberOfAcquiredChannels, FrameLength))
    
    # Send data to LSL outlet
    for i in range(FrameLength):
        outlet.push_sample(data[:, i])
    
    # Sleep for a short time to prevent CPU overuse
    time.sleep(0.01)