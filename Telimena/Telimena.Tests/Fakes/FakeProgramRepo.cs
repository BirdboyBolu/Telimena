﻿namespace Telimena.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Client;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Repository;

    public class FakeProgramRepo : FakeRepo<Program>, IProgramRepository
    {
        public List<ProgramUsage> Usages { get; set; } = new List<ProgramUsage>();

        public void AddUsage(ProgramUsage objectToAdd)
        {
            this.Usages.Add(objectToAdd);
        }

        public Task<IEnumerable<Program>> GetProgramsByDeveloperName(string developerName)
        {
            throw new NotImplementedException();
        }

        public UsageData GetUsage(Program program, ClientAppUser clientAppUser)
        {
            return this.Usages.FirstOrDefault(x=>x.ClientAppUser == clientAppUser && x.Program == program);
        }

        public Task<List<ProgramUsage>> GetAllUsages(Program program)
        {
            throw new NotImplementedException();
        }
    }
}