using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Serialize.Linq.Nodes;

namespace WebApp.AspNetCore.Hubs
{
    public class PersonHub : Hub
    {
        public IEnumerable<Person> GetAllPersons()
        {
            return PersonRepository.Current;
        }

        public IEnumerable<Person> Query(ExpressionNode query)
        {
            try
            {
                var expression = query.ToBooleanExpression<Person>();
                return PersonRepository.Current.Where(expression.Compile());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return new List<Person>();
            }
        }

        public IEnumerable<Person> Query(ExpressionNode query, ExpressionNode fields)
        {
            var persons = Query(query);
            var fieldsExpression = (Expression<Func<Person, Person>>) fields.ToExpression();
            persons = persons.Select(fieldsExpression.Compile());
            return persons;
        }

        public override Task OnConnected()
        {
            Console.WriteLine($"Client {Context.ConnectionId} connected");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Console.WriteLine($"Client {Context.ConnectionId} connected");
            return base.OnDisconnected(stopCalled);
        }
    }
}
