# Logtail - .NET Logging Made Easy
  
  [![Logtail python client](https://user-images.githubusercontent.com/19272921/154085622-59997d5a-3f91-4bc9-a815-3b8ead16d28d.jpeg)](https://betterstack.com/logtail)


[![ISC License](https://img.shields.io/badge/license-ISC-ff69b4.svg)](LICENSE.md)
[![Nuget version](https://badge.fury.io/nu/Logtail.svg)](https://www.nuget.org/packages/Logtail)

Collect logs directly from your .NET applications.

[Logtail](https://betterstack.com/logtail) is a hosted service that centralizes all of your logs into one place. Allowing for analysis, correlation and filtering with SQL. Actionable Grafana dashboards and collaboration come built-in. Logtail works with [any language or platform and any data source](https://docs.logtail.com/). 

### Features
- Simple integration. Built on well-known NLog logging library.
- Support for structured logging and events.
- Automatically captures useful context.
- Performant, light weight, with a thoughtful design.

### Supported language versions
- .NET 5 or newer (to use the example project .NET 6 is required)

# Installation

We currently support logging using **NLog** and **Serilog**.

You can find both installation guides in our [Better Stack .NET logging](https://betterstack.com/docs/logs/net-c/) docs.

---

# Example project

To help you get started with using Logtail in your .NET projects, we have prepared a simple program that showcases the usage of Logtail logger.

## Download and install the example project

You can download the [example project](https://github.com/logtail/logtail-dotnet/tree/main/example-project) from GitHub directly or you can clone it to a select directory.

## Run the example project using Visual Studio
 
Replace `<source-token>` with your actual source token in the `nlog.config` file. You can find your source token by going to logtail.com -> sources -> edit.

Open the `.csproj` file in the Visual Studio. Then click on the green play button `DotNetLogtail` or press **F5** to run the application.

You should see the following output:

```powershell
All done! Now, you can check Logtail to see your logs
```

## Run in the command line

Replace `<source-token>` with your actual source token in the `nlog.config` file. You can find your source token by going to logtail.com -> sources -> edit.

Open the command line in the projects directory and enter the following command:

```powershell
dotnet run
```

You should see the following output:

```powershell
All done! Now, you can check Logtail to see your logs
```

## Explore how example project works
 
Learn how to setup .NET logging by exploring the workings of the [example project](https://github.com/logtail/logtail-dotnet/tree/main/example-project) in detail. 

---

## Get in touch

Have any questions? Please explore the Logtail [documentation](https://docs.logtail.com/) or contact our [support](https://betterstack.com/help).
