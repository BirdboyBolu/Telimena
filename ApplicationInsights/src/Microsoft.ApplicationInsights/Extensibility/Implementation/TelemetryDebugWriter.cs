﻿// <copyright file="TelemetryDebugWriter.cs" company="Microsoft">
// Copyright © Microsoft. All Rights Reserved.
// </copyright>

#define DEBUG

using System.Security;

namespace Microsoft.ApplicationInsights.Extensibility.Implementation
{
    using System;
    using System.Diagnostics;
    using Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Platform;

    /// <summary>
    /// Writes telemetry items to debug output.
    /// </summary>
    public class TelemetryDebugWriter : IDebugOutput
    {
        /// <summary>
        /// Gets or sets a value indicating whether writing telemetry items to debug output is enabled.
        /// </summary>
        public static bool IsTracingDisabled { get; set; }

        /// <summary>
        /// Write the specified <see cref="ITelemetry"/> item to debug output.
        /// </summary>
        /// <param name="telemetry">Item to write.</param>
        /// <param name="filteredBy">If specified, indicates the telemetry item was filtered out and not sent to the API.</param>
        public static void WriteTelemetry(ITelemetry telemetry, string filteredBy = null)
        {
            var output = PlatformSingleton.Current.GetDebugOutput();
            if (output.IsAttached() && output.IsLogging())
            {
                string prefix = "***TELIMENA*** (AppInsights) Telemetry";
                if (!string.IsNullOrEmpty(telemetry.Context.InstrumentationKey))
                {
                    prefix = "***TELIMENA*** (AppInsights) Telemetry (with AppInsights)";
                }

                if (!string.IsNullOrEmpty(filteredBy))
                {
                    prefix += " (filtered by " + filteredBy + ")";
                }

                prefix += ": ";

                string serializedTelemetry = JsonSerializer.SerializeAsString(telemetry);
                output.WriteLine(prefix + serializedTelemetry);
            }
        }

        /// <summary>
        /// Write the specified message item to debug output.
        /// </summary>
        /// <param name="message">Item to write.</param>
        public static void WriteLine(string message)
        {
            var output = PlatformSingleton.Current.GetDebugOutput();
            if (output.IsAttached() && output.IsLogging())
            {
                string prefix = "***TELIMENA*** (AppInsights) Telemetry: ";
                output.WriteLine(prefix + message);
            }
        } 
        
        /// <summary>
        /// Write a debug log and try writing to Event log as well.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteError(string message)
        {
            WriteLine(message);
#if NETFRAMEWORK
            try
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = ".NET Runtime";
                    eventLog.WriteEntry(Process.GetCurrentProcess().ProcessName + " - " + message, EventLogEntryType.Error, 1000);
                }
            }
            catch (Exception ex)
            {
                WriteLine($"Cannot write to event store: {ex}");
            }
#endif
        }


        void IDebugOutput.WriteLine(string message)
        {
#if NETSTANDARD1_3
            Debug.WriteLine(message);
#else
            Debugger.Log(0, "category", message + Environment.NewLine);
#endif
        }

        bool IDebugOutput.IsLogging()
        {
            if (IsTracingDisabled)
            {
                return false;
            }
#if NETSTANDARD1_3
            return true;
#else
            return Debugger.IsLogging();
#endif
        }

        bool IDebugOutput.IsAttached()
        {
            return Debugger.IsAttached;
        }
    }
}
