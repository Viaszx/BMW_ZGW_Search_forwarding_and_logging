# BMW ZGW Search and encrypted logs
---
This repository contains a simple example program that demonstrates the following features:

- ZGW IP address lookup across all network connections
- Port forwarding
- Interacting with control units
- Using AES to encrypt all data received

The implementation provides a basic illustrative example of how to use the above functionality.

## Overview
**To get started with the vehicle you need to have a  [ENET][enet] cable.**  
> It is a simple twisted pair cable that is connected on one side to a network card and on the other side to the vehicle's OBD-II port.  

An ENET cable can also be used to access the electronic systems of BMW vehicles, which also enables:
- programming
- diagnostics
- setting of various parameters.  

> In order to connect to the car you need to know the **IP** of the **ZGW** unit.  

You can install **Ediabas** from the BWM standard tools.
> Ediabas is a collaborative environment.

Ediabas already includes a tool called `ZGW_SEARCH.exe` which is located at `x:\EDIABAS\Hardware\ENET`,  
but this program only send request to the `169.254.x.x` (Link-Local Address range).

# Program

This program, developed in the SharpDevelop environment, serves as an illustrative example showcasing various capabilities, including TCP/IP networking, multitasking, and asynchronous operations, as well as encryption using cryptography.

## Used Libraries

- **Networking Operations:** `System.Net;`, `System.Net.Sockets;` for TCP/IP interaction.
- **Multitasking and Asynchrony:** `System.Threading.Tasks;` for enhanced performance and responsiveness.
- **Encryption with Cryptography:** `System.Security.Cryptography;` for secure data encryption and decryption.

## Visual
![zgw_search_log_decrypte](https://github.com/Viaszx/Mazda-SkyActiv-EngineCoolantTemp/assets/78595419/58674369-8196-411d-90b1-a3bd2e792f0b)

[enet]: <http://bmwtools.info/uploads/enet_doku.pdf> "Пример Title"
