# WcfPad

*A developer-friendly alternative to WcfTestClient*

### Installation

Download and run the .exe installer from the [releases page](https://github.com/21robin12/wcfpad/releases)

> You might receive a warning stating `Windows Defender SmartScreen prevented an unrecognized app from starting. Running this app might put your PC at risk.` - if so, select `More info` followed by `Run anyway`

### Usage

 - Enter a WCF endpoint address in the textbox and click Add
 - In the text editor, type `_endpoints.` to bring up autocomplete options
 - After selecting an endpoint and a method from the autocomplete options, WcfPad will automatically generate the request structure in the text editor
 - Modify request object as required and hit Run Script to invoke the WCF method

### Features

 - Endpoints are saved even when closing WcfPad
 - Write reusable scripts to call WCF methods
 - Use Javascript logic to build up data before calling WCF methods
 - Easily manage multiple services with renaming
 - Use autocomplete to browse available WCF methods
 
![](screenshot.png)
