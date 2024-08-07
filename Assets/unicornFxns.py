import sys
import os
import numpy as np
import threading

# Add the directory containing UnicornPy.pyd to the Python path
unicorn_lib_path = r'C:\Users\SURGE_User\Documents\gtec\Unicorn Suite\Hybrid Black\Unicorn Python\Lib'
if unicorn_lib_path not in sys.path:
    sys.path.append(unicorn_lib_path)

import UnicornPy

class Unicorn:
    def __init__(self):
        self.device = None
        self.number_of_channels = None
        self.sampling_rate = UnicornPy.SamplingRate
        self.frame_length = 1
        self.acquiring = False
        self.data_thread = None

    # Function to connect to the Unicorn device.
    def connect_to_unicorn(self):
        # Get available devices.
        device_list = UnicornPy.GetAvailableDevices(True)

        if len(device_list) <= 0 or device_list is None:
            raise Exception("No device available. Please pair with a Unicorn first.")

        # Print the list of available devices.
        print("Available devices:")
        for i, device in enumerate(device_list):
            print(f"{i}: {device}")

        # Request device selection from the user.
        device_id = int(input("Enter the number of the device you want to connect to: "))
        if device_id < 0 or device_id >= len(device_list):
            raise IndexError('The selected device ID is not valid.')

        # Open selected device.
        self.device = UnicornPy.Unicorn(device_list[device_id])
        self.number_of_channels = self.device.GetNumberOfAcquiredChannels()
        print(f"Connected to device: {device_list[device_id]}")

    # Function to start data acquisition.
    def start_acquisition(self):
        if self.device is None:
            raise Exception("Device not connected. Please connect to a device first.")
        
        self.acquiring = True
        receive_buffer_length = self.frame_length * self.number_of_channels * 4  # 4 bytes per float
        receive_buffer = bytearray(receive_buffer_length)
        
        def acquire_data():
            try:
                self.device.StartAcquisition(False)  # False to not use test signals
                print("Data acquisition started.")
                
                while self.acquiring:
                    self.device.GetData(self.frame_length, receive_buffer, receive_buffer_length)
                    data = np.frombuffer(receive_buffer, dtype=np.float32)
                    print(data)
                    # Process the data here or append it to a larger buffer

            except UnicornPy.DeviceException as e:
                print(f"Device error during acquisition: {e}")
                self.acquiring = False
            except Exception as e:
                print(f"An unknown error occurred during acquisition: {e}")
                self.acquiring = False
            finally:
                self.device.StopAcquisition()
                print("Data acquisition stopped.")

        self.data_thread = threading.Thread(target=acquire_data)
        self.data_thread.start()

    # Function to stop data acquisition.
    def stop_acquisition(self):
        self.acquiring = False
        if self.data_thread is not None:
            self.data_thread.join()
        if self.device is not None:
            self.device.StopAcquisition()
            print("Data acquisition stopped.")

    # Function to close the device connection.
    def close_connection(self):
        if self.device is not None:
            del self.device
            print("Device connection closed.")
        else:
            print("No device connected.")


if __name__ == "__main__":
    unicorn = Unicorn()
    
    try:
        unicorn.connect_to_unicorn()
        
        # Start data acquisition
        unicorn.start_acquisition()

        # Wait for user input to stop acquisition
        input("Press Enter to stop data acquisition...\n")
        unicorn.stop_acquisition()

    except Exception as e:
        print(f"An error occurred: {e}")
    finally:
        unicorn.close_connection()