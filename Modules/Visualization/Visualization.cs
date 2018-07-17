using Microsoft.AspNetCore.Hosting;
using TypeEdge.Enums;
using TypeEdge.Modules;
using TypeEdge.Modules.Enums;
using TypeEdge.Modules.Messages;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ThermostatApplication.Messages;
using ThermostatApplication.Modules;
using VisualizationWeb;

namespace Modules
{
    public class Visualization : EdgeModule, IVisualization
    {
        IWebHost _webHost;
        private HubConnection hubConnection;

        public override CreationResult Configure(IConfigurationRoot configuration)
        {
            _webHost = new WebHostBuilder()
                .UseConfiguration(configuration)
                .UseKestrel()
                .UseContentRoot(Path.Combine(Directory.GetCurrentDirectory()))
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            return base.Configure(configuration);
        }
        public override async Task<ExecutionResult> RunAsync()
        {
            TriggerSetupSignalrConnection();
            await _webHost.RunAsync();


            return ExecutionResult.Ok;
        }

        private async void TriggerSetupSignalrConnection()
        {
            try
            {
                await Task.Run(SetupSignalrConnectionAsync);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task SetupSignalrConnectionAsync()
        {
            hubConnection = await ConnectAsync("http://127.0.0.1:5000/visualizerhub");
        }

        // This method connects to a url for SignalR to send data.
        public static async Task<HubConnection> ConnectAsync(string baseUrl)
        {
            // Keep trying to until we can start
            while (true)
            {
                var connection = new HubConnectionBuilder()
                    .WithUrl(baseUrl)
                    .Build();
                try
                {
                    await connection.StartAsync();
                    Console.WriteLine($"HubConnection to {baseUrl} established.");
                    return connection;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await Task.Delay(1000);
                }
            }
        }

        public Visualization(IOrchestrator proxy)
        {
            proxy.Visualization.Subscribe(this, async (e) =>
            {
                await AddSampleAsync(e);
                return MessageResult.Ok;
            });
        }

        private int valueIdx = 0;
        private async Task AddSampleAsync(Temperature e)
        {
            if (hubConnection == null) return;

            await hubConnection.InvokeAsync("SendMessage", valueIdx.ToString(), e.Value);
            valueIdx++;
        }
        
    }
}
