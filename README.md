# OpcIotHub

## Description
This application uses the OPC Unified Architecture (OPC UA) to accept a list of nodes andsubscribe to their value.
When a change is received, it pushesthe new value to the an IOT hub. Here the data can be processed near realtime.
 
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

## Thanks

Many thanks to Converter Systems LLC (Cross River, NY), especially to [Andrew Cullen](https://github.com/awcullen) for making the OPC library available allowing projects like this!
