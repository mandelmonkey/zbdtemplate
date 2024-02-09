using Beamable.Server;
using Newtonsoft.Json;
using System;
using Beamable.Server.Api.RealmConfig;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.IO;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Api;
using System.Text;
using System.Collections.Generic;
using Beamable.Api.Autogenerated.Models;
using Beamable.Common.Inventory;

namespace Beamable.Microservices
{

    [Microservice("GameServer")]
    public class GameServer : Microservice
    {
        string satsKey = "currency.sats";
        string rewardedTimeKey = "currency.rewardedTime";
        string whitelistedId = "items.whitelisted";
        string validatedId = "items.validated";
        string blacklistedId = "items.blacklisted";

        [ClientCallable]
        public async Task<string> GetStats()
        {
            RewardsResponse rewardRes = new RewardsResponse();
            long currentSats = await Services.Inventory.GetCurrency(satsKey);
            long currentRewardedTime = await Services.Inventory.GetCurrency(rewardedTimeKey);
            RealmConfig config = await Services.RealmConfig.GetRealmConfigSettings();

            long rewardTime = long.Parse(config.GetSetting("accounts", "rewardTime", "5"));

            rewardRes.currentSats = currentSats;
            rewardRes.currentTimePlayed = currentRewardedTime;
            rewardRes.currentRequiredTime = rewardTime;
            return JsonConvert.SerializeObject(rewardRes);
        }



        [ClientCallable]
        public async Task<string> SendPlaytime(string userId, long totalPlaytimeMins)
        {
            BeamableLogger.Log("userId " + userId + " playtime " + totalPlaytimeMins);
            RewardsResponse rewardRes = new RewardsResponse();

            try
            {
                bool isWhiteListed = await IsWhitelisted();
                bool isBlackListed = await IsBlacklisted();
                long currentSats = await Services.Inventory.GetCurrency(satsKey);
                long currentRewardedTime = await Services.Inventory.GetCurrency(rewardedTimeKey);

                rewardRes.whitelisted = isWhiteListed;
                rewardRes.currentSats = currentSats;
                rewardRes.currentTimePlayed = currentRewardedTime;
                rewardRes.blacklisted = isBlackListed;

                if (isBlackListed && isWhiteListed)
                {
                    rewardRes.error = true;
                    rewardRes.message = "please contact support";
                    return JsonConvert.SerializeObject(rewardRes);
                }



                // Get Config constants and check if they are set correctly

                RealmConfig config = await Services.RealmConfig.GetRealmConfigSettings();

                long rewardTime = long.Parse(config.GetSetting("accounts", "rewardTime", "15"));
                rewardRes.currentRequiredTime = rewardTime;

                int rewardAmount = int.Parse(config.GetSetting("accounts", "rewardAmount", "10"));
                string missingConfigs = "Please add the following to your beamable Config ";
                bool isMissingConfig = false;
                string quagoUsername = config.GetSetting("accounts", "quagoUsername", "");
                string quagoPassword = config.GetSetting("accounts", "quagoPassword", "");
                string quagoClientId = config.GetSetting("accounts", "quagoClientId", "");
                string quagoAppToken = config.GetSetting("accounts", "quagoAppToken", "");
                string zbdApiKey = config.GetSetting("accounts", "zbdApiKey", "");



                if (zbdApiKey == "")
                {
                    isMissingConfig = true;
                    missingConfigs += "zbdApiKey";
                }

                if (isMissingConfig)
                {
                    rewardRes.error = true;
                    rewardRes.message = missingConfigs;
                    return JsonConvert.SerializeObject(rewardRes);
                }

                // If not using Quago remove these checks
                if (quagoUsername == "")
                {
                    isMissingConfig = true;
                    missingConfigs += "quagoUsername ";
                }
                if (quagoPassword == "")
                {
                    isMissingConfig = true;
                    missingConfigs += "quagoPassword ";
                }
                if (quagoClientId == "")
                {
                    isMissingConfig = true;
                    missingConfigs += "quagoClientId ";
                }
                if (quagoAppToken == "")
                {
                    isMissingConfig = true;
                    missingConfigs += "quagoAppToken ";
                }

                PlayerInfo quagoData = await QuagoController.GetQuagoData(userId, quagoUsername, quagoPassword, quagoClientId, quagoAppToken);

                // Quago could probably not find an entry for the user yet
                if (quagoData.success == false)
                {
                    rewardRes.error = true;
                    rewardRes.message = quagoData.error;
                    return JsonConvert.SerializeObject(rewardRes);
                }

                long quagoPlaytimeMins = 0;
                // Quago has detected that the user has not played
                if (quagoData != null && quagoData.TotalMotionHours > 0)
                {
                    // Return an error if Quago has detected any fake gamer play or game play on an emulator
                    if (quagoData.InauthPlaytimePercentage > 0 || quagoData.EmuPlaytimePercentage > 0)
                    {
                        if (!isBlackListed)
                        {
                            await Services.Inventory.AddItem(blacklistedId);
                        }
                        rewardRes.blacklisted = true;

                        rewardRes.error = true;

                        if (quagoData.InauthPlaytimePercentage > 0)
                        {
                            rewardRes.message = "Inauthentic playing detected\n";
                        }
                        if (quagoData.EmuPlaytimePercentage > 0)
                        {
                            rewardRes.message = "Emulators are not allowed";
                        }

                        return JsonConvert.SerializeObject(rewardRes);
                    }

                    quagoPlaytimeMins = (long)Math.Floor((decimal)(quagoData.TotalMotionHours * 60f));
                }
                else
                {
                    BeamableLogger.Log("no quago data yet");
                }


                BeamableLogger.Log("total playtime mins " + totalPlaytimeMins + " quago " + quagoPlaytimeMins);






                // Calculate the current time played since the last reward, if it greater than `rewardTime` assign sats balance

                long currentPlayTimeMins = totalPlaytimeMins - currentRewardedTime;

                if (currentPlayTimeMins >= rewardTime)
                {

                    currentRewardedTime = totalPlaytimeMins;
                    currentPlayTimeMins = 0;
                    await Services.Inventory.SetCurrency(rewardedTimeKey, currentRewardedTime);


                    // If the difference between the quago calcuated playtime and out client calcualted time is less than 20 then they have probably cheated so dont add sats
                    // in this case if the difference is greater than 20 mins
                    if (totalPlaytimeMins - quagoPlaytimeMins < -20)
                    {

                        BeamableLogger.Log("play time discrepancy " + (totalPlaytimeMins - quagoPlaytimeMins));


                    }
                    else
                    {
                        currentSats += rewardAmount;
                        await Services.Inventory.SetCurrency(satsKey, currentSats);

                    }

                }


                rewardRes.currentRequiredTime = rewardTime;
                rewardRes.currentSats = currentSats;
                rewardRes.currentTimePlayed = currentPlayTimeMins;

                return JsonConvert.SerializeObject(rewardRes);

            }
            catch (Exception e)
            {

                BeamableLogger.LogError(e);

                rewardRes.error = true;
                rewardRes.message = e + "";
                return JsonConvert.SerializeObject(rewardRes);
            }
        }

