# Example project

To help you get started with using Logtail in your .NET projects, we have prepared a simple program that showcases the usage of Logtail logger.

## Download and install the example project

You can download the example project from GitHub directly or you can clone it to a select directory.

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

# Setup

This part shows and explains the usage of the Logtail package for .NET as shown in the example application

## Create NLog config

In the root directory of the project create the `nlog.config` file or copy the file from the example project. In Visual Studio, you can press **Ctrl + Shift + A** and enter the file name. This file is used to configure NLog using XML syntax. The content of the file should look like this:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
        autoReload="true"
        internalLogLevel="Warn"
        internalLogFile="internal.txt">
	<extensions>
		<add assembly="Logtail" />
	</extensions>

	<targets>
		<!-- Dont forget to change SOURCE_TOKEN to your actual source token-->
		<target xsi:type="Logtail" name="logtail" layout="${message}" sourceToken="SOURCE_TOKEN" />
	</targets>

	<rules>
		<logger name="*" minlevel="Trace" writeTo="logtail" />
	</rules>
</nlog>
```

Make sure that you replace `SOURCE_TOKEN` with the actual source token that you can find in the Source settings.

Also, **make sure that the** `nlog.config` **file is set to be copied to the output directory** when running the application. 

If you are using Visual Studio, you can set this option by right-clicking on the file and selecting **Properties.** Find the **Copy to Output Directory** option and set it to **Copy Always.**

Another way is to open the `.csproj` file and add the following directive:

```xml
<ItemGroup>
    <None Update="nlog.config">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
</ItemGroup>
```

## Create logger

First, include the `NLog` library upon which the Logtail package was built. Then create a `Logger` instance which will be later used for sending log messages. To create a `Logger` instance, call `LogManager.GetCurrentClassLogger()` constructor. 

```csharp
using NLog;

// Create logger for current class
var logger = LogManager.GetCurrentClassLogger();
```

This will create a logger for the current class. In this case, it will be created for the `Program` class and it will add `“logger_string”` with the value `“Program”` to the context of the JSON log message.

### Colored property values

If you'd like your logged properties to be colored by their type, include following configuration when you create a logger:

```csharp
// Configure NLog to color properties based on their type
NLog.Config.ConfigurationItemFactory.Default.ValueFormatter = new Logtail.NLog.ColorValueFormatter();
```

### Filter logs

The name of the logger will also be present in the log message which will look something like this:

```json
"2022-01-26 10:25:06.0980|DEBUG|Program|Debugging is hard, but can be easier with Logtai!"
```

This provides an option to filter logs based on the logger that sends them. You can create a logger for each of the logical components of your application and then filter the logs based on the names of the components. 

For example, if you create a logger as a field of the `ShoppingCart` class, the value of `logger_string` will be `ShoppingCart` :

```csharp
public class ShoppingCart
{
     private static Logger ShoppingCartLogger = LogManager.GetCurrentClassLogger();
     //...
}
```

The output will look similar to this:

```json
{
   "dt":"2022-01-26 10:48:10.635 UTC",
   "context":{
      "logger_string":"ShoppingCart",
      "runtime":{
         "class_string":"ShoppingCart",
         "file_string":"C:\\Users\\someuser\\source\\repos\\DotNetLogtail\\DotNetLogtail\\ShoppingCart.cs",
         "line_integer":"16",
         "member_string":".ctor"
      }
   },
   "level_string":"Error",
   "message_string":"2022-01-26 11:48:10.6354|ERROR|DotNetLogtail.ShoppingCart|Error !!!!!"
}
```

Then it is possible to filter the logs using the following search formula:

```json
context.logger_string="ShoppingCart"
```

This will only show logs that were sent from to `ShoppingCart` logger.

# Logging

The `Logger` instance we created in the setup is used to send log messages to Logtail. It provides 6 logging methods for the 6 default log levels. The log levels and their method are:

- **TRACE** - Trace the code using the `Trace()` method
- **DEBUG** - Send debug messages using the `Debug()` method
- **INFO** - Send informative messages about the application progress using the `Info()` method
- **WARN** - Report non-critical issues using the `Warn()` method
- **ERROR** - Send messages about serious problems using the `Error()` method
- **FATAL** - Report fatal errors that caused the application to crash using the `Fatal()` method

## Logging example

To send a log message of select log level, use the corresponding method. In this example, we will send the **DEBUG** level log and **ERROR** level log.

```csharp
//Send debug messages using the Debug() method
logger.Debug("Debugging is hard, but can be easier with Logtai!");

//Send message about serious problems using the Error() method
logger.Error("Error occurred! And it's not good.");
```

This will create the following JSON output:

```json
{
   "dt":"2022-01-26 09:25:06.098 UTC",
   "context":{
      "logger_string":"Program",
      "runtime":{
         "class_string":"Program",
         "file_string":"C:\\Users\\someuser\\source\\repos\\DotNetLogtail\\DotNetLogtail\\Program.cs",
         "line_integer":"21",
         "member_string":"<Main>$"
      }
   },
   "level_string":"Debug",
   "message_string":"2022-01-26 10:25:06.0980|DEBUG|Program|Debugging is hard, but can be easier with Logtai!"
}

{
   "dt":"2022-01-26 09:25:06.098 UTC",
   "context":{
      "logger_string":"Program",
      "runtime":{
         "class_string":"Program",
         "file_string":"C:\\Users\\someuser\\source\\repos\\DotNetLogtail\\DotNetLogtail\\Program.cs",
         "line_integer":"32",
         "member_string":"<Main>$"
      }
   },
   "level_string":"Error",
   "message_string":"2022-01-26 10:25:06.0980|ERROR|Program|Error occurred! And it's not good."
}
```

## Additional configuration

The Logtail target will send you logs periodically in batches to optimize network traffic with several retries in case of unexpected HTTP errors. You can adjust this behavior by setting the `maxBatchSize`, `flushPeriodMilliseconds`, and `retries` parameters to your custom values in your config.

```xml
<target
   xsi:type="Logtail"
   name="logtail"
   layout="${message}"
   sourceToken="YOUR_SOURCE_TOKEN"
   maxBatchSize="200"
   flushPeriodMilliseconds="1000"
   retries="3" />
```

## Structuring the logs

All of the properties that you pass to the log will be stored in a structured form in the `context` section of the logged event.

```csharp
logger.Info("User {user} - {userID} just ordered item {item}", "Josh", 95845, 75423);
```

Code above will create the following output:

```json
{
   "dt":"2022-01-26 09:55:34.128 UTC",
   "context":{
      "logger_string":"Program",
      "runtime":{
         "class_string":"Program",
         "file_string":"D:\\dotnet_logtail\\Program.cs",
         "line_integer":"25",
         "member_string":"<Main>$"
      },
      "properties":{
         "item_integer":"75423",
         "userID_integer":"95845",
         "user_string":"Josh"
      }
   },
   "level_string":"Info",
   "message_string":"2022-01-26 10:55:34.1285|INFO|Program|User \"Josh\" - 95845 just ordered item 75423"
}
```

A new field called `properties` is added into the `context` and it contains the arguments that were passed and their values.
