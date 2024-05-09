import numpy as np
from scipy.signal import butter, filtfilt, welch
from sklearn.svm import SVC
from sklearn.model_selection import train_test_split, cross_val_score
from sklearn.externals import joblib

import sys
sys.path.append("C:/Users/SURGE_User/Documents/gtec/Unicorn Suite/Hybrid Black/Unicorn Python/Lib")
import time
import shutil
import pylsl
import UnicornPy
import os

# Define your bandpass filter
def bandpass_filter(data, lowcut, highcut, fs, order=5):
    nyq = 0.5 * fs  # Nyquist Frequency
    low = lowcut / nyq
    high = highcut / nyq
    b, a = butter(order, [low, high], btype='band')
    y = filtfilt(b, a, data, axis=0)
    return y

# Feature extraction function
def extract_features(data, fs, target_freqs):
    freqs, psd = welch(data, fs, nperseg=fs*2, axis=0)
    target_indices = np.searchsorted(freqs, target_freqs)
    features = psd[target_indices]
    return features.mean(axis=1)  # Average features across time if needed

# Function definitions (bandpass_filter, extract_features)

def collect_data(device, FrameLength, receivedBufferLength):
    receivedBuffer = bytearray(receivedBufferLength)
    device.GetData(FrameLength, receivedBuffer, receivedBufferLength)
    new_data = np.frombuffer(receivedBuffer, dtype=np.float32).reshape((numberOfAcquiredChannels, FrameLength))
    return new_data

def process_data(data_buffer, fs, target_freqs):
    filtered_data = bandpass_filter(data_buffer, 0.1, 30, fs)
    features = extract_features(filtered_data, fs, target_freqs)
    return features

# Initialize device
deviceList = UnicornPy.GetAvailableDevices(True)
device = UnicornPy.Unicorn(deviceList[1])
device.StartAcquisition(False)
numberOfAcquiredChannels = device.GetNumberOfAcquiredChannels()
FrameLength = 4
receivedBufferLength = FrameLength * numberOfAcquiredChannels * 4

# Data collection settings
fs = 250  # Sampling rate
target_freqs = [9, 10, 12, 15]  # SSVEP frequencies, consider (8, 12, 15, 30) Hz
data_buffer = np.zeros((numberOfAcquiredChannels, fs * 2))  # Buffer for 2 seconds of data

# Classifier and data storage for training
classifier = SVC(kernel='linear', probability=True)
training_data = []
training_labels = []

try:
    while True:
        new_data = collect_data(device, FrameLength, receivedBufferLength)
        data_buffer = np.roll(data_buffer, -FrameLength, axis=1)
        data_buffer[:, -FrameLength:] = new_data

        if len(training_data) >= 100:  # Assume 100 samples needed before first training
            features = [process_data(np.array(data), fs, target_freqs) for data in training_data]
            X_train, X_test, y_train, y_test = train_test_split(features, training_labels, test_size=0.25, random_state=42)
            classifier.fit(X_train, y_train)
            print("Classifier trained!")
            accuracy = classifier.score(X_test, y_test)
            print(f"Validation Accuracy: {accuracy}")
            training_data = []  # Reset data after training
            training_labels = []

        # Simulate labeling (replace with actual labeling logic)
        if np.random.rand() > 0.5:
            training_data.append(data_buffer.copy())  # Store copy of buffer
            training_labels.append(np.random.randint(0, 4))  # Random label

        time.sleep(0.01)

except KeyboardInterrupt:
    print("Training stopped by user.")

# Save the trained classifier
joblib.dump(classifier, 'trained_classifier.pkl')
print("Classifier saved!")