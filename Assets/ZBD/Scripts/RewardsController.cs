using System;
using System.Collections;
using System.Collections.Generic;
using Beamable;
using Beamable.Server.Clients;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Newtonsoft.Json;
using static ZBD.Models;
using Beamable.Common;

namespace ZBD
{
    public class RewardsController : MonoBehaviour
    {
        public bool debug;
        public Text satsLabel;
        public Text satsConvertedLabel;
        public TMP_Text countdownLabel;
        public TMP_Text responseLabel;
        public TMP_Text debugModeLabel;
        int checkTime = 60;
        private float currentTime;
        public String appToken;

        [HideInInspector]
        string userId;

        public RewardsResponse currentStats;

        public GameObject infoPanel;
        public GameObject successPanel;
        public GameObject errorPanel;
        public Text errorPanelText;

        public GameObject withdrawPanel;
        public Text withdrawPanelSats;

        public Slider rewardsSlider;
        public Text rewardsSliderInfo;

        public InputField emailField;

        public GameObject testButton;

        public GameObject withdrawButton;
        string beamableGamerTag;

        public Text beamableGamerTagLabel;
        Coroutine checkLoop;
        private void Awake()
        {
            if (!debug)
            {
                countdownLabel.gameObject.SetActive(false);
                responseLabel.gameObject.SetActive(false);
                testButton.gameObject.SetActive(false);
                debugModeLabel.gameObject.SetActive(false);
            }
        }

        public void ShowInfo()
        {
            infoPanel.SetActive(true);
        }

        public void CloseInfo()
        {
            infoPanel.SetActive(false);
        }
        void Start()
        {
            userId = PlayerPrefs.GetString("userId", "");

            if (userId.Length < 1)
            {
                userId = Utils.Instance.ComputeSha256Hash(System.Guid.NewGuid().ToString());

                PlayerPrefs.SetString("userId", userId);
            }


            StartQuago();

            countdownLabel.text = "";

            Invoke("CheckTimePlayed", 5);

            GetStats();

            emailField.text = PlayerPrefs.GetString("email", "");

            GetBeamableGamerTag();
        }

        void StartQuago()
        {
            /*
             If using Quago for cheat detection make sure the appToken has been set
             If not you can remove this function
            */
            if (appToken.Length == 0)
            {
                Debug.LogError("app token not set in inspector, if you are not using Quago for anti cheat, remove StartQuago function");
                return;
            }
            Quago.initialize(QuagoSettings.create(appToken, QuagoSettings.QuagoFlavor.PRODUCTION).setLogLevel(QuagoSettings.LogLevel.INFO)
);


            Quago.beginTracking(userId);
        }

        /*This is a user id on beamable we can use to look up the user on their dashboard*/
        public async void GetBeamableGamerTag()
        {
            try
            {
                var ctx = BeamContext.Default;
                await ctx.OnReady;
                beamableGamerTag = ctx.Api.User.id + "";
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                beamableGamerTag = "error getting beamable id";
            }
            beamableGamerTagLabel.text = "id: " + beamableGamerTag;

        }

        public void Test()
        {
            currentTime = 0;
            CheckTimePlayed();
        }


        public void ShowEarnings()
        {
            if (currentStats != null && currentStats.blacklisted)
            {
                ShowError("We need to verify your account first\nPlease contact support with id:" + beamableGamerTag);
                return;
            }


            withdrawPanel.SetActive(true);


        }

        void ResetTimePlayed()
        {
            currentTime = checkTime;
            if (checkLoop != null)
            {
                StopCoroutine(checkLoop);
            }
            checkLoop = StartCoroutine(CountdownRoutine());
        }

        void ShowError(string message)
        {
            errorPanel.SetActive(true);
            errorPanelText.text = message;
        }



        void ShowSuccess()
        {
            successPanel.SetActive(true);
        }



        void CheckTimePlayed()
        {
            countdownLabel.text = "updating...";
            try
            {

                SendToGameServer(userId);

            }
            catch (Exception e)
            {
                Debug.LogError("catch " + e);
                responseLabel.text = e + "";
                ResetTimePlayed();
            }

        }



