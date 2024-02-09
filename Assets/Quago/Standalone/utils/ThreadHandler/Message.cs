/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System.Text;

public class Message
{
    public Message() : this(0, 0, 0, null)
    {
    }

    public Message(int What) : this(What, 0, 0, null)
    {
    }

    public Message(int What, int Arg1, int Arg2, object Obj)
    {
        this.What = What;
        this.Arg1 = Arg1;
        this.Arg2 = Arg2;
        this.Obj = Obj;
    }

    public int What { get; set; }
    public int Arg1 { get; set; }
    public int Arg2 { get; set; }
    public object Obj { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        sb.Append("What = ");
        sb.Append(What);
        sb.Append(", Arg1 = ");
        sb.Append(Arg1);
        sb.Append(", Arg2 = ");
        sb.Append(Arg2);
        sb.Append(", Obj = ");
        if (Obj == null)
            sb.Append("Null");
        else
            sb.Append(Obj.ToString());
        sb.Append("}");
        return sb.ToString();
    }

    /* ToDo: replace new Message() with recycling mechanism */
    public static Message Obtain(HandlerThread.Handler handler)
    {
        return new Message();
    }

    public static Message Obtain(HandlerThread.Handler handler, int What)
    {
        return new Message(What);
    }

    public static Message Obtain(HandlerThread.Handler handler, int What, int Arg1, int Arg2, object Obj)
    {
        return new Message(What, Arg1, Arg2, Obj);
    }
}
#endif