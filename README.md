
This sample shows how to connect using a Managed Service Principal in Azure Active directory, protected by a client secret, in order to run unattended queries against Neo4j


## Managed Service Principals from Azure Active Directory in Neo4j

### This guide will demonstrate how to configure Azure Active Directory (AAD) and Neo4j such that 

- user accounts are authenticated using an interactive flow (user confirms using browser) 
- service accounts can run unattended, using a certificate or client secret to obtain a token as part of an automated flow

### High-level requirements
The following steps are required

- Create an Enterprise Application in AAD 
- Implement OIDC settings for Neo4j to accept the AAD Enterprise Application (EA) as an identity provider per the documentation
- Test that the integration works in Neo4j Browser
- Check that the security.log entries for AAD users are as expected (e.g. user@company.com)
- Create a Managed Service Principal (MSP) in the same AAD domain.
- Adjust the application manifest of the EA to accept the ‘Application’ member type
- Define an App Registration for the MSP to access the EA
- Define a certificate or secret to allow access to the MSP (certificate is recommended)
-- Store the client_id and client_secret
- Add a bespoke claim to the App Registration (cannot conflict with reserved names)
- Set the bespoke claim value to be a string value (e.g. “Neo4j-ServicePrincipal”) IF the user.mail field is empty, or the user.mail field if it is not. The output of this custom claim will be the username in Neo4j.
- In application code, obtain a JWT token by making a REST call to the AAD https://login.microsoftonline.com/<your-tenancy-id>//oauth2/token endpoint
- Extract the JWT token from the JSON response, then use it in Authtokens.Bearer on the Neo4j driver API to authenticate & authorise using the token.


