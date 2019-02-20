using Microsoft.Azure.Devices.Client;
using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AzureSamples.EdgeDownstreamDeviceSas
{
    class Program
    {
        private static int MESSAGE_COUNT = 10;
        private const int TEMPERATURE_THRESHOLD = 30;
        static string ConnectionString = "connection_string;GatewayHostName=iot_edge_gw_host_name";
        static void Main(string[] args)
        {
            var rootCertificatePath = Environment.GetEnvironmentVariable("CA_CERTIFICATE_PATH");

            // Add Root Certificate
            InstallCACert(certificatePath: rootCertificatePath);

            var deviceClient = DeviceClient.CreateFromConnectionString(ConnectionString);
            SendEvents(deviceClient, MESSAGE_COUNT).Wait();
            Console.WriteLine("Exiting!\n");
            Console.ReadLine();
        }

        /// <summary>
        /// Add certificate in local cert store for use by downstream device
        /// client for secure connection to IoT Edge runtime and for identity with IoT Hub.
        ///
        ///    Note: On Windows machines, if you have not run this from an Administrator prompt,
        ///    a prompt will likely come up to confirm the installation of the certificate.
        ///    This usually happens the first time a certificate will be installed.
        /// </summary>
        static void InstallCACert(string certificatePath, string certificatePassword = null)
        {
            if (!string.IsNullOrWhiteSpace(certificatePath))
            {
                Console.WriteLine("User configured CA certificate path: {0}", certificatePath);
                if (!File.Exists(certificatePath))
                {
                    // cannot proceed further without a proper cert file
                    Console.WriteLine("Invalid certificate file: {0}", certificatePath);
                    throw new InvalidOperationException("Invalid certificate file.");
                }
                else
                {
                    Console.WriteLine("Attempting to install CA certificate: {0}", certificatePath);
                    X509Store store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
                    store.Open(OpenFlags.ReadWrite);
                    if (certificatePassword == null)
                    {
                        store.Add(new X509Certificate2(certificatePath));
                    }
                    else
                    {
                        store.Add(new X509Certificate2(certificatePath, certificatePassword));
                    }
                    Console.WriteLine("Successfully added certificate: {0}", certificatePath);
                    store.Close();
                }
            }
            else
            {
                Console.WriteLine("CA_CERTIFICATE_PATH was not set or null, not installing any CA certificate");
            }
        }

        /// <summary>
        /// Send telemetry data, (random temperature and humidity data samples).
        /// to the IoT Edge runtime. The number of messages to be sent is determined
        /// by environment variable MESSAGE_COUNT.
        /// </summary>
        static async Task SendEvents(DeviceClient deviceClient, int messageCount)
        {
            string dataBuffer;
            Random rnd = new Random();
            Console.WriteLine("Edge downstream device attempting to send {0} messages to Edge Hub...\n", messageCount);

            for (int count = 0; count < messageCount; count++)
            {
                float temperature = rnd.Next(20, 35);
                float humidity = rnd.Next(60, 80);
                dataBuffer = string.Format(new CultureInfo("en-US"), "{{MyFirstDownstreamDevice \"messageId\":{0},\"temperature\":{1},\"humidity\":{2}}}", count, temperature, humidity);
                Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));
                eventMessage.Properties.Add("temperatureAlert", (temperature > TEMPERATURE_THRESHOLD) ? "true" : "false");
                Console.WriteLine("\t{0}> Sending message: {1}, Data: [{2}]", DateTime.Now.ToLocalTime(), count, dataBuffer);

                await deviceClient.SendEventAsync(eventMessage).ConfigureAwait(false);
            }
        }
    }
}
