# OpcIotHub

[![.NET Core](https://github.com/rzeldent/opc-iot-hub/actions/workflows/main.yml/badge.svg)](https://github.com/rzeldent/opc-iot-hub/actions/workflows/main.yml)

## Description
This application uses the OPC Unified Architecture (OPC UA) to accept a list of nodes and subscribe to their value.
When a change is received, it pushes the new value to the an IOT hub. Here the data can be processed near realtime.
This ideal to monitor values from PLCs, like the Siemens SIMATIC S7-1500.
 
## Usage
This application can be used to publish industrial data to the cloud where it can be analyzed. Practical uses are:
- Anomily detection
- SCADA systems
- Historical performance and reporting
- Deep learning and optimization
- ...

## Implementation
There sources and sinks. These can enabled by DI in the file Program.cs.

Currently the following implementation of the interfaces are present:

Sources:
  - OPC-UA; Subscribes to OPC nodes and publishes the changed values,
  - Mock; Publishes a random value.

Sinks:
  - Azure IOTHub; Publishes to an IOT Hub,
  - MQTT; Uses a standard MQTT message bus,
  - Mock; Just logs the data.

## Setup

### Azure

Create an IOT hub in Azure. Then open a cloud console.


Add the device to the hub:
```PowerShell
PS> az iot hub device-identity create --hub-name <Hub name> --device-id <Device name>

```

Show the connection string to enter in the appSettings.json:
```PowerShell
PS> az iot hub device-identity connection-string show --hub-name <Hub name> --device-id <Device name> --output table
```

### MQTT

### Amazon AWS

## TODO

Feel free to participate.

- Get Amazon AWS working. Still some issues with special signing required for AWS.
- More sinks?

## Thanks

Many thanks to Converter Systems LLC (Cross River, NY), especially to [Andrew Cullen](https://github.com/awcullen) for making the OPC library available allowing projects like this!
