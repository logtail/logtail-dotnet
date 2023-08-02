//This project uses new C# templates to generate top-level statements
// See https://aka.ms/new-console-template for more information

/**
 * This project showcases logging to Logtail
 */

using NLog;

// Configure NLog to color properties based on their type
NLog.Config.ConfigurationItemFactory.Default.ValueFormatter = new Logtail.NLog.ColorValueFormatter();

// Create logger for current class
var logger = LogManager.GetCurrentClassLogger();

//Following code showcases 6 NLog's default log levels
//Additionaly, it also show how to structure logs to add additional data

//Ttrace the code using the Trace() method
logger.Trace("Tracing the code!");

//Send debug messages using the Debug() method
logger.Debug("Debugging is hard, but can be easier with Logtail!");

//Send informative messages about application progress using the Info() method
//All of the properties that you pass to the log will be stored in a structured
//form in the context section of the logged event
logger.Info("User {user} - {userID} just ordered item {item}", "Josh", 95845, 75423);

//Report non-critical issues using the Warn() method
logger.Warn("Something is happening!");

//Send message about serious problems using the Error() method
logger.Error("Error occurred! And it's not good.");

//Report fatal errors that coused application to crash using the Fatal() method
logger.Fatal("Application crashed! Needs to be fixed ASAP!");

Console.WriteLine("All done! Now, you can check Logtail to see your logs");
