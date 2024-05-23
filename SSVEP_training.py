import numpy as np
from scipy.signal import butter, filtfilt, welch
from sklearn.svm import SVC
from sklearn.model_selection import train_test_split, cross_val_score
import joblib

import sys
sys.path.append("C:/Users/SURGE_User/Documents/gtec/Unicorn Suite/Hybrid Black/Unicorn Python/Lib")
import time
import shutil
import pylsl
import UnicornPy
import os

# Define your bandpass filter
def bandpass_filter(data, lowcut, highcut, fs, order=3):
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
    if data_buffer.shape[1] > 500:  # Ensuring enough data points
        filtered_data = bandpass_filter(data_buffer, 0.1, 30, fs)
        features = extract_features(filtered_data, fs, target_freqs)
        return features
    else:
        print("Data length is too short for filtering.")
        return None

# Initialize device
deviceList = UnicornPy.GetAvailableDevices(True)
device = UnicornPy.Unicorn(deviceList[0])
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
y_train = np.array([])  # Initialize as an empty array
X_train = np.array([])  # Initialize as an empty array

try:
    while True:
        new_data = collect_data(device, FrameLength, receivedBufferLength)
        print(new_data.shape)
        print(new_data)
        #check buffer updating
        print("Before updating buffer: ", data_buffer[:, -1])  # Print last column before update

        # Update buffer with new data
        data_buffer = np.roll(data_buffer, -FrameLength, axis=1)
        data_buffer[:, -FrameLength:] = new_data

        print("After updating buffer: ", data_buffer[:, -1])  # Print last column after update

        # Check if buffer is ready for processing
        if np.all(data_buffer[:, -1] != 0):  # This checks if the last column is not all zeros
            features = process_data(data_buffer, fs, target_freqs)
            if features is not None:
                print("Features extracted: ", features)
            else:
                print("No valid features extracted.")
        else:
            print("Buffer not yet filled.")

        if len(training_data) >= 100:  # Assume 100 samples needed before first training
            # Assuming features is a list of feature vectors returned by process_data
            features = [process_data(np.array(data), fs, target_freqs) for data in training_data]

            # Filter out None values
            features = [f for f in features if f is not None]

            # Convert to numpy array and reshape to 2D if necessary
            X_train = np.array(features)
            if len(X_train.shape) == 1:
                X_train = X_train.reshape(-1, 1)

            valid_features_labels = [(f, l) for f, l in zip(features, training_labels) if f is not None]
            if valid_features_labels:
                X_train, y_train = zip(*valid_features_labels)
                X_train = np.array(X_train)
                y_train = np.array(y_train)

            # print("X_train shape:", X_train.shape)
            # print("y_train shape:", y_train.shape)
            if X_train.size > 0 and y_train.size > 0:
                classifier.fit(X_train, y_train)
                print("Classifier trained!")
                # accuracy = classifier.score(X_train, y_train)  # Ensure X_test and y_test are also defined correctly
            else:
                print("No valid data available for training.")


            # print("Classifier trained!")
            # accuracy = classifier.score(X_train, y_train)
            # print(f"Validation Accuracy: {accuracy}")
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