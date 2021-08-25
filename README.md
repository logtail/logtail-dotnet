# Logtail .NET

.NET Standard 2.1 client for [Logtail.com](https://logtail.com).

```bash
dotnet add package Logtail
```

## Using with NLog

You can configure our NLog target in your `nlog.config`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog
    xmlns="<http://www.nlog-project.org/schemas/NLog.xsd>"
    xsi:schemaLocation="NLog NLog.xsd"
    xmlns:xsi="<http://www.w3.org/2001/XMLSchema-instance>" >

  <targets>
    <target xsi:type="Logtail" name="logtail" sourceToken="YOUR_LOGTAIL_SOURCE_TOKEN"/>
  </targets>

  <rules>
    <logger name="*" minlevel="Info" writeTo="logtail" />
  </rules>
</nlog>
```

... and then use it from your code:

```csharp
using System;
using NLog;

namespace YourApplication
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            logger.Info("Hello World");
        }
    }
}
```
