﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models
{
    public abstract class TelemetryDetail
    {
       
        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string IpAddress { get; set; }
        public virtual AssemblyVersionInfo AssemblyVersion { get; set; }
        public int? AssemblyVersionId { get; set; }
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public int TelemetrySummaryId { get; set; }

        public abstract TelemetrySummary GetTelemetrySummary();
        public abstract void SetTelemetrySummary(TelemetrySummary summary);
        public abstract IReadOnlyList<TelemetryUnit> GetTelemetryUnits();
    }
}