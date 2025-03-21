using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;

namespace AthenaHealthConnect;

public class FhirSample
{
    public const string Athena22Server = "https://ap22sandbox.fhirapi.athenahealth.com/demoAPIServer";
    public const string Athena23Server = "https://ap23sandbox.fhirapi.athenahealth.com/demoAPIServer";
    public const string AthenaServer = Athena22Server;
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
            //await FetchAndPrintPatientAsync(client, patientId);
            var resource = await client.ReadAsync<Patient>(ResourceIdentity.Build("Patient", patientId));

            // Now let's add some search parameters and do another search
            SearchParams q = new SearchParams().Where("patient.id=" + patientId);
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

    // Retrieves the Patient resource and prints basic patient info.
    private static async System.Threading.Tasks.Task FetchAndPrintPatientAsync(FhirClient client, string patientId)
    {
        try
        {
            Patient patient = await client.ReadAsync<Patient>($"Patient/{patientId}");
            Console.WriteLine("Patient Resource:");
            Console.WriteLine($"- ID: {patient.Id}");

            // Safely display patient's name if available.
            if (patient.Name != null && patient.Name.Any())
            {
                var name = patient.Name.First();
                string givenNames = name.Given != null ? string.Join(" ", name.Given) : "";
                Console.WriteLine($"- Name: {givenNames} {name.Family}");
            }
            else
            {
                Console.WriteLine("- Name: Not available");
            }

            Console.WriteLine($"- Gender: {patient.Gender}");
            Console.WriteLine($"- BirthDate: {patient.BirthDate}");

            // Optional: Print full JSON
            // string json = new FhirJsonSerializer().SerializeToString(patient);
            // Console.WriteLine("Patient JSON:");
            // Console.WriteLine(json);
        }
        catch (FhirOperationException foe)
        {
            Console.WriteLine($"Error retrieving Patient/{patientId}: {foe.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"General error retrieving Patient/{patientId}: {ex.Message}");
        }
        Console.WriteLine();
    }
}
