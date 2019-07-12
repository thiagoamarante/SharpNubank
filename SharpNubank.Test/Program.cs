using System;
using System.Threading.Tasks;

namespace SharpNubank.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                using (var nu = new NubankClient())
                {
                    Console.WriteLine("Nubank Client");
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
