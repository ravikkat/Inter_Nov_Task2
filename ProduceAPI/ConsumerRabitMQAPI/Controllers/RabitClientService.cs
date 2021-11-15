using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Text;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
//using System.Net.Http;
using ConsumerRabitMQAPI.Models;
namespace ConsumerRabitMQAPI.Controllers
{
    public class RabitClientService : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private IModel _channel;
        private Timer _timer;
        private ConnectionFactory factory;
        public RabitClientService(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<RabitClientService>();
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
             factory = new ConnectionFactory()
            {
                HostName = "localhost",
                //localhost
                //host.docker.internal
                Port = 31672
                //  HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                // Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
            };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            _timer = new Timer(PublishBitcoin, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(15*60));

            return Task.CompletedTask;
        }

        private void PublishBitcoin(object state)
        {
            if(factory == null)
            {
                factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    //localhost
                    //host.docker.internal
                    Port = 31672
                    //  HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                    // Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
                };
            }

            string smessagerabit = GetbitCoindata();


            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //channel.QueueDeclare(queue: "greetings",
                //                     durable: false,
                //                     exclusive: false,
                //                     autoDelete: false,
                //                     arguments: null);

                //string message ="Greet";
                //var body = Encoding.UTF8.GetBytes(message);

                //channel.BasicPublish(exchange: "",
                //                     routingKey: "greetings",
                //                     basicProperties: null,
                //                     body: body);



                var body = Encoding.UTF8.GetBytes(smessagerabit);

                channel.BasicPublish(exchange: "",
                                     routingKey: "BitCoin",
                                     basicProperties: null,
                                     body: body);
            }
        }

        private void HandleMessage(string content)
        {
            // we just print this message   
            _logger.LogInformation($"consumer received {content}");
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }

        private string GetbitCoindata()
        {
            string sResulut = "Test";
            string surl = "https://api.coindesk.com/v1/bpi/currentprice.json";
            using (var client = new System.Net.Http.HttpClient())
            {

                client.BaseAddress = new Uri(surl);

                //HTTP GET
                try
                {
                    var responseTask = client.GetAsync(surl);
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();

                        var alldata = readTask.Result;
                        //  var roles = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(alldata);
                        //  dynamic stuff = JsonConvert.DeserializeObject(alldata);
                        // dynamic objParsedata = Newtonsoft.Json.Linq.JObject.Parse(alldata);

                        sResulut = alldata;
                       // sgdCurrency = ((Newtonsoft.Json.Linq.JContainer)objParsedata).Last.ToString();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("{0} Exception caught.", e);
                }

            }
            return sResulut;

        }
    }
}
