using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json.Serialization;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;


namespace MyApp.Namespace
{
    public class SupportModel : PageModel
    {

        const string connectionString = "AccountEndpoint=https://ibas-db-account-3315.documents.azure.com:443/;AccountKey=RPLN3ZgzoWV4QlX6uksMfZImUdnJnCSPUS7vI0SWyjRnCKea7HoGq5twATLY8ZTXOpZdJBcXCJDJACDbQwgk3g==;";
        CosmosClient _dbclient;

        public SupportModel()
        {
            CosmosSerializationOptions serializerOptions = new()
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            };

            _dbclient = new CosmosClientBuilder(connectionString)
            .WithSerializerOptions(serializerOptions)
            .Build();


            Henvendelse.Bruger = KontaktPerson;

            
        }

        public async Task<IActionResult> OnPostAsync()
        {


            Henvendelse.HenvendelseID = Guid.NewGuid().ToString();
            KontaktPerson.BrugerID = Guid.NewGuid().ToString();
            Console.WriteLine(Henvendelse.HenvendelseID + " - " + KontaktPerson.BrugerID);

            if (!ModelState.IsValid || Henvendelse == null)
            {
                Console.WriteLine("Yo, this no works");
                foreach(var state in ModelState)
            {
                if (state.Value.Errors.Any())
                {
                    Console.WriteLine($"Key: {state.Key}, Errors: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }
                return Page();
            }


            var container = _dbclient.GetContainer("IBasSupportDB", "ibassupport");

            try
            {
             
                PartitionKey categoryKey = new(Henvendelse.category);
                ItemResponse<Issue> response = await container.UpsertItemAsync(Henvendelse, categoryKey);
                //Console.WriteLine("Virker ikke");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ModelState.AddModelError(string.Empty, ex.Message);
                return BadRequest();
            }

            return RedirectToPage("./Index");
        }

        public class Bruger
        {
            [JsonPropertyName("Id")] // Her angiver vi et alternativt navn for egenskaben
            public string BrugerID { get; set; }

            public string? Navn { get; set; }

            public string? Email { get; set; }
        }

        public class Issue
        {
            [JsonPropertyName("Id")] // Her angiver vi et alternativt navn for egenskaben
            public string HenvendelseID { get; set; }

            public Bruger? Bruger { get; set; }

            public string? Telefon { get; set; }

            public string? Beskrivelse { get; set; }

            public string? category { get; set; }

            public DateTime? DatoTid { get; set; } = DateTime.Now;
        }

        [BindProperty]
        public Issue? Henvendelse { get; set; } = new Issue
        {
            HenvendelseID = "",
            Beskrivelse = "",
            Telefon = "",
            category = ""
           
         
        };

        [BindProperty]
        public Bruger? KontaktPerson { get; set; } = new Bruger
        {
            BrugerID = "",
            Email = "",
            Navn = ""
        };

        
    }
}
