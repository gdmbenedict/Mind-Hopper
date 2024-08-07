import sys
import os
import json
import numpy as np
import threading
import time
import joblib
from scipy.signal import butter, lfilter

# Add the directory containing UnicornPy.pyd to the Python path
unicorn_lib_path = r'C:\Users\SURGE_User\Documents\gtec\Unicorn Suite\Hybrid Black\Unicorn Python\Lib'
if unicorn_lib_path not in sys.path:
    sys.path.append(unicorn_lib_path)

import UnicornPy

class UnicornTest:
    def __init__(self):
        self.device = None
        self.device_serial = None
        self.number_of_channels = None
        self.sampling_rate = UnicornPy.SamplingRate
        self.frame_length = 1
        self.acquiring = False
        self.data_thread = None
        self.classifier = None

    def list_devices(self):
        device_list = UnicornPy.GetAvailableDevices(True)
        print(json.dumps(device_list))
        sys.stdout.flush()  # Ensure the output is flushed to the console

    def connect_to_unicorn(self):
        device_list = UnicornPy.GetAvailableDevices(True)
        if len(device_list) <= 0 or device_list is None:
            return "No device available. Please pair with a Unicorn first."
        self.device_serial = "UN-2019.05.07"  # try to get dropdown menu working for this input
        self.device = UnicornPy.Unicorn(self.device_serial)
        self.number_of_channels = self.device.GetNumberOfAcquiredChannels()
        self.save_state()
        return f"Connected to device: {self.device_serial}"

    def load_classifier(self, model_path):
        self.load_state()
        print(f"Loading classifier from: {model_path}")
        if not os.path.exists(model_path):
            raise FileNotFoundError(f"Model file not found: {model_path}")
        
        try:
            self.classifier = joblib.load(model_path)
            print("Classifier loaded successfully.")
        except Exception as e:
            print(f"Error loading classifier: {e}")
            raise e
        self.save_state()

    def start_acquisition(self):
        self.load_state()
        print(f"Device: {self.device}")
        print(f"Classifier: {self.classifier}")

        if self.device is None:
            raise Exception("Device not connected. Please connect to a device first.")
        
        if self.classifier is None:
            raise Exception("Classifier not loaded. Please load a classifier first.")

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

        def acquire_and_analyze_data():
            try:
                self.device.StartAcquisition(False)  # False to not use test signals
                while self.acquiring:
                    self.device.GetData(self.frame_length, receive_buffer, receive_buffer_length)
                    raw_data = np.frombuffer(receive_buffer, dtype=np.float32)
                    processed_data = preprocess_data(raw_data).reshape(1, -1)
                    label = self.classifier.predict(processed_data)
                    print(f"Predicted label: {label[0]}")
                    self.save_recent_label(label[0])
                    sys.stdout.flush()  # Ensure the output is flushed to the console

            except UnicornPy.DeviceException as e:
                print(f"Device error: {e}")
                self.acquiring = False
            except Exception as e:
                print(f"An error occurred: {e}")
                self.acquiring = False
            finally:
                self.device.StopAcquisition()
                print("Data acquisition stopped in thread")

        self.data_thread = threading.Thread(target=acquire_and_analyze_data)
        self.data_thread.start()
        return "Data acquisition started."

    def save_recent_label(self, label):
        if os.path.exists('unicorn_state.json'):
            with open('unicorn_state.json', 'r') as f:
                state = json.load(f)
        else:
            state = {}
        state['recent_label'] = label
        with open('unicorn_state.json', 'w') as f:
            json.dump(state, f)

    def stop_acquisition(self):
        self.acquiring = False
        if self.data_thread is not None:
            self.data_thread.join()
        return "Data acquisition stopped."

    def close_connection(self):
        self.load_state()
        if self.device is not None:
            del self.device
        self.clear_state()
        return "Device connection closed."

    def save_state(self):
        state = {
            'device_serial': self.device_serial if self.device else None,
            'classifier': 'ssvep_classifier.pkl' if self.classifier else None
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
                if state['classifier'] is not None:
                    model_path = state['classifier']
                    if os.path.exists(model_path):
                        self.classifier = joblib.load(model_path)
                        print("Classifier loaded from saved state.")
                    else:
                        print(f"Classifier file not found: {model_path}")

    def clear_state(self):
        if os.path.exists('unicorn_state.json'):
            os.remove('unicorn_state.json')

def main():
    unicorn = UnicornTest()
    if len(sys.argv) < 2:
        print("No command provided.")
        return

    command = sys.argv[1]
    result = None

    try:
        if command == "list_devices":
            unicorn.list_devices()
        elif command == "connect":
            result = unicorn.connect_to_unicorn()
        elif command == "load_classifier":
            if len(sys.argv) < 3:
                print("No model path provided.")
                return
            result = unicorn.load_classifier(sys.argv[2])
        elif command == "start":
            result = unicorn.start_acquisition()
        elif command == "stop":
            result = unicorn.stop_acquisition()
        elif command == "close":
            result = unicorn.close_connection()
        else:
            result = "Unknown command."
    except Exception as e:
        result = f"Error executing command '{command}': {e}"

    if result is not None:
        print(result)

if __name__ == "__main__":
    main()
