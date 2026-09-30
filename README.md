# TeeVee Random Tool Box

A Windows desktop app that collects small utilities into one window, built with WPF on .NET 8.

## Tools
| Tool | What it does |
|---|---|
| **Date and SSN Formatter** | Opens an Excel file, reformats date columns to `MM-dd-yyyy`, pads the SSN column with leading zeros, and saves a new `Formatted_<filename>.xlsx` copy. The original file is not modified. |
| **Button Tool** | A placeholder page for testing new tools |

## Features
- Custom window chrome (minimize, maximize, close)
- Sidebar navigation between tool pages
- Excel processing runs in the background so the UI stays responsive

## Build and run
Requires Windows and the .NET 8 SDK.
```bash
dotnet run --project ThongKhongToolBox
```
Or open `ThongKhongToolBox.sln` in Visual Studio and press F5.

## Tech
C# · WPF · .NET 8 · [ClosedXML](https://github.com/ClosedXML/ClosedXML)
