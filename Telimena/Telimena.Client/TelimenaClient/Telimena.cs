﻿namespace Telimena.Client
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Model;
    #endregion

    /// <summary>
    ///     Telemetry and Lifecycle Management Engine App
    ///     <para>This is a client SDK that allows handling application telemetry and lifecycle</para>
    /// </summary>
    public partial class Telimena : ITelimena
    {
        public async Task<UpdateCheckResult> CheckForUpdates()
        {
            await this.InitializeIfNeeded();
            try
            {
                string responseContent = await this.Messenger.SendGetRequest(ApiRoutes.GetLatestVersionInfo + "?programId=" + this.ProgramId);
                LatestVersionResponse response = this.Serializer.Deserialize<LatestVersionResponse>(responseContent);
                return UpdateResponseReader.ReadResponse(this.ProgramInfo, response);
            }
            catch (Exception ex)
            {
                if (!this.SuppressAllErrors)
                {
                    throw;
                }

                return new UpdateCheckResult()
                {
                    Error = ex
                };
            }
        }

        public async Task<RegistrationResponse> Initialize()
        {
            return await this.RegisterClient();
        }

        /// <summary>
        ///     Loads the referenced helper assemblies, e.g. for the purpose of updating
        /// </summary>
        /// <param name="assemblies"></param>
        public void LoadHelperAssemblies(params Assembly[] assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                this.HelperAssemblies.Add(assembly);
            }

            this.LoadAssemblyInfos(this.HelperAssemblies);
        }

        /// <summary>
        ///     Loads the referenced helper assemblies, e.g. for the purpose of updating
        /// </summary>
        /// <param name="assemblyNames"></param>
        public void LoadHelperAssembliesByName(params string[] assemblyNames)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            foreach (string assemblyName in assemblyNames)
            {
                Assembly assembly = Assembly.LoadFrom(Path.Combine(path, assemblyName));
                this.HelperAssemblies.Add(assembly);
            }

            this.LoadAssemblyInfos(this.HelperAssemblies);
        }

        /// <summary>
        ///     Report the usage of the application function.
        /// </summary>
        /// <param name="functionName"></param>
        /// <returns></returns>
        public async Task<StatisticsUpdateResponse> ReportUsage([CallerMemberName] string functionName = null)
        {
            try
            {
                await this.InitializeIfNeeded();
                StatisticsUpdateRequest request = new StatisticsUpdateRequest()
                {
                    ProgramId = this.ProgramId,
                    UserId = this.UserId,
                    FunctionName = functionName,
                    Version = this.ProgramInfo.PrimaryAssembly.Version
                };
                string responseContent = await this.Messenger.SendPostRequest(ApiRoutes.UpdateProgramStatistics, request);
                return this.Serializer.Deserialize<StatisticsUpdateResponse>(responseContent);
            }
            catch (Exception ex)
            {
                if (!this.SuppressAllErrors)
                {
                    throw;
                }

                return new StatisticsUpdateResponse()
                {
                    Error = ex
                };
            }
        }

        /// <summary>
        ///     Sends the initial app usage info
        /// </summary>
        /// <returns></returns>
        protected internal async Task<RegistrationResponse> RegisterClient(bool skipUsageIncrementation = false)
        {
            try
            {
                RegistrationRequest request = new RegistrationRequest()
                {
                    ProgramInfo = this.ProgramInfo,
                    TelimenaVersion = this.TelimenaVersion,
                    UserInfo = this.UserInfo,
                    SkipUsageIncrementation = skipUsageIncrementation
                };
                string responseContent = await this.Messenger.SendPostRequest(ApiRoutes.RegisterClient, request);
                RegistrationResponse response = this.Serializer.Deserialize<RegistrationResponse>(responseContent);
                this.UserId = response.UserId;
                this.ProgramId = response.ProgramId;
                return response;
            }
            catch (Exception ex)
            {
                if (!this.SuppressAllErrors)
                {
                    throw;
                }

                return new RegistrationResponse()
                {
                    Error = ex
                };
            }
        }

        private async Task InitializeIfNeeded()
        {
            if (!this.IsInitialized)
            {
                this.IsInitialized = true;
                await this.Initialize();
            }
        }

        private void LoadAssemblyInfos(IEnumerable<Assembly> assemblies)
        {
            this.ProgramInfo.HelperAssemblies = new List<AssemblyInfo>();
            foreach (Assembly assembly in assemblies)
            {
                this.ProgramInfo.HelperAssemblies.Add(new AssemblyInfo(assembly));
            }
        }
    }
}