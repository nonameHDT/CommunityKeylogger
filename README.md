# CommunityKeylogger

An opensource keylogger

Author: nonameHDT

Facebook: https://www.facebook.com/hung.de.tien.175

Email: admin@hungdetien.com

Website: hungdetien.com

Release: 05/06/2016

Language: CSharp

# Features

* Auto detect VM. 
* Send email

# Something need to change

Open AppRun.cs and change some properties
* mailto (email address to receive log)
* mailfrom (email address to send from)
* mailfrompassword (password of mailfrom)
* logSize (when log is large than logSize * 512, the mail will be sent)
* SMTPAddress  (default is Gmail SMTP)
* SMTPPort

# Conditions to run

* No Antivirus
* Have D Partition
* Check Manufacturer (vmware, virtualbox,...)
* Check network MAC address range
* Check network status
* Check Total RAM (>= 1GB) and Processor count (even)
