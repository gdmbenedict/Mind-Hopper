import sys
import os
import numpy

class Unicorn:
    def __init__(self):
        self.device  = None

    def connect_to_unicorn(self):
        return "Connected to Unicorn"

    def start_acquisition(self):
        return "Data acquisition started"

    def stop_acquisition(self):
        return "Data acquisition stopped"

    def close_connection(self):
        return "Device connection closed"
