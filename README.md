# Firefox Auto Refresh
This small .NET console application will monitor a directory of your choice for changes, then refresh Firefox when it detects them.

![A screenshot of Firefox Auto Refresh in action](http://nathanalden.github.com/FirefoxAutoRefresh/screenshot.png "Screenshot")

## Prerequisites
* [Firefox](http://www.mozilla.org/en-US/firefox/new/)
* [MozRepl](https://addons.mozilla.org/en-us/firefox/addon/mozrepl/) add-on installed and enabled
* .NET Framework 4.0

## Instructions
1. Download a [ZIP file](https://github.com/NathanAlden/FirefoxAutoRefresh/blob/master/dist/FirefoxAutoRefresh.zip) containing application binaries and configuration
2. Extract the ZIP file anywhere you'd like
3. Change configuration using the FirefoxAutoRefresh.exe.config file
4. Run FirefoxAutoRefresh.exe

## Configuration

### Directory

The directory to monitor; a directory must be provided

### Filters

A comma-, semi-colon- or pipe-delimited list of wildcard filters; if no filters are specified, the application will monitor changes to all files

### RefreshDelayInMilliseconds

The number of milliseconds that must elapse after the last detected change before Firefox is refreshed; this setting keeps Firefox from rapidly refreshing when many changes are detected simultaneously

### Host

The host on which the Firefox Telnet server is listening

### Port

The port on which the Firefox Telnet server is listening