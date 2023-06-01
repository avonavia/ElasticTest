using System;
using Nest;
using System.Collections.Generic;
using Elasticsearch;


namespace ElasticTest
{
    public static class Elastic
    {
        public static void Work()
        {
            ElasticClient client = Create();

            ShowCommands();

            Commands(client);
            
        }
        static ElasticClient Create()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("cars");

            var client = new ElasticClient(settings);

            var Cars = new Car[50];

            Random randomID = new Random();
            Random randomColor = new Random();

            List<string> colors = new List<string> { "Green", "Red", "Blue", "Orange", "Yellow", "Purple", "Pink", "Black", "White" };


            for (int i = 0; i < 10; i++)
            {
                int colorID = randomColor.Next(colors.Count);
                Cars[i] = new Car { CarID = randomID.Next(200), Color = colors[colorID], CreatedDate = DateTime.Now };
            }

            foreach (var car in Cars)
            {
                client.IndexDocument(car);
            }

            if (!client.Ping().ApiCall.Success)
            {
                Console.WriteLine("Elastic is not on");
                Console.ReadKey();
            }

            if (!client.Indices.Exists("cars").Exists)
            {
                client.Indices.Create("cars", index => index);
            }
            else
            {
                Console.WriteLine("\nIndex created OR already exists");
            }

            return client;
        }

        static void Delete(ElasticClient client)
        {
            Console.WriteLine("\n");
            if (client.Indices.Exists("cars").Exists)
            {
                client.Indices.Delete("cars", index => index);

                Console.WriteLine("Deleted");
            }
            else
            {
                Console.WriteLine("Index doesn't exist");
            }
        }

        static void Select(ElasticClient client)
        {
            Console.WriteLine("\n");
            if (client.Indices.Exists("cars").Exists)
            {
                Console.WriteLine("Result:");
                var searchResponse = client.Search<Car>(s => s
                    .Query(q => q.Match(m => m.Field(f => f.Color).Query("White"))
                    )
                );

                var foundCars = searchResponse.Documents;

                foreach (var car in foundCars)
                {
                    Console.WriteLine($"ID: {car.CarID}, Color: {car.Color}, Date: {car.CreatedDate}");
                }

                if (foundCars.Count == 0)
                {
                    Console.WriteLine("None");
                }
            }
            else
            {
                Console.WriteLine("Index doesn't exist");
            }
        }

        static void ShowAll(ElasticClient client)
        {
            Console.WriteLine("\n");
            if (client.Indices.Exists("cars").Exists)
            {
                Console.WriteLine("Result:");
                var searchResponse = client.Search<Car>(s => s);

                var foundCars = searchResponse.Documents;

                foreach (var car in foundCars)
                {
                    Console.WriteLine($"ID: {car.CarID}, Color: {car.Color}, Date: {car.CreatedDate}");
                }
            }
            else
            {
                Console.WriteLine("Index doesn't exist");
            }
        }

        static void ShowCommands()
        {
            Console.WriteLine("\nTo Search press: 1\nTo Delete press: 2\nTo see all documents press: 3\nTo Create index press: 4");
        }

        static void Commands(ElasticClient client)
        {
            while (true)
            {
                var key = Console.ReadKey().Key;

                switch (key)
                {
                    case ConsoleKey.D1:
                        Select(client);
                        break;

                    case ConsoleKey.D2:
                        Delete(client);
                        break;

                    case ConsoleKey.D3:
                        ShowAll(client);
                        break;

                    case ConsoleKey.D4:
                        Create();
                        break;
                }
            }
        }
    }
}
