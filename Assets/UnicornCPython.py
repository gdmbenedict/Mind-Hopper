import sys
import os
import json
import numpy as np
import threading
import time
import joblib
from scipy.signal import butter, lfilter
from sklearn.ensemble import RandomForestClassifier
from sklearn.model_selection import train_test_split
from sklearn.metrics import accuracy_score

# Add the directory containing UnicornPy.pyd to the Python path
unicorn_lib_path = r'C:\Users\SURGE_User\Documents\gtec\Unicorn Suite\Hybrid Black\Unicorn Python\Lib'
if unicorn_lib_path not in sys.path:
    sys.path.append(unicorn_lib_path)

import UnicornPy

class Unicorn:
    def __init__(self):
        self.device = None
        self.device_serial = None
        self.number_of_channels = None
        self.sampling_rate = UnicornPy.SamplingRate
        self.frame_length = 1
        self.acquiring = False
        self.data_thread = None
        self.data = []
        self.labels = []
        self.acquisition_duration = 60  # Duration in seconds for data acquisition

    def list_devices(self):
        device_list = UnicornPy.GetAvailableDevices(True)
        return json.dumps(device_list)

    def connect_to_unicorn(self):
        device_list = UnicornPy.GetAvailableDevices(True)
        if len(device_list) <= 0 or device_list is None:
            return "No device available. Please pair with a Unicorn first."
        self.device_serial = "UN-2019.05.07" # try to get dropdown menu working for this input
        self.device = UnicornPy.Unicorn(self.device_serial)
        self.number_of_channels = self.device.GetNumberOfAcquiredChannels()
        self.save_state()
        return f"Connected to device: {self.device_serial}"

    def start_acquisition(self):
        self.load_state()
        if self.device is None:
            raise Exception("Device not connected. Please connect to a device first.")
        
        self.acquiring = True
        receive_buffer_length = self.frame_length * self.number_of_channels * 4  # 4 bytes per float
        receive_buffer = bytearray(receive_buffer_length)
        
        def bandpass_filter(data, lowcut, highcut, fs, order=4):
            nyquist = 0.5 * fs
            low = lowcut / nyquist
            high = highcut / nyquist
            b, a = butter(order, [low, high], btype='band')
            return lfilter(b, a, data)

        def preprocess_data(raw_data):
            # Bandpass filter around the four frequencies
            filtered_data = []
            for freq in [9, 10, 12, 15]:
                filtered = bandpass_filter(raw_data, freq-1, freq+1, self.sampling_rate)
                filtered_data.append(filtered)
            return np.array(filtered_data).flatten()

        def acquire_data():
            start_time = time.time()
            try:
                self.device.StartAcquisition(False)  # False to not use test signals
                while self.acquiring and (time.time() - start_time) < self.acquisition_duration:
                    self.device.GetData(self.frame_length, receive_buffer, receive_buffer_length)
                    raw_data = np.frombuffer(receive_buffer, dtype=np.float32)
                    processed_data = preprocess_data(raw_data)
                    self.data.append(processed_data)
                    # Labels should be provided or determined by your experimental setup
                    # For this example, let's assume a random label
                    self.labels.append(np.random.choice(['R', 'L', 'D', 'U']))  # Replace with actual label logic

            except UnicornPy.DeviceException as e:
                print(f"Device error: {e}")
                self.acquiring = False
            except Exception as e:
                print(f"An error occurred: {e}")
                self.acquiring = False
            finally:
                self.device.StopAcquisition()
                print("Data acquisition stopped in thread")
                self.train_and_save_model()

        self.data_thread = threading.Thread(target=acquire_data)
        self.data_thread.start()
        return "Data acquisition started."

    def stop_acquisition(self):
        self.acquiring = False
        if self.data_thread is not None:
            self.data_thread.join()
        return "Data acquisition stopped."

    def train_and_save_model(self):
        X = np.array(self.data)
        y = np.array(self.labels)

        X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
        clf = RandomForestClassifier(n_estimators=100, random_state=42)
        clf.fit(X_train, y_train)
        y_pred = clf.predict(X_test)
        accuracy = accuracy_score(y_test, y_pred)
        print(f"Model accuracy: {accuracy}")

        joblib.dump(clf, 'ssvep_classifier.pkl')
        print("Model saved to ssvep_classifier.pkl")
        self.acquiring = False  # Ensure the acquiring flag is reset

    def close_connection(self):
        self.load_state()
        if self.device is not None:
            del self.device
        self.clear_state()
        return "Device connection closed."

    def save_state(self):
        state = {
            'device_serial': self.device_serial if self.device else None
        }
        with open('unicorn_state.json', 'w') as f:
            json.dump(state, f)

    def load_state(self):
        if os.path.exists('unicorn_state.json'):
            with open('unicorn_state.json', 'r') as f:
                state = json.load(f)
                if state['device_serial'] is not None:
                    device_list = UnicornPy.GetAvailableDevices(True)
                    if state['device_serial'] in device_list:
                        self.device = UnicornPy.Unicorn(state['device_serial'])
                        self.number_of_channels = self.device.GetNumberOfAcquiredChannels()
                        self.device_serial = state['device_serial']

    def clear_state(self):
        if os.path.exists('unicorn_state.json'):
            os.remove('unicorn_state.json')

def main():
    unicorn = Unicorn()
    if len(sys.argv) < 2:
        print("No command provided.")
        return

    command = sys.argv[1]
    if command == "connect":
        result = unicorn.connect_to_unicorn()
    elif command == "start":
        result = unicorn.start_acquisition()
    elif command == "stop":
        result = unicorn.stop_acquisition()
    elif command == "close":
        result = unicorn.close_connection()
    else:
        result = "Unknown command."

    print(result)

if __name__ == "__main__":
    main()
