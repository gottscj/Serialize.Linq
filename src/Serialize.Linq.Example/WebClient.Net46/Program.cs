using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;
using Newtonsoft.Json;
using Serialize.Linq;
using Serialize.Linq.Extensions;
using Serialize.Linq.Nodes;

namespace WebClient.Net46
{
    class Program
    {
        private static MediaTypeWithQualityHeaderValue _mediaTypeJson;
        private const string ConnectionString = "http://localhost:51807";

        static void Main(string[] args)
        {
            _mediaTypeJson = new MediaTypeWithQualityHeaderValue("application/json");

            Console.WriteLine("Wait for Server to start, then press any key to start...");
            Console.ReadKey();
            ColorWriteLine($"Connecting to {ConnectionString}/signalr", ConsoleColor.Cyan);

            var hubConnection = new HubConnection(ConnectionString + "/signalr");
            var hubProxy = hubConnection.CreateHubProxy("PersonHub");

            hubProxy.JsonSerializer.Converters.Add(new ExpressionNodeJsonConverter());
            hubConnection.TraceLevel = TraceLevels.StateChanges;
            hubConnection.TraceWriter = new ConsoleTextWriter();
            hubConnection.Start(new WebSocketTransport()).Wait();

            ColorWriteLine($"Connected to {hubConnection.Url}", ConsoleColor.Green);

            ColorWriteLine("Run WebRequests:", ConsoleColor.Cyan);
            RunWebRequestsAsync(CancellationToken.None)
                .ContinueWith(_ => ColorWriteLine($"Web Api Finished", ConsoleColor.Green))
                .Wait();


            ColorWriteLine("Run SignalR:", ConsoleColor.Cyan);
            RunSignalRRequestsAsync(hubProxy)
                .ContinueWith(_ => ColorWriteLine($"SignalR Finished", ConsoleColor.Green))
                .Wait();

            ColorWriteLine("Example done... Press any key to exit...", ConsoleColor.Cyan);
            Console.ReadKey();
        }

        private static void ColorWriteLine(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.WriteLine();
            Console.ResetColor();
        }
        
        private static async Task RunWebRequestsAsync(CancellationToken cancellationToken)
        {
            await RunAllPersonsAsync(() => GetAllPersons(cancellationToken));
            
            await RunAllPersonsFromJapan(exp => QueryPersons(exp, cancellationToken));
            
            await RunAllPersonsOfAge100(exp => QueryPersons(exp, cancellationToken));
            
            await RunAllMalePersons(exp => QueryPersons(exp, cancellationToken));
            
            await RunAllLivingPersons(exp => QueryPersons(exp, cancellationToken));

            
        }

        private static async Task RunSignalRRequestsAsync(IHubProxy hubProxy)
        {
            await RunAllPersonsAsync(() => hubProxy.Invoke<IEnumerable<Person>>("GetAllPersons"));
            
            await RunAllPersonsFromJapan(exp => hubProxy.Invoke<IEnumerable<Person>>("Query", exp.ToExpressionNode()));

            await RunAllPersonsOfAge100(exp => hubProxy.Invoke<IEnumerable<Person>>("Query", exp.ToExpressionNode()));

            await RunAllMalePersons(exp => hubProxy.Invoke<IEnumerable<Person>>("Query", exp.ToExpressionNode()));

            await RunAllLivingPersons(exp => hubProxy.Invoke<IEnumerable<Person>>("Query", exp.ToExpressionNode()));

            await RunQueryAndFieldProjection(
                (exp, fields) => hubProxy.Invoke<IEnumerable<Person>>("Query", exp.ToExpressionNode(),
                    fields.ToExpressionNode()));
        }

        private static async Task RunQueryAndFieldProjection(
            Func<Expression<Func<Person, bool>>, Expression<Func<Person, Person>>, Task<IEnumerable<Person>>> query)
        {
            ColorWriteLine("All persons from Japan, only get 'Age'", ConsoleColor.DarkYellow);
            Expression<Func<Person, bool>> expression = p => p.Residence == "Japan";
            Expression<Func<Person, Person>> fields = p => new Person {Age = p.Age};
            var persons = await query(expression, fields);
            ShowPersons(persons);

        }
        private static async Task RunAllPersonsAsync(Func<Task<IEnumerable<Person>>> getAllTask)
        {
            ColorWriteLine("All persons", ConsoleColor.DarkYellow);
            var persons = await getAllTask();
            ShowPersons(persons);
        }

        private static async Task RunAllPersonsFromJapan(Func<Expression<Func<Person, bool>>, Task<IEnumerable<Person>>> query)
        {
            ColorWriteLine("All persons from Japan", ConsoleColor.DarkYellow);
            Expression<Func<Person, bool>> expression = p => p.Residence == "Japan";
            var persons = await query(expression);
            ShowPersons(persons);
        }

        private static async Task RunAllPersonsOfAge100(Func<Expression<Func<Person, bool>>, Task<IEnumerable<Person>>> query)
        {
            ColorWriteLine("All persons of Age >= 100", ConsoleColor.DarkYellow);
            Expression<Func<Person, bool>> expression = p => p.Age >= 100;
            var persons = await query(expression);
            ShowPersons(persons);
        }

        private static async Task RunAllMalePersons(Func<Expression<Func<Person, bool>>, Task<IEnumerable<Person>>> query)
        {
            ColorWriteLine("All male persons", ConsoleColor.DarkYellow);
            Expression<Func<Person, bool>> expression = p => p.Gender == Gender.Male;
            var persons = await query(expression);
            ShowPersons(persons);
        }

        private static async Task RunAllLivingPersons(Func<Expression<Func<Person, bool>>, Task<IEnumerable<Person>>> query)
        {
            ColorWriteLine("All living persons", ConsoleColor.DarkYellow);
            Expression<Func<Person, bool>> expression = p => p.DeathDate == null;
            var persons = await query(expression);
            ShowPersons(persons);
        }

        private static async Task<IEnumerable<Person>> GetAllPersons(CancellationToken cancellationToken)
        {
            using (var client = PrepareHttpClient())
            {
                var response = await client.GetAsync("api/persons", cancellationToken);
                response.EnsureSuccessStatusCode();
                var json  = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<Person>>(json);
            }
        }

        private static async Task<IEnumerable<Person>> QueryPersons(Expression<Func<Person, bool>> query, CancellationToken cancellationToken)
        {
            var queryNode = query.ToExpressionNode();
            using (var client = PrepareHttpClient())
            {
                var jsonQuery = JsonConvert.SerializeObject(queryNode, new ExpressionNodeJsonConverter());
                var response = await client.PostAsync("api/persons", new StringContent(jsonQuery, Encoding.UTF8, "application/json"), cancellationToken);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<Person>>(json);
            }
        }

        private static HttpClient PrepareHttpClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(ConnectionString) };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(_mediaTypeJson);
            return client;
        }

        private static void ShowPersons(IEnumerable<Person> persons)
        {
            foreach (var person in persons)
            {
                ColorWriteLine($"{person.Id}) {person.FirstName} {person.LastName}, {person.Gender}, age {person.Age}, from {person.Residence}", ConsoleColor.DarkCyan);
            }
        }
    }
}
