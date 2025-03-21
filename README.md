# Finding The Darkest Part Of The Sky Above You

### This tool is designed to help you image or visually observe the the night's sky in the darkest part of the sky above you, making your time under the stars more efficent! 

 It's a cross platform application which calculates the current position of the sun, moon, atmospheric extinction point, your gps location and altitude to return RA and DEC coordinates of the darkest area of the sky above you. This makes for more efficent imaging or visual observing. 

 It will soon be extended to incorporate a trajectory of this darkest spot in the sky as the sun and moon move throughout the night. It will also suggest targets around this dark sky reigon to image  / observe throughout the night and suggest a target plan. Hopefully light pollution will one day also be taken into consideration, but at the time of writing there's no simple web api to retrive sky quality data from (that I have found). 

## How to run
```dotnet run```
or to run on just windows for dev
```dotnet run -f net9.0-windows10.0.19041.0```

## How to build
```dotnet build```