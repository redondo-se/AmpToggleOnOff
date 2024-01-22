# AmpToggleOnOff
Windows app to monitor audio device output levels and trigger a USB relay when sound is being output.

# More Info

This app was created to trigger a [MA1240a Multi-Zone 12 Channel Amplifier](https://www.daytonaudio.com/product/1017/ma1240a-multi-zone-12-channel-amplifier) to turn on and off when music is playing from soundcards on the local computer. The amplifier has built in functionality to turn itself on and off based on input signals, but that functionality had failed and the amp would always remain powered on. It also has a mode that allows the power to be turned on when a 12v trigger signal is input.

The windows PC is running multiple instances of [squeezelite](https://sourceforge.net/projects/lmsclients/files/squeezelite/windows/) as clients to [Logitech Media Server](https://www.mysqueezebox.com/download) each outputting to separate audio devices on the local machine which are connected to the multi-zone amp. This app monitors the output level on each of the audio devices and triggers a [KNACRO 5V 2 Channel Relay](https://www.amazon.com/KNACRO-SRD-05VDC-SL-C-control-intelligent-2-Channel/dp/B07CNVNF91/ref=cm_cr_arp_d_product_top?ie=UTF8&th=1) which in turn triggers the multi-zone amp to turn on. When music stops playing the USB relay is turned off which triggers the multi-zone amp to turn off.

## Requirements

 - A windows machine with .Net 4.5.1 installed
 - One or more audio outputs (sound cards)
 - A KNACRO USB Relay
 - An appropriate power source for the contact side of the relay

## Installation/Usage

 - Connect the KNACRO USB Relay to the windows machine
 - Extract the contents of the archive to a directory of your choice
 - Run the application from a command line with -l option to list the audio devices:
 
	 AmpToggleOnOff.exe -l
 
 
 - Update DeviceList.txt with the audio devices you want to monitor
 - Use [usb-relay-hid](https://github.com/pavel-a/usb-relay-hid) to find the serial number of your USB relay
 - Run the app to begin monitoring audio levels and trigger the relay:

    AmpToggleOnOff.exe &lt;relaySerial&gt; &lt;relayIndex&gt; &lt;secondsQuietBeforeOff&gt;

 - The app can be run with windows task scheduler or as a service with something like [NSSM](https://nssm.cc/)
