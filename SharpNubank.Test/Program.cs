using Newtonsoft.Json;
using SharpNubank.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SharpNubank.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var accessTokenFile = "accesstoken.key";
                string accessToken = null;

                if (File.Exists(accessTokenFile))
                    accessToken = File.ReadAllText(accessTokenFile);

                using (var nu = new NubankClient(accessToken))
                {
                    Console.WriteLine("Nubank Client");
                    if (!nu.Authenticated)
                    {
                        Console.WriteLine("Please, type your login (CPF):");
                        var login = Console.ReadLine().Trim();
                        Console.WriteLine("Type your password:");
                        var password = Console.ReadLine().Trim();

                        var result = await nu.Login(login, password);
                        if (!result.Success)
                            throw new Exception(result.Message);

                        if (result.Data.NeedsDeviceAuthorization)
                        {
                            Console.WriteLine("You must authenticate with your phone to be able to access your data.");
                            Console.WriteLine("Scan the QRCode below with you Nubank application on the following menu:");
                            Console.WriteLine("Nu(Seu Nome) > Perfil > Acesso pelo site");
                            Console.WriteLine();
                            Console.Write(result.Data.GetQrCodeAsAscii());
                            Console.WriteLine();
                            Console.ReadKey();

                            result = await nu.AutenticateWithQrCode(result.Data.Code);
                            if (!result.Success)
                                throw new Exception(result.Message);

                            File.WriteAllText(accessTokenFile, nu.GetAccessToken());
                        }
                    }

                    Console.WriteLine("Authenticated");
                    var details = await nu.CustomerDetails();
                    if (!details.Success)
                        throw new Exception(details.Message);

                    Console.WriteLine($"Hi {details.Data.PreferredName}");

                    var events = await nu.CreditCardEvents();
                    if (!events.Success)
                        throw new Exception(events.Message);

                    Console.WriteLine($"These are your last events");
                    foreach (var @event in events.Data.Take(5))
                    {
                        Console.WriteLine($"{@event.Time.ToLocalTime()} - {@event.Title} - {@event.Amount} - {@event.Description}");
                    }

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
