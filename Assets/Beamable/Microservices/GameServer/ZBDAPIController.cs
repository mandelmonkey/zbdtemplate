using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using Beamable.Server;
using Beamable.Common;

public static class ZBDAPIController
{
    public static async Task<WithdrawResponse> SendToEmail(string email, int amount, string description, string apikey)
    {

        SendToEmailPayload request = new SendToEmailPayload();
        request.email = email;
        request.amount = (amount * 1000) + "";
        request.comment = description;

        string jsonData = JsonConvert.SerializeObject(request);

        BeamableLogger.Log("send to email payload: " + jsonData);

        HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");


        HttpClient httpClient = new HttpClient();
        string url = "https://api.zebedee.io/v0/email/send-payment";

        httpClient.DefaultRequestHeaders.Add("apikey", apikey);


        var response = await httpClient.PostAsync(url, content);
        WithdrawResponse sendResponse = new WithdrawResponse();
        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            BeamableLogger.Log("responseBody", responseBody);
            sendResponse = JsonConvert.DeserializeObject<WithdrawResponse>(responseBody);
            return sendResponse;
        }
        else
        {
            BeamableLogger.Log("error sending to email");
            sendResponse.success = false;
            try
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                sendResponse.message = responseBody;

            }
            catch (Exception e)
            {
                sendResponse.message = e.ToString();
            }

            return sendResponse;
        }
    }

}


