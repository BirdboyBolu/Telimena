﻿using System;
using Newtonsoft.Json;
using TelimenaClient;

namespace AutomaticTestsClient
{
    public class TestAppWorker
    {
        private readonly Arguments arguments;

        public TestAppWorker(Arguments arguments)
        {
            this.arguments = arguments;
        }

        public void Work()
        {
            Telimena telimena = this.GetTelimena(this.arguments.TelemetryKey);


            try
            {
                switch (this.arguments.Action)
                {
                    case Actions.Initialize:
                        this.HandleInitialize(telimena);
                        break;
                    case Actions.ReportViewUsage:
                        this.HandleReportViewUsage(telimena);
                        break;
                    case Actions.HandleUpdates:
                        telimena.HandleUpdatesBlocking(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Done");
        }

        private Telimena GetTelimena(Guid argumentsTelemetryKey)
        {
            Telimena telimena;
            if (this.arguments.ProgramInfo != null)
            {
             
                telimena = new Telimena(argumentsTelemetryKey, telemetryApiBaseUrl: new Uri(this.arguments.ApiUrl), programInfo: this.arguments.ProgramInfo);
            }
            else
            {
                telimena = new Telimena(this.arguments.TelemetryKey, telemetryApiBaseUrl: new Uri(this.arguments.ApiUrl));
            }

            return telimena;
        }

        private void HandleInitialize(Telimena telimena)
        {

            TelemetryInitializeResponse result = telimena.InitializeBlocking_toReDo();

            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        private void HandleReportViewUsage(Telimena telimena)
        {
            
            TelemetryUpdateResponse result;
            if (this.arguments.ViewName != null)
            {
                result = telimena.ReportViewAccessedBlocking(this.arguments.ViewName);
            }
            else
            {
                result = telimena.ReportViewAccessedBlocking("DefaultView");
            }

            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}