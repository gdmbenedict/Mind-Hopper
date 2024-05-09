import numpy as np
from scipy.signal import butter, filtfilt
from sklearn.svm import SVC
from sklearn.model_selection import train_test_split
from sklearn.metrics import accuracy_score
from scipy.signal import welch
import joblib

import sys
sys.path.append("C:/Users/SURGE_User/Documents/gtec/Unicorn Suite/Hybrid Black/Unicorn Python/Lib")
import time
import shutil
import pylsl
import UnicornPy
import os


def bandpass_filter(data, lowcut, highcut, fs, order=5):
    nyq = 0.5 * fs  # Nyquist Frequency
    low = lowcut / nyq
    high = highcut / nyq
    b, a = butter(order, [low, high], btype='band')
    y = filtfilt(b, a, data)
    return y


def extract_features(data, fs, target_freqs):
    """
    Extracts power spectral density features at specific target frequencies.

    Args:
    data (numpy array): The EEG signal data.
    fs (int): Sampling frequency of the EEG data.
    target_freqs (list of float): The frequencies of interest for SSVEP.

    Returns:
    numpy array: The power spectral density values at the target frequencies.
    """
    # Calculate power spectral density
    freqs, psd = welch(data, fs, nperseg=fs*2, axis=0)  # Using a window length of 2 seconds

    # Find the closest indices of freqs to the target frequencies
    target_indices = np.searchsorted(freqs, target_freqs)
    # Extract the PSD values at these indices
    features = psd[target_indices]
    
    return features.mean(axis=1)

# Constants and settings
fs = 256  # Sampling rate in Hz
target_freqs = [9, 10, 12, 15]  # SSVEP frequencies

# Open Unicorn device
deviceList = UnicornPy.GetAvailableDevices(True)
device = UnicornPy.Unicorn(deviceList[1])
device.StartAcquisition(False)
numberOfAcquiredChannels = device.GetNumberOfAcquiredChannels()
FrameLength = 4
receivedBufferLength = FrameLength * numberOfAcquiredChannels * 4
receivedBuffer = bytearray(receivedBufferLength)

# Load the classifier
classifier = joblib.load('ssvep_classifier.pkl')

# Set up LSL
info = pylsl.StreamInfo('UnicornBCI', 'EEG', numberOfAcquiredChannels, fs, 'float32', 'myuid34234')
outlet = pylsl.StreamOutlet(info)

# Set up classifier (This should be trained beforehand)
classifier = SVC(kernel='linear')  # Assuming it's already trained

# Define your bandpass filter and feature extraction functions as before
def bandpass_filter(data, lowcut, highcut, fs, order=5):
    nyq = 0.5 * fs
    low = lowcut / nyq
    high = highcut / nyq
    b, a = butter(order, [low, high], btype='band')
    y = filtfilt(b, a, data, axis=0)
    return y

def extract_features(data, fs, target_freqs):
    freqs, psd = welch(data, fs, nperseg=fs*2, axis=0)
    target_indices = np.searchsorted(freqs, target_freqs)
    features = psd[target_indices]
    return features.mean(axis=1)

# [Initialize Unicorn and LSL as before...]

data_buffer = np.zeros((numberOfAcquiredChannels, fs * 2))  # Buffer for 2 seconds of data
while True:
    device.GetData(FrameLength, receivedBuffer, receivedBufferLength)
    new_data = np.frombuffer(receivedBuffer, dtype=np.float32).reshape((numberOfAcquiredChannels, FrameLength))
    data_buffer = np.roll(data_buffer, -FrameLength, axis=1)
    data_buffer[:, -FrameLength:] = new_data
    
    if np.any(data_buffer):  # Make sure the buffer is not empty
        filtered_data = bandpass_filter(data_buffer, 0.1, 30, fs)
        features = extract_features(filtered_data, fs, target_freqs)
        
        # Predict using the classifier
        prediction = classifier.predict([features])
        print("Predicted Class:", prediction)
    
    # Sleep to reduce CPU load
    time.sleep(0.01)

# press ctrl+c to stop the script








