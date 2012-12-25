// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogManager.cs">
//   Copyright (C) 2012 by Alexander Davyduk. All rights reserved.
// </copyright>
// <summary>
//   Defines the LogManager type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.SharedSource.RedirectManager.Utils
{
  using Sitecore.Diagnostics;

  /// <summary>
  /// LogManager class
  /// </summary>
  public class LogManager
  {
    /// <summary>
    /// Writes the info message to the log.
    /// </summary>
    /// <param name="message">The message.</param>
    public static void WriteInfo(string message)
    {
      if (Configuration.EnableLogging)
      {
        Log.Info(string.Format("RedirectManager: {0}", message), "RedirectManager");
      }
    }

    /// <summary>
    /// Writes the error message to the log.
    /// </summary>
    /// <param name="message">The message.</param>
    public static void WriteError(string message)
    {
      if (Configuration.EnableLogging)
      {
        Log.Error(string.Format("RedirectManager: {0}", message), "RedirectManager");
      }
    }
  }
}