/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;
using System.Collections.Generic;

public class QuagoNetworkModule
{
    protected Logger LOG = new Logger("QuagoNetworkModule");
    protected readonly QuagoNetworkSettings quagoNetworkSettings;
    protected readonly HandlerThread handlerThread;
    protected readonly HandlerThread.Handler handler;

    internal enum COMMAND : int
    {
        ADD = 1, SEND = 2
    }

    public void HandlerException(string threadName, Exception exception)
    {
        if (Logger.AskForPermission(QuagoSettings.LogLevel.ERROR))
            LOG.E("start", "Thread = " + threadName, exception);
    }

    public QuagoNetworkModule(QuagoNetworkSettings quagoNetworkSettings)
    {
        this.quagoNetworkSettings = quagoNetworkSettings;

        /* Creating Handler and Thread */
        handlerThread = new HandlerThread(
            "NetworkManagerThread",
            handler = new ReportHandler(this, quagoNetworkSettings),
            HandlerException
        );
    }

    /**
     * Adds a {@link QuagoNetworkRequest} into our Queue then asks to send it.
     *
     * @param container
     */
    private void Add(QuagoNetworkRequest container)
    {
        handlerThread.SendMessage(
            Message.Obtain(
                handler,
                (int)COMMAND.ADD, 0, 0,
                container
                )
            );
    }

    /**
     * Send request, can be done with a delay in milliseconds.
     *
     * @param delayMillis the requested delay in milliseconds, 0 = immediate
     */
    protected void Send(long delayMillis)
    {
        handlerThread.SendEmptyMessageDelayed((int)COMMAND.SEND, delayMillis);
    }

    public void ReleaseModule()
    {
        if (handlerThread == null) return;
        handlerThread.RemoveAllMessages();
        handlerThread.QuitSafely();
    }

    public void SendPayload(string payload)
    {
        Add(new QuagoNetworkRequestJson(quagoNetworkSettings, payload));
    }

    /*.......................................Handler.Methods......................................*/

    protected internal class ReportHandler : HandlerThread.Handler
    {
        protected Logger LOG;
        protected Queue<QuagoNetworkRequest> deque = new();
        protected QuagoNetworkModule NM;
        protected bool sending = false;
        protected QuagoNetworkSettings config;

        public ReportHandler(QuagoNetworkModule network, QuagoNetworkSettings config)
        {
            NM = network;
            this.config = config;
            LOG = new Logger("ReportHandler");
        }

        public void HandleRespond(int responseCode, QuagoNetworkRequest request)
        {
            try
            {
                LOG.D("handleRespond", "Response Code = {0}", responseCode);
                switch (responseCode)
                {
                    case 0:
                    /* Network timeout (no response) = RETRY */
                    case 502:
                    case 503:
                    case 504:
                        /* RETRY */
                        if (request.IsPastDeadline())
                        {
                            request.OnFailure(responseCode);
                            /* Drop the msg and stop sending */
                            CompleteSendingMsg();
                            LOG.D("handleRespond", "Message reached deadline!");
                        }
                        else
                        {
                            /* Keep retrying */
                            request.PrepareRetry();
                            NM.Send(request.GetRandomizedInterval());
                        }
                        return;
                    default:
                        /* Drop the msg and stop sending*/
                        request.OnSuccess(responseCode);
                        CompleteSendingMsg();
                        break;
                }
            }
            catch (Exception e)
            {
                LOG.E("handleRespond", e);
                request.OnException(e);
                /* Drop the msg and stop sending */
                CompleteSendingMsg();
            }
        }

        protected void CompleteSendingMsg()
        {
            /* Drop the msg and stop sending */
            sending = false;
            deque.Dequeue();
            /* Send next msg */
            NM.Send(0);
        }

        public override void HandleMessage(Message msg)
        {
            try
            {
                switch ((COMMAND)msg.What)
                {
                    case COMMAND.ADD:
                        /* Try to Add */
                        LOG.D("handleMessage", "ADD");

                        QuagoNetworkRequest addContainer = (QuagoNetworkRequest)msg.Obj;
                        if (deque.Count >= config.MaxCachedMessages)
                        {
                            LOG.W("handleMessage",
                                "[!] Warning: Ignoring adding the package to queue due to reaching the limit!"
                            );
                            break;
                        }

                        deque.Enqueue(addContainer);
                        if (!sending)
                        {
                            /* If not sending then try to send */
                            sending = true;
                            NM.Send(0);
                        }
                        break;

                    case COMMAND.SEND:
                        /* Try to send */
                        if (deque.Count == 0)
                            /* Nothing to send so exit */
                            break;
                        QuagoNetworkRequest sendContainer = deque.Peek();
                        if (sendContainer == null)
                            /* Nothing to send so exit */
                            break;

                        if (sendContainer.GetRetryNumber() == 0)
                            sendContainer.StartTimer();
                        else
                            /* Update headers with number of retries */
                            sendContainer.SetHeader(
                                "X-Retry-Num", sendContainer.GetRetryNumber().ToString()
                            );

                        LOG.D("handleMessage", "SEND");

                        /* Execute Call and receive the server response */
                        int responseCode = new NetworkCall().execute(sendContainer);

                        /* Handle the server response */
                        HandleRespond(responseCode, sendContainer);
                        break;

                    default:
                        LOG.E("handleMessage",
                            "[!] Error: Received unknown msg type = {0}", msg.What
                        );
                        break;
                }
            }
            catch (Exception e)
            {
                LOG.E("handleMessage", e);
            }
        }
    }
}
#endif