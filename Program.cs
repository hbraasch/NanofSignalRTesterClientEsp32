using nanoFramework.Hardware.Esp32;
using nanoFramework.Networking;
using nanoFramework.SignalR.Client;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Memory = nanoFramework.Runtime.Native.GC;

namespace NanofSignalRTesterClientEsp32
{
    internal class Program
    {
        public static HubConnection hubConnection;
        private const string SERVER_BASE_URL = "https://qhwvcrqg-7285.aue.devtunnels.ms";
        private const string SSID = "XXX";
        private const string PSWD = "XXX";



        static void Main(string[] args)
        {
            try
            {
                if (!Debugger.IsAttached)
                {
                    Debug.WriteLine("App stopped because not connected to debugger. Remove this code when running standalone");
                    Thread.Sleep(Timeout.Infinite);
                }

                #region *// Wifi
                if (!ConnectToWifi()) throw new ApplicationException("Cannot connect to WIFI");

                Debug.WriteLine("Waiting 3 sec to have network stabilize");
                Thread.Sleep(3000);
                #endregion

                string connectionUrl = $"{SERVER_BASE_URL}/DirectMessage";
                X509Certificate myCert = new X509Certificate(_DigiCertGlobalRootG2);
                
                Program.hubConnection = new HubConnection(connectionUrl, options: new HubConnectionOptions() { Reconnect = true, Certificate = myCert, SslVerification = System.Net.Security.SslVerification.CertificateRequired });

                // Handle reconnection events (optional)
                Program.hubConnection.Closed += (s, e) =>
                {
                    // The hub connection will automatically attempt to reconnect.
                    Thread.Sleep(1000);
                    Program.hubConnection.Start();
                };

                Debug.WriteLine($"Connection URL: {connectionUrl}");

                try
                {
                    PrintMemory("Before");
                    Program.hubConnection.Start();
                }
                catch (Exception)
                {
                    PrintMemory("After");
                }

                while (true)
                {
                    Thread.Sleep(500);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
            }
        }

#if false
        // The code belows works fine on a Raspberry PI. The nanoFramework code is a port of this 
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            string SERVER_BASE_URL = "https://qhwvcrqg-7285.aue.devtunnels.ms";

            // Build the hub connection URL and include the client username as a query string parameter
            string connectionUrl = $"{SERVER_BASE_URL}/DirectMessage";

            var hubConnection = new HubConnectionBuilder()
                    .WithUrl(connectionUrl) // Use the connection URL with query string parameter
                    .WithAutomaticReconnect() // Enable automatic reconnection
                    .Build();

            // Handle reconnection events (optional)
            hubConnection.Closed += async(exception) =>
                {
                    // The hub connection will automatically attempt to reconnect.
                    await Task.Delay(TimeSpan.FromSeconds(5)); // Optional delay before attempting to reconnect.
                    await hubConnection.StartAsync();
            };

            await hubConnection.StartAsync();

            Console.WriteLine($"Waiting ...");
        }

#endif

        private static bool ConnectToWifi()
        {
            Debug.WriteLine("Connecting to WiFi...");

            // As we are using TLS, we need a valid date & time
            // We will wait maximum 1 minute to get connected and have a valid date
            var success = WifiNetworkHelper.ConnectDhcp(SSID, PSWD, requiresDateTime: true, token: new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token);
            if (!success)
            {
                Debug.WriteLine($"Can't connect to wifi: {WifiNetworkHelper.Status}");
                if (WifiNetworkHelper.Status == NetworkHelperStatus.ExceptionOccurred)
                {
                    Debug.WriteLine($"NetworkHelper.ConnectionError.Exception: {WifiNetworkHelper.HelperException.Message}");
                }
            }

            Debug.WriteLine($"Date and time is now {DateTime.UtcNow}");
            return success;
        }

        private const string _DigiCertGlobalRootG2 =
@"-----BEGIN CERTIFICATE-----
MIIDjjCCAnagAwIBAgIQAzrx5qcRqaC7KGSxHQn65TANBgkqhkiG9w0BAQsFADBh
MQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3
d3cuZGlnaWNlcnQuY29tMSAwHgYDVQQDExdEaWdpQ2VydCBHbG9iYWwgUm9vdCBH
MjAeFw0xMzA4MDExMjAwMDBaFw0zODAxMTUxMjAwMDBaMGExCzAJBgNVBAYTAlVT
MRUwEwYDVQQKEwxEaWdpQ2VydCBJbmMxGTAXBgNVBAsTEHd3dy5kaWdpY2VydC5j
b20xIDAeBgNVBAMTF0RpZ2lDZXJ0IEdsb2JhbCBSb290IEcyMIIBIjANBgkqhkiG
9w0BAQEFAAOCAQ8AMIIBCgKCAQEAuzfNNNx7a8myaJCtSnX/RrohCgiN9RlUyfuI
2/Ou8jqJkTx65qsGGmvPrC3oXgkkRLpimn7Wo6h+4FR1IAWsULecYxpsMNzaHxmx
1x7e/dfgy5SDN67sH0NO3Xss0r0upS/kqbitOtSZpLYl6ZtrAGCSYP9PIUkY92eQ
q2EGnI/yuum06ZIya7XzV+hdG82MHauVBJVJ8zUtluNJbd134/tJS7SsVQepj5Wz
tCO7TG1F8PapspUwtP1MVYwnSlcUfIKdzXOS0xZKBgyMUNGPHgm+F6HmIcr9g+UQ
vIOlCsRnKPZzFBQ9RnbDhxSJITRNrw9FDKZJobq7nMWxM4MphQIDAQABo0IwQDAP
BgNVHRMBAf8EBTADAQH/MA4GA1UdDwEB/wQEAwIBhjAdBgNVHQ4EFgQUTiJUIBiV
5uNu5g/6+rkS7QYXjzkwDQYJKoZIhvcNAQELBQADggEBAGBnKJRvDkhj6zHd6mcY
1Yl9PMWLSn/pvtsrF9+wX3N3KjITOYFnQoQj8kVnNeyIv/iPsGEMNKSuIEyExtv4
NeF22d+mQrvHRAiGfzZ0JFrabA0UWTW98kndth/Jsw1HKj2ZL7tcu7XUIOGZX1NG
Fdtom/DzMNU+MeKNhJ7jitralj41E6Vf8PlwUHBHQRFXGU7Aj64GxJUTFy8bJZ91
8rGOmaFvE7FBcf6IKshPECBV1/MUReXgRPTqh5Uykw7+U0b6LJ3/iyK5S9kJRaTe
pLiaWN0bfVKfjllDiIGknibVb63dDcY3fe0Dkhvld1927jyNxF1WW6LZZm6zNTfl
MrY=
-----END CERTIFICATE-----";


        public static void PrintMemory(string msg)
        {
            NativeMemory.GetMemoryInfo(NativeMemory.MemoryType.Internal, out uint totalSize, out uint totalFreeSize, out uint largestBlock);
            Debug.WriteLine($"\n{msg}-> Internal Total Mem {totalSize} Total Free {totalFreeSize} Largest Block {largestBlock}");
            Debug.WriteLine($"nF Mem {Memory.Run(false)}\n ");
        }

    }
}
