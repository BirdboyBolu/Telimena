﻿using System;
using System.Collections.Generic;

namespace TelimenaClient
{
    /// <summary>
    /// Class StatisticsUpdateRequest.
    /// </summary>
    public class TelemetryUpdateRequest
    {
        /// <summary>
        /// For serialization
        /// </summary>
        protected TelemetryUpdateRequest()
        {

        }

        /// <summary>
        /// New instance of request
        /// </summary>
        /// <param name="telemetryKey"></param>
        public TelemetryUpdateRequest(Guid telemetryKey)
        {
            if (telemetryKey == Guid.Empty)
            {
                throw new ArgumentException("Telemetry key is an empty guid.", nameof(this.TelemetryKey));
            }

            this.TelemetryKey = telemetryKey;
        }
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public Guid UserId { get; set; }
        /// <summary>
        /// Gets or sets the unique program's telemetry key
        /// </summary>
        /// <value>The program identifier.</value>
        public Guid TelemetryKey { get; set; }
        /// <summary>
        /// Gets or sets the name of the view.
        /// </summary>
        /// <value>The name of the view.</value>
        public string ComponentName { get; set; }
        
        /// <summary>
        /// Holds info about the version components (file, assembly)
        /// </summary>
        public VersionData VersionData { get; set; }

        /// <summary>
        /// Gets or sets the custom data.
        /// </summary>
        /// <value>The custom data.</value>
        public Dictionary<string, string> TelemetryData { get; set; }
    }
}