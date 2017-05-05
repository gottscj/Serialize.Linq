using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Models;

namespace WebApp.AspNetCore
{
    public class PersonRepository : IEnumerable<Person>
    {
        private static readonly Lazy<PersonRepository> 
            Instance = new Lazy<PersonRepository>(() => new PersonRepository(),
            LazyThreadSafetyMode.ExecutionAndPublication);

        public static readonly PersonRepository Current = Instance.Value;
        private readonly List<Person> _persons = new List<Person>();
        private readonly object _syncRoot = new object();
        public IEnumerator<Person> GetEnumerator()
        {
            lock (_syncRoot)
            {
                return _persons.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public PersonRepository()
        {
            var records = ReadPersonRecords();
            var culture = new CultureInfo("de-DE");

            foreach (var record in records)
            {
                if (string.IsNullOrWhiteSpace(record))
                    continue;

                try
                {
                    var line = record.Split(';');
                    var birthdate = DateTime.Parse(line[3], culture);
                    var deathdate = line[4].Equals("Living", StringComparison.OrdinalIgnoreCase) ? null : (DateTime?)DateTime.Parse(line[4], culture);
                    _persons.Add(CreatePerson(_persons.Count + 1, line[1], line[2], birthdate, deathdate, line[6]));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Failed to parse record '{0}': {1}", record, ex.Message);
                }
            }
        }

        private Person CreatePerson(int id, string name, string gender, DateTime birthDate, DateTime? deathDate, string residence)
        {
            var names = name.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            var age = CalculateAge(birthDate, deathDate);

            return new Person
            {
                Id = id,
                Age = age,
                BirthDate = birthDate,
                DeathDate = deathDate,
                FirstName = names[0],
                LastName = names[1],
                Gender = gender.Trim().ToLowerInvariant() == "f" ? Gender.Female : Gender.Male,
                Residence = residence,
            };
        }

        private int CalculateAge(DateTime birthDate, DateTime? deathDate)
        {
            var endDate = deathDate ?? DateTime.Today;
            var age = endDate.Year - birthDate.Year;
            if (birthDate > endDate.AddYears(-age))
                --age;

            return age;
        }

        private static IEnumerable<string> ReadPersonRecords()
        {
            var csvStream = File.OpenRead("Persons.csv");

            if (csvStream == null)
                throw new InvalidProgramException("Failed to read Persons.");

            using (csvStream)
            {
                using (var reader = new StreamReader(csvStream))
                {
                    var text = reader.ReadToEnd();
                    text = text.Replace("\r", "");
                    return text.Split('\n');
                }
            }
        }
    }
}