        async Task<bool> IsWhitelisted()
        {
            var items = await Services.Inventory.GetItems<ItemContent>();


            foreach (var item in items)
            {
                if (item.ItemContent.Id == whitelistedId)
                {
                    return true;
                }
            }
            return false;
        }

        async Task<bool> IsBlacklisted()
        {
            var items = await Services.Inventory.GetItems<ItemContent>();


            foreach (var item in items)
            {
                if (item.ItemContent.Id == blacklistedId)
                {
                    return true;
                }
            }
            return false;
        }



        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [ClientCallable]
        public async Task<string> WithdrawBitcoin(string email)
        {


            long currentSats = await Services.Inventory.GetCurrency(satsKey);
            await Services.Inventory.SetCurrency(satsKey, 0);


            WithdrawResponse res = new WithdrawResponse();

            bool isBlackListed = await IsBlacklisted();
            if (isBlackListed)
            {
                res.success = false;
                res.message = "Please contact support";
                return JsonConvert.SerializeObject(res);
            }

            if (currentSats == 0)
            {
                res.success = false;
                res.message = "You have not earned enough bitcoin to withdraw";
                return JsonConvert.SerializeObject(res);
            }


            RealmConfig config = await Services.RealmConfig.GetRealmConfigSettings();

            string apikey = config.GetSetting("accounts", "zbdApiKey", "");

            if (apikey.Length == 0)
            {
                res.success = false;
                res.message = "zbdApiKey not set in beamable dashboard";
                await Services.Inventory.SetCurrency(satsKey, currentSats);
                return JsonConvert.SerializeObject(res);
            }

            res = await ZBDAPIController.SendToEmail(email, (int)currentSats, "Withdrawal", apikey);


            if (!res.success)
            {
                await Services.Inventory.SetCurrency(satsKey, currentSats);

            }


            long rewardTime = long.Parse(config.GetSetting("accounts", "rewardTime", "5"));

            long currentRewardedTime = await Services.Inventory.GetCurrency(rewardedTimeKey);
            res.currentRequiredTime = rewardTime;
            res.currentSats = 0;
            res.currentTimePlayed = 0;
            return JsonConvert.SerializeObject(res);

        }

    }


}
