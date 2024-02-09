/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
public class QuagoNetworkRequestJson : QuagoNetworkRequest
{
    private readonly string jsonString;

    public QuagoNetworkRequestJson(QuagoNetworkSettings config, string jsonString)
        : base(config, "POST", false, false, true)
    {
        this.jsonString = jsonString;
    }

    public override string GenerateBody()
    {
        return jsonString;
    }
}
#endif