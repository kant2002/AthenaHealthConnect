using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace AthenaHealthConnect
{
    public class FhirSample
    {
        public const string AthenaServer = "https://ap22sandbox.fhirapi.athenahealth.com/demoAPIServer";
        public static async System.Threading.Tasks.Task Test(string server, string token)
        {
            //using Hl7.Fhir.Model;
            //using Hl7.Fhir.Rest;

            FhirClientSettings clientSettings = new FhirClientSettings();
            clientSettings.PreferredFormat = ResourceFormat.Json;
            clientSettings.PreferredParameterHandling = SearchParameterHandling.Strict;
            // Change the above, or check and set any other settings you want


            // Choose your preferred FHIR server or add your own
            // More at https://confluence.hl7.org/display/FHIR/Public+Test+Servers

            FhirClient client = new FhirClient(server, clientSettings, new AuthorizationMessageHandler() {  AuthorizationToken = token });
            // FhirClient client = new FhirClient("http://hapi.fhir.org/baseR4", clientSettings);
            // FhirClient client = new FhirClient("http://test.fhir.org/r4", clientSettings);

            try
            {
                // First we try a search for a resource type, without using any search parameters,
                // but limiting to max 5 entries in the result Bundle

                Bundle results = await client.SearchAsync<Observation>(null, null, 5);

                if (results.Entry == null) Console.WriteLine("No results found");

                foreach (var entry in results.Entry)
                {
                    if (entry.Resource is Observation)
                        Console.WriteLine($"Found observation with id {entry.Resource.Id} and summary '{((Observation)entry.Resource).Text?.Div}'");
                    else
                        Console.WriteLine($"Found unexpected resource type: {entry.Resource.TypeName}'");
                }

                // Now let's add some search parameters and do another search
                SearchParams q = new SearchParams().Where("name=steve");
                //SearchParams q = new SearchParams().Where("name=steve").Where("birthdate=1974-12-25").LimitTo(5);

                results = await client.SearchAsync<Patient>(q);

                // This time continue asking for the next bundle while there are more results on the server
                while (results != null)
                {
                    if (results.Entry == null) Console.WriteLine("No results found");

                    foreach (var entry in results.Entry)
                    {
                        if (entry.Resource is Patient)
                            Console.WriteLine($"Found patient with id {entry.Resource.Id} and summary '{((Patient)entry.Resource).Text?.Div}'");
                        else
                            Console.WriteLine($"Found unexpected resource type: {entry.Resource.TypeName}'");
                    }

                    // get the next page of results
                    results = client.Continue(results);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine($"An error has occurred: {err.Message}");
            }

        }

        public static async System.Threading.Tasks.Task FindPatientById(string server, string token, string patientId)
        {
            //using Hl7.Fhir.Model;
            //using Hl7.Fhir.Rest;

            FhirClientSettings clientSettings = new FhirClientSettings();
            clientSettings.PreferredFormat = ResourceFormat.Json;
            clientSettings.PreferredParameterHandling = SearchParameterHandling.Strict;
            // Change the above, or check and set any other settings you want


            // Choose your preferred FHIR server or add your own
            // More at https://confluence.hl7.org/display/FHIR/Public+Test+Servers

            FhirClient client = new FhirClient(server, clientSettings, new AuthorizationMessageHandler() { AuthorizationToken = token });
            // FhirClient client = new FhirClient("http://hapi.fhir.org/baseR4", clientSettings);
            // FhirClient client = new FhirClient("http://test.fhir.org/r4", clientSettings);

            try
            {
                // Now let's add some search parameters and do another search
                SearchParams q = new SearchParams().Where("patient=" + patientId);
                //SearchParams q = new SearchParams().Where("name=steve").Where("birthdate=1974-12-25").LimitTo(5);

                Bundle results = await client.SearchAsync<Patient>(q);

                if (results.Entry == null) Console.WriteLine("No results found");

                // This time continue asking for the next bundle while there are more results on the server
                while (results != null)
                {
                    if (results.Entry == null) Console.WriteLine("No results found");

                    foreach (var entry in results.Entry)
                    {
                        if (entry.Resource is Patient)
                            Console.WriteLine($"Found patient with id {entry.Resource.Id} and summary '{((Patient)entry.Resource).Text?.Div}'");
                        else
                            Console.WriteLine($"Found unexpected resource type: {entry.Resource.TypeName}'");
                    }

                    // get the next page of results
                    results = client.Continue(results);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine($"An error has occurred: {err.Message}");
            }

        }
    }
}
