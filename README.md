```
_________  ________      _____                             .___            
\_   ___ \ \_____  \    /     \   _____ _____    ____    __| _/___________ 
/    \  \/  /   |   \  /  \ /  \ /     \\__  \  /    \  / __ |/ __ \_  __ \
\     \____/    |    \/    Y    \  Y Y  \/ __ \|   |  \/ /_/ \  ___/|  | \/
 \______  /\_______  /\____|__  /__|_|  (____  /___|  /\____ |\___  >__|   
        \/         \/         \/      \/     \/     \/      \/    \/        
```

# COMmander
COMmander is a tool written in C# that can enrich defensive telemetry around RPC and COM.
For a detailed blog post on the development of the tool and ruleset, see
[Jacob Acuna's](https://www.linkedin.com/in/jacob-acuna1)
[blog post](https://jacobacuna.me/2025-06-12-COMmander/)

COMmander leverages the `Microsoft-Windows-RPC` ETW provider to tap into low level RPC events.
This provides detailed RPC-related events on the system that can provide defenders
with details about RPC, as well as layers of abstraction built on top of it, such as COM.

The way COMmander works is very simple - you provide a configuration file containing detection
rules. These rules consist of various filters that provide granular control over what COMmander will
detect (See the configuration file section for more details). After running the binary, that's
all you have to do - COMmander will monitor the system for events that match the filters you provided
and send alerts in the terminal if any are encountered.

One of the issues with handling so many events is that it often requires a significant amount
of resources to run. However, COMmander is extremely lightweight, and consistently uses minimal
resources while still providing detection functionality.

![image](https://github.com/user-attachments/assets/916bb7e0-70d5-41a9-98e1-c87e3a680593)

> [!WARNING]  
> Running the CLI application and service binary at the same time will break the service. Restart
> the service if this occurs.

# Build Instructions
Open in Visual Studio and press the build button.

# Service Usage
## Installation
1. Download the latest release from this repository.
2. Run the `InstallService.ps1` powershell script as an **administrator**
	- Service files are stored in `C:\Program Files\COMmander`
 	- The service is called `COMmander` and runs as the local system account
3. Run `Start-Service COMmander` if it isn't started already

> [!NOTE]  
> During the installation process, the script may open a window asking for the credentials
> for the local system account. If this occurs, simply press enter.

## Uninstall
To uninstall COMmander, run the `UninstallService.ps1` script included in the releases
of this repository as an **administrator**.

## Event Viewer
COMmander's service will attempt to log events in the Windows Event Viewer, under the name
`COMmander` beneath the `Application and Service Logs` section.

![image](https://github.com/user-attachments/assets/9495e225-e1d4-49a5-9844-963e4dd2ccc0)

Below are the event IDs that you will see in the event viewer.

| Event ID | Event Type  | Description                       |
|----------|-------------|-----------------------------------|
| 1        | Information | The service is starting           |
| 2        | Information | The service is stopping           |
| 3        | Information | A rule was loaded                 |
| 4        | Error       | There was an error during runtime |
| 5        | Warning     | A detection was triggered         |

![image](https://github.com/user-attachments/assets/2ba136c4-e3a8-4a8f-9fd1-c8f4ba816183)

# Command-Line Usage
```shell
COMmander.exe
```
![COMmander-Preview(1)](https://github.com/user-attachments/assets/c61dcb4b-3447-42c6-9bf9-342c3e3a1349)

# Configuration File
By default, COMmander will attempt to find a configuration file called `config.xml` in the
same directory as the binary. Rules may include a combination of rule types, but for the time
being may only include a single instance of a rule type (ie you can only have one `OpNum`
filter). Below is a table containing the possible values to use in the detection rules.

| Rule Type      | Example Value                          |
|----------------|----------------------------------------|
| InterfaceUUID  | `06bba54a-be05-49f9-b0a0-30f790261023` |
| OpNum          | `13`                                   |
| Endpoint       | `\PIPE\DAV RPC SERVICE`                |
| NetworkAddress | `NULL`                                 |
| ProcessName    | `lsass`                                |

Below is a sample configuration file template:
```xml
<Rules>
	<Rule name="DCOM Invoked WebClient">
		<InterfaceUUID>c8cb7687-e6d3-11d2-a958-00c04f682e16</InterfaceUUID>
		<Endpoint>\PIPE\DAV RPC SERVICE</Endpoint>
	</Rule>
	<Rule name="Authentication Coercion using PetitPotam EfsRpcOpenFileRaw">
		<InterfaceUUID>c681d488-d850-11d0-8c52-00c04fd90f7e</InterfaceUUID>
		<OpNum>0</OpNum>
	</Rule>
</Rules>
```
