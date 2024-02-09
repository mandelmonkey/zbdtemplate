using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Beamable.Server;
using Beamable.Common;

public static class QuagoController
{


    public static async Task<string> GetQuagoAccessToken(string username, string password, string clientId)
    {

        AuthRequest request = new AuthRequest
        {
            AuthFlow = "USER_PASSWORD_AUTH",
            AuthParameters = new AuthParameters
            {
                USERNAME = username,
                PASSWORD = password,
            },
            ClientId = clientId
        };

        string jsonData = JsonConvert.SerializeObject(request);


        HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/x-amz-json-1.1");


        HttpClient httpClient = new HttpClient();
        string url = "https://cognito-idp.us-east-1.amazonaws.com/";

        httpClient.DefaultRequestHeaders.Add("X-Amz-Target", "AWSCognitoIdentityProviderService.InitiateAuth");


        var response = await httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            BeamableLogger.Log(responseBody);

            QuagoAccessTokenResponse parsedObject = JsonConvert.DeserializeObject<QuagoAccessTokenResponse>(responseBody);

            return parsedObject.AuthenticationResult.AccessToken; // or any other processing you'd like to do with the response
        }
        else
        {
            BeamableLogger.Log("error getting access token");
            return $"Error: {response.StatusCode}"; // handle errors appropriately for your use-case
        }
    }






    public static async Task<PlayerInfo> GetQuagoData(string user, string quagoUsername, string quagoPassword, string quagoClientId, string quagoAppToken)
    {
        PlayerInfo playerInfo = new PlayerInfo();
        string appToken = quagoAppToken;
        string accessToken = await GetQuagoAccessToken(quagoUsername, quagoPassword, quagoClientId);
        string url = "https://inference.quago.io/user/" + user + "?app_token=" + appToken;
        BeamableLogger.Log("getting quago data " + url);
        BeamableLogger.Log("quago access token " + accessToken);
        HttpClient httpClient = new HttpClient();

        string authHeader = "Bearer " + accessToken;

        httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);

        BeamableLogger.Log("quago header " + authHeader);
        var response = await httpClient.GetAsync(url);

        BeamableLogger.Log("quago response code " + response.StatusCode + "");
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                BeamableLogger.Log($"Quago response {responseBody}");
                playerInfo = JsonConvert.DeserializeObject<PlayerInfo>(responseBody);
                playerInfo.success = true;

                return playerInfo;
            }
            catch (Exception e)
            {
                playerInfo.success = false;
                playerInfo.error = e.ToString();
                BeamableLogger.LogError($"Quago error 1: {e.ToString()}");
                return playerInfo;
            }
        }
        else
        {
            playerInfo.success = false;

            try
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                BeamableLogger.LogError($"Quago error 2.1 : {httpClient.DefaultRequestHeaders} ");
                BeamableLogger.LogError($"Quago error 2.2 : {httpClient.DefaultRequestHeaders.Authorization} ");
                BeamableLogger.LogError($"Quago error 2 : {responseBody} ");
                playerInfo.error = responseBody;
            }
            catch (Exception e)
            {
                playerInfo.error = e.ToString();
            }

            BeamableLogger.LogError($"Quago error 3: {response.StatusCode} ");
            return playerInfo;
        }
    }
}


