using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serialize.Linq.Nodes;

namespace WebApp.AspNetCore.Controllers
{
    [Route("api/[controller]")]
    public class PersonsController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<Person> Get()
        {
            return PersonRepository.Current;
        }

        [HttpPost]
        public IEnumerable<Person> Query([FromBody]ExpressionNode query)
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

        
    }
}
