using unifysolutions.technical_test.Client;
using unifysolutions.technical_test.Model;


var petStoreClient = new PetstoreClient(new HttpClient());
var cts = new CancellationTokenSource();
var availablePets = await petStoreClient.GetAvailablePetsOrderByNameAsync(SortOrder.Descending,cts.Token);
foreach (var pet in availablePets)
{
    Console.WriteLine(pet.Name);
}
Console.WriteLine();
Console.WriteLine("Press any key to exit");
var key = Console.ReadKey();