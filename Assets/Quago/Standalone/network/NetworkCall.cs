/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class NetworkCall
{
    protected static Logger LOG = new Logger("NetworkCall");

    public int execute(QuagoNetworkRequest container)
    {
        try
        {
            ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

            Dictionary<string, string> headers = container.GetHeaders();
            string body = container.GenerateBody();

            /* Keep one logger check to save from foreach() */
            if (Logger.AskForPermission(QuagoSettings.LogLevel.DEBUG))
            {
                LOG.D("execute", "Url = {0}", container.Url);
                LOG.D("execute", "Headers = ");
                foreach (KeyValuePair<string, string> entry in headers)
                    LOG.D("execute", "\t[{0}, {1}]", entry.Key, entry.Value);
                LOG.D("execute", "Payload = {0}", body);
            }

            HttpWebRequest request = null;
            try
            {
                /* set default POST configuration options */
                request = (HttpWebRequest)WebRequest.Create(container.Url);
                request.Method = container.RequestMethod;

                WebHeaderCollection webHeaders = new WebHeaderCollection();

                /* set headers */
                foreach (KeyValuePair<string, string> entry in headers)
                    webHeaders.Add(entry.Key, entry.Value);

                request.Headers = webHeaders;
                request.ContentType = "application/json";
                request.Timeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
                request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

                /* necessary to pass the body to the connection */
                if (container.DoOutput)
                {
                    byte[] bytes = new System.Text.UTF8Encoding().GetBytes(body);
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(bytes, 0, bytes.Length);
                    dataStream.Close();
                }

                /* Send the request then wait here until it returns */
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                /* Response Code */
                return (int)response.StatusCode;
            }
            catch (Exception e1)
            {
                LOG.E("execute-e1", e1);
            }
            finally
            {
                if (request != null) request.Abort();
            }
        }
        catch (Exception e2)
        {
            LOG.E("execute-e2", e2);
        }
        /* Retry */
        return 0;
    }

    public bool RemoteCertificateValidationCallback(System.Object sender,
    X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        // If there are errors in the certificate chain,
        // look at each error to determine the cause.
        if (sslPolicyErrors == SslPolicyErrors.None) return true;

        for (int i = 0; i < chain.ChainStatus.Length; i++)
        {
            if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
            {
                continue;
            }
            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
            chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
            chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
            bool chainIsValid = chain.Build((X509Certificate2)certificate);
            if (!chainIsValid) return false;
        }

        return true;
    }
}
#endif