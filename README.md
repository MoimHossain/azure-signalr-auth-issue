# azure-signalr-auth-issue

An example project to share code with the team to diagnose the issue with Azure signalR authentication


# Issue description

I have this API project (written with .net core 2.1) that hosts Azure SignalR. This project acts as an API server for the product we are building. 

We have a client application (an Azure Web site), that's written with ReactJS and uses MSAL (Microsoft Authentication Library) and a Azure AD v2 application to sign-in users. (The signin happens in the browser and we receive Access token for this API project).

The code in client side roughly looks like following:

```
        let msal = new Msal.UserAgentApplication(
            this.getAadClientId(),
            Aad,
            this.authCallback.bind(this),
            {
                redirectUri: Environments.getEnvironment().redirectUri,
                cacheLocation: 'localStorage',
                logger: this.createLogger(),
                storeAuthStateInCookie: true,
                state: "12345",
                navigateToLoginRequestUrl: true
            }
        );

        // login
        msal.loginRedirect(['User.Read']);
    
```

And then after user successfully signed in, we collect the access token for the API project.

```
    /**
     * Function authCallback gets invoked by MSAL right after loginRedirect 
     * and acquireTokenRedirect. Use tokenType to determine the context.
     * loginRedirect yields tokenType = "id_token" and acquireTokenRedirect yields
     * tokenType = 'access_token'.   
     */
    authCallback = (errorDesc, token, error, tokenType, state) => {
        if (token) {
            let scopes = ['https://<MY AAD TENENT NAME>.onmicrosoft.com/<MY APP NAME>/access_as_user'];
            this.setUserContext(token);
            if (!msal) {
                msal = this.createMsalObject();
            }
            msal.acquireTokenSilent(scopes).then((accessToken) => {
                // do nothing here - token is cached by MSAL now
            }, (errorInSilentMode) => {
                console.error(errorInSilentMode);
                msal.acquireTokenRedirect(scopes);
            });
        }
    };
```

Now we have the access token to talk to our API. (the project in this repo)

The client side implementation to connect to SignalR is like:

```
class SignalRService {
    bindConnectionMessage = (connection) => {
        var messageCallback = function (message) {
            console.log('we have got a message.. ', message);
        };
        connection.on('sayHello', messageCallback);
    }

    createConnection = () => {
        let uri = `${Environments.getEnvironment().ApiBaseUri}taskhub`;
        this.connection = new SignalR.HubConnectionBuilder()
            .withUrl(uri, {
                accessTokenFactory: () => SecurityService.getApiAccessToken()
            })
            .configureLogging(SignalR.LogLevel.Error)
            .build();
        this.bindConnectionMessage(this.connection);

        this.connection.start()
            .then(() => {
                // We have connection established
            })
            .catch((error) => {

            });
    }

```

I couldn't have included the AD app ID and more details. Both of the client App and API app is running as Azure web app.

## The issues 

- The Hub's ```OnConnectedAsync``` doesn't get called when there's ```Authorize``` attribute in it. (However, it doesn't happend all the times - I would say most of the times.)