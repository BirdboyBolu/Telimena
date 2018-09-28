﻿using System;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Telimena.ToolkitClient
{
    #region Using

    #endregion

    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        /// <summary>
        ///     Creates a new instance of Telimena Client
        /// </summary>
        /// <param name="telemetryApiBaseUrl">Leave default, unless you want to call different telemetry server</param>
        /// <param name="mainAssembly">
        ///     Leave null, unless you want to use different assembly as the main one for program name,
        ///     version etc
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Telimena(Assembly mainAssembly = null, Uri telemetryApiBaseUrl = null)
        {
            if (telemetryApiBaseUrl == null)
            {
                telemetryApiBaseUrl = DefaultApiUri;
            }

            if (mainAssembly == null)
            {
                mainAssembly = GetProperCallingAssembly();
            }

            StartupData data = LoadProgramData(mainAssembly);
            this.ProgramInfo = data.ProgramInfo;
            this.UserInfo = data.UserInfo;
            this.TelimenaVersion = data.TelimenaVersion;
            this.UpdaterVersion = data.UpdaterVersion;

            this.HttpClient = new TelimenaHttpClient(new HttpClient {BaseAddress = telemetryApiBaseUrl});
            this.Messenger = new Messenger(this.Serializer, this.HttpClient);
        }

        /// <summary>
        ///     Creates a new instance of Telimena Client
        /// </summary>
        /// <param name="telemetryApiBaseUrl">Leave default, unless you want to call different telemetry server</param>
        /// <param name="programInfo">
        ///   Send custom program info if you don't want assembly based approach (and if you know what you're doing) 
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Telimena(ProgramInfo programInfo, Uri telemetryApiBaseUrl = null)
        {
            if (telemetryApiBaseUrl == null)
            {
                telemetryApiBaseUrl = DefaultApiUri;
            }

            Assembly assembly = GetProperCallingAssembly();

            StartupData data = LoadProgramData(assembly, programInfo);
            this.ProgramInfo = data.ProgramInfo;
            this.UserInfo = data.UserInfo;
            this.TelimenaVersion = data.TelimenaVersion;
            this.UpdaterVersion = data.UpdaterVersion;

            this.HttpClient = new TelimenaHttpClient(new HttpClient {BaseAddress = telemetryApiBaseUrl});
            this.Messenger = new Messenger(this.Serializer, this.HttpClient);
        }

    }
} 