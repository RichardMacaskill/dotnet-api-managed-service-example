using Neo4j.Driver;
using System.Text.Json;

var uri = Environment.GetEnvironmentVariable("NEO4J_URI");
var loginUri = Environment.GetEnvironmentVariable("NEO4J_AAD_LoginUri");
var clientId = Environment.GetEnvironmentVariable("NEO4J_ServicePrincipal_ClientId");
var clientSecret = Environment.GetEnvironmentVariable("NEO4J_ServicePrincipal_ClientSecret");
var resource = Environment.GetEnvironmentVariable("NEO4J_AAD_Application_Scope");

/* Using a stored password (Client Secret) for the ServicePrincipal, obtain a jwt token to pass to Neo4j 
NOTE: it is recommended to use a certificate for Production deployments */

var client = new HttpClient();
var request = new HttpRequestMessage(HttpMethod.Post, loginUri);
var collection = new List<KeyValuePair<string, string>>();

/* Build the request to the AAD Login uri */

collection.Add(new("grant_type", "client_credentials"));
collection.Add(new("client_id", clientId));
collection.Add(new("client_secret", clientSecret));
collection.Add(new("resource", resource));
var content = new FormUrlEncodedContent(collection);

request.Content = content;

/* Make the login API call to obtain a jwt access token, and extract it from the result json */

var response = await client.SendAsync(request);
response.EnsureSuccessStatusCode();
String resultJson = await response.Content.ReadAsStringAsync();
var accessToken = JsonDocument.Parse(resultJson).RootElement.GetProperty("access_token").GetString();

// for debugging // Console.WriteLine("token: {0}",accessToken);

/* using accessToken, run a query */

var driver = GraphDatabase.Driver(uri, AuthTokens.Bearer(accessToken));

var records = await driver.ExecutableQuery("MATCH(m:Movie) return m.title as moviename LIMIT 1;").ExecuteAsync();

Console.WriteLine(records.Result[0].Values["moviename"]);