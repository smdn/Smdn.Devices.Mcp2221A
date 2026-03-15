#!/bin/sh
tshark -r $1 -Tfields -e frame.number -e usb.src -e usb.dst -e usb.capdata -E header=y -Y 'usb.capdata != 0' | ./format-usbdata.csx
