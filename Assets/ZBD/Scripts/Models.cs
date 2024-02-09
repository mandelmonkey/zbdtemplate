using System.Collections;
using System.Collections.Generic;
namespace ZBD
{
    public class Models
    {

        public class RewardsResponse
        {
            public long currentTimePlayed;
            public long currentSats;
            public long currentRequiredTime;
            public bool validated;
            public bool whitelisted;
            public bool blacklisted;
            public bool error;
            public string message;
        }

        public class WithdrawResponse
        {
            public bool success;
            public string message;
            public long currentTimePlayed;
            public long currentSats;
            public long currentRequiredTime;
        }

    }
}