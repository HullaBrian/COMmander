```
_________  ________      _____                             .___            
\_   ___ \ \_____  \    /     \   _____ _____    ____    __| _/___________ 
/    \  \/  /   |   \  /  \ /  \ /     \\__  \  /    \  / __ |/ __ \_  __ \
\     \____/    |    \/    Y    \  Y Y  \/ __ \|   |  \/ /_/ \  ___/|  | \/
 \______  /\_______  /\____|__  /__|_|  (____  /___|  /\____ |\___  >__|   
        \/         \/         \/      \/     \/     \/      \/    \/        
```

# COMmander
COMmander is a proof of concept tool that can enrich defensive telemetry around RPC and COM.

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
resources while still providing the detection functionality.

![image](https://github.com/user-attachments/assets/916bb7e0-70d5-41a9-98e1-c87e3a680593)

# Build Instructions
Open in Visual Studio and press the build button.

# Usage
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