        void ResetStatsLabels()
        {
            satsLabel.text = "0";
            withdrawPanelSats.text = "0";
            rewardsSliderInfo.text = "";
            satsConvertedLabel.text = "";
        }
        void SetStats()
        {

            satsLabel.text = currentStats.currentSats + "";
            satsConvertedLabel.text = "₿" + ((float)currentStats.currentSats / 100000000f).ToString("0.############");
            withdrawPanelSats.text = currentStats.currentSats + "";

            float currentEarningPercent = (float)currentStats.currentTimePlayed / (float)currentStats.currentRequiredTime;
            if (currentEarningPercent > 1)
            {
                currentEarningPercent = 1;
            }
            rewardsSlider.value = currentEarningPercent;
            long timePlayedCorrected = currentStats.currentTimePlayed;
            if (timePlayedCorrected > currentStats.currentRequiredTime)
            {
                timePlayedCorrected = currentStats.currentRequiredTime;
            }
            rewardsSliderInfo.text = FormatRewardsSliderInfo(timePlayedCorrected, currentStats.currentRequiredTime);

            if (currentStats.error)
            {
                responseLabel.text = currentStats.message;
            }
            else
            {
                responseLabel.text = currentStats.currentTimePlayed + "/" + currentStats.currentRequiredTime + " mins\n" + currentStats.currentSats + " sats";

            }
        }
        async void GetStats()
        {

            var ctx = BeamContext.Default;
            await ctx.OnReady;
            var result = await ctx.Microservices().GameServer().GetStats();
            currentStats = JsonConvert.DeserializeObject<RewardsResponse>(result);
            SetStats();
        }

        public void DownloadZBD()
        {
            Application.OpenURL("https://zbd.gg");
        }

        public void WithdrawBitcoin()
        {

            string email = emailField.text;

            if (email.Length == 0)
            {
                ShowError("please enter your ZBD email");
                return;
            }
            withdrawButton.SetActive(false);


            try
            {

                ContinueWithdraw(email);

            }
            catch (Exception e)
            {
                withdrawButton.SetActive(true);
                ShowError("error");
                Debug.LogError(e);

            }

        }

        async void ContinueWithdraw(string email)
        {

            var ctx = BeamContext.Default;
            await ctx.OnReady;
            var result = await ctx.Microservices().GameServer().WithdrawBitcoin(email);

            WithdrawResponse res = JsonConvert.DeserializeObject<WithdrawResponse>(result);

            if (!res.success)
            {
                ShowError(res.message);
            }
            else
            {
                PlayerPrefs.SetString("email", email);
                ShowSuccess();
                satsLabel.text = "0";
                withdrawPanelSats.text = "0";
                rewardsSliderInfo.text = FormatRewardsSliderInfo(0, res.currentRequiredTime);
            }
            withdrawButton.SetActive(true);
        }

        string FormatRewardsSliderInfo(long currentTime, long requiredTime)
        {
            return currentTime + "/" + requiredTime + " mins approx";
        }
        async void SendToGameServer(string deviceIdd)
        {
            var ctx = BeamContext.Default;
            await ctx.OnReady;
            try
            {
                var result = await ctx.Microservices().GameServer().SendPlaytime(userId, PlaytimeTracker.Instance.GetTotalPlayTimeMins());

                currentStats = JsonConvert.DeserializeObject<RewardsResponse>(result);
                Debug.Log("response " + result);

                SetStats();

                ResetTimePlayed();

            }
            catch (Exception e)
            {

                Debug.LogError(e);
                responseLabel.text = e.ToString();

            }
        }

        IEnumerator CountdownRoutine()
        {
            while (currentTime >= 0)
            {
                yield return new WaitForSeconds(1f);


                currentTime--;

                if (currentTime == 0)
                {
                    CheckTimePlayed();
                }
                else if (currentTime > 0)
                {
                    countdownLabel.text = currentTime.ToString("0");
                }
            }
        }

    }
}
