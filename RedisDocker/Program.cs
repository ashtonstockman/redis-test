using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisDocker
{
    public class Program
    {
        public static string redisConnString = "192.168.99.100:32768";

        static void Main(string[] args)
        {
            Console.WriteLine("Please select 1 to work with simple keys or 2 to work with complex hashsets.");
            switch (Console.ReadLine())
            {
                case "1":
                    SimpleKeys();
                    break;
                case "2":
                    ComplexHashsets();
                    break;
                default:
                    Console.WriteLine("Please enter 1 or 2.  If done, close the window :D");
                    break;
            }
        }

        #region ComplexHashsets

        private static void ComplexHashsets()
        {
            while (true)
            {
                Console.WriteLine("Please select 1 to write values to redis and 2 to read values from redis.");
                switch (Console.ReadLine())
                {
                    case "1":
                        WriteComplexToRedis();
                        break;
                    case "2":
                        ReadComplexFromRedis();
                        break;
                    default:
                        Console.WriteLine("Please enter 1 or 2.  If done, close the window :D");
                        break;
                }
            }
        }

        private static void ReadComplexFromRedis()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnString);
            var db = redis.GetDatabase(0);

            Console.WriteLine("Please enter a key to retrieve from local Redis cache...");
            var requestedKey = Console.ReadLine();

            var readObject = new MyObject();
            readObject.ObjectAdress = new Address();
            var hashEntries = db.HashGetAll($"user:{requestedKey}");

            readObject.Id = (long)hashEntries.Where(entry => entry.Name == "Id").First().Value;
            readObject.Name = hashEntries.Where(entry => entry.Name == "Name").First().Value;
            readObject.ObjectAdress.StreetAddress = hashEntries.Where(entry => entry.Name == "StreetAddress").First().Value;
            readObject.ObjectAdress.ZipCode = hashEntries.Where(entry => entry.Name == "ZipCode").First().Value;

            Console.WriteLine(readObject.ToString());
        }

        private static void WriteComplexToRedis()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnString);
            var db = redis.GetDatabase(0);

            var address = new Bogus.DataSets.Address();

            var storedObject = new MyObject();
            storedObject.Id = db.StringIncrement("UniqueUserId"); // Fetch unique id for object
            storedObject.Name = new Bogus.DataSets.Name().FindName();
            storedObject.ObjectAdress = new Address() { StreetAddress = address.StreetAddress(), ZipCode = address.ZipCode() };

            var propertyList = ConvertToHashEntryList(storedObject);
            db.HashSet("user:" + storedObject.Id, propertyList.ToArray());

            Console.WriteLine("the following user was saved" + Environment.NewLine + storedObject.ToString());
        }

        private static List<HashEntry> ConvertToHashEntryList(object instance)
        {
            var propertiesInHashEntryList = new List<HashEntry>();
            foreach (var property in instance.GetType().GetProperties())
            {
                if (!property.Name.Equals("ObjectAdress"))
                {
                    // This is just for an example
                    propertiesInHashEntryList.Add(new HashEntry(property.Name, instance.GetType().GetProperty(property.Name).GetValue(instance).ToString()));
                }
                else
                {
                    var subPropertyList = ConvertToHashEntryList(instance.GetType().GetProperty(property.Name).GetValue(instance));
                    propertiesInHashEntryList.AddRange(subPropertyList);
                }
            }
            return propertiesInHashEntryList;
        }

        #endregion

        #region SimpleKeys

        private static void SimpleKeys()
        {
            while (true)
            {
                Console.WriteLine("Please select 1 to write values to redis and 2 to read values from redis.");
                switch (Console.ReadLine())
                {
                    case "1":
                        WriteToRedis();
                        break;
                    case "2":
                        ReadFromRedis();
                        break;
                    default:
                        Console.WriteLine("Please enter 1 or 2.  If done, close the window :D");
                        break;
                }
            }
        }

        private static void WriteToRedis()
        {
            Console.WriteLine("Please enter a key to write to local Redis cache...");
            var requestedKey = Console.ReadLine();

            Console.WriteLine($"Please enter a value to save for key: {requestedKey}.");
            var setVal = Console.ReadLine();

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnString);
            var db = redis.GetDatabase(0);

            db.StringSet(requestedKey, setVal);

            var output = $"The key {requestedKey} has an updated value of {db.StringGet(requestedKey)}.";
            Console.WriteLine(output);
        }

        private static void ReadFromRedis()
        {
            Console.WriteLine("Please enter a key to retrieve from local Redis cache...");
            var requestedKey = Console.ReadLine();

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnString);
            var db = redis.GetDatabase(0);
            var output = $"The requested key {requestedKey} has a value of {db.StringGet(requestedKey)}.";
            Console.WriteLine(output);
        }

        #endregion
    }

    public class MyObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Address ObjectAdress { get; set; }

        public override string ToString()
        {
            return $"userId:{Id.ToString()}  name: {Name}  Address: {ObjectAdress.StreetAddress} {ObjectAdress.ZipCode}";
        }
    }
    public class Address
    {
        public string StreetAddress { get; set; }
        public string ZipCode { get; set; }
    }
}
