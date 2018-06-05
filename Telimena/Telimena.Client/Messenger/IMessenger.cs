﻿namespace Telimena.Client
{
    using System.Threading.Tasks;

    internal interface IMessenger
    {
        Task<string> SendPostRequest(string requestUri, object objectToPost);
    }
}