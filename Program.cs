﻿using Neo4j.Driver;
using System.Text.Json;

var uri = Environment.GetEnvironmentVariable("NEO4J_URI");
var loginUri =
    Environment.GetEnvironmentVariable(
        "NEO4J_AAD_LoginUri"); // this is the MS API to obtain a token, e.g. https://login.microsoftonline.com/<your-tenancy-id>/oauth2/token 
var clientId =
    Environment.GetEnvironmentVariable("NEO4J_ServicePrincipal_ClientId"); // the client secret id for the MSP
var clientSecret =
    Environment.GetEnvironmentVariable("NEO4J_ServicePrincipal_ClientSecret"); // the client secret value for the MSP
var resource =
    Environment.GetEnvironmentVariable(
        "NEO4J_AAD_Application_Scope"); // the scope for the token request e.g. <your enterprise application id>/Token.Connect

/* Using a stored password (Client Secret) for the ServicePrincipal, obtain a jwt token to pass to Neo4j 
NOTE: MS recommend to use a certificate for Production deployments */

var client = new HttpClient();
var request = new HttpRequestMessage(HttpMethod.Post, loginUri);

if (clientSecret != null && clientId != null && resource != null)
{
    var collection = new List<KeyValuePair<string, string>>
    {
        /* Build the request to the AAD Login uri */

        new("grant_type", "client_credentials"),
        new("client_id", clientId),
        new("client_secret", clientSecret),
        new("resource", resource)
    };
    var content = new FormUrlEncodedContent(collection);

    request.Content = content;
}


/* Make the login API call to obtain a jwt access token, and extract it from the result json */

var response = await client.SendAsync(request);
response.EnsureSuccessStatusCode();
string resultJson = await response.Content.ReadAsStringAsync();
var accessToken = JsonDocument.Parse(resultJson).RootElement.GetProperty("access_token").GetString();

// for debugging // Console.WriteLine("token: {0}",accessToken);

/* using accessToken, run a query */

var driver = GraphDatabase.Driver(uri, AuthTokens.Bearer(accessToken));

var records = await driver.ExecutableQuery("MATCH(m:Movie) return m.title as moviename LIMIT 1;").ExecuteAsync();

Console.WriteLine(records.Result[0].Values["moviename"]);