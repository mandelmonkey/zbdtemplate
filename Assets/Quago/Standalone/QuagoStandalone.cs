/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using static QuagoMetaInformation;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

[assembly: InternalsVisibleTo("quagoEditMode")]
internal static class QuagoStandalone
{
    private const int MOUSE_WHEEL = 0;
    private const int MOUSE_BUTTON_LEFT = 1;
    private const int MOUSE_BUTTON_RIGHT = 2;
    private const int MOUSE_BUTTON_MIDDLE = 3;
    private const int MOUSE_BUTTON_FORWARD = 4;
    private const int MOUSE_BUTTON_BACKWARD = 5;

    private const string quagoGameObjectName = "QuagoMono_GameObject";
    private static System.Diagnostics.Stopwatch stopwatch = new();
    private static QuagoManager quagoManager = new();
    private static GameObject quagoGameObject = null;
    private static object _lock = new();
    private static bool _init_called = false, _initialized = false, _resolutionChanged = false;
    private const long _resolutionChangeDurationMillis = 1000L;
    private static long _resolutionChangeStartMillis = 0L;

    private static int winWidth, winHeight, scrWidth, scrHeight, scrRefreshRate;

    private static HandlerThread handlerThread;
    private static QuagoStandaloneHandler handler;
    private static QuagoSettings settings;
    //private static KeyCodeDictionary keyCodeDictionary = new();

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    private static Dictionary<string, Key> keyMap = new();
#elif ENABLE_LEGACY_INPUT_MANAGER
    private static Dictionary<string, KeyCode> keyMap = new();
#endif

    internal enum COMMAND : int
    {
        INIT,
        SEGMENT_BEGIN,
        SEGMENT_END,
        TRACKING_BEGIN,
        TRACKING_END,
        SET_KEY_VALUE,
        INIT_COMPLETE,
        SET_USER_ID,
        SET_ADDITIONAL_ID
    }

    private class QuagoMono : MonoBehaviour
    {
        protected float MouseLastX, MouseLastY;

        public void Start()
        {
            /* Tells Unity not to destroy our GameObject when Scenes change. */
            DontDestroyOnLoad(this);

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            bool mouseExists = Mouse.current != null;
#elif ENABLE_LEGACY_INPUT_MANAGER
            bool mouseExists = Input.mousePresent;
#else
            bool mouseExists = false;
#endif

            if (mouseExists)
            {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                Vector3 mousePosition = Mouse.current.position.ReadValue();
                MouseLastX = mousePosition.x;
                MouseLastY = mousePosition.y;
#elif ENABLE_LEGACY_INPUT_MANAGER
                Vector3 mousePosition = Input.mousePosition;
                MouseLastX = mousePosition.x;
                MouseLastY = mousePosition.y;
#else
                MouseLastX = MouseLastY = 0;   
#endif
            }
            else
            {
                MouseLastX = MouseLastY = 0;
            }

            /* Screen */
            scrWidth = Screen.currentResolution.width;
            scrHeight = Screen.currentResolution.height;
            scrRefreshRate = Screen.currentResolution.refreshRate;

            /* Window */
            winWidth = Screen.width;
            winHeight = Screen.height;

            /* Create MetaData on MainThread now we are ready to initialize */
            quagoManager.Initialize(settings, new());
            _initialized = true;
        }

        public void OnApplicationFocus(bool hasFocus)
        {
            if (!_initialized) return;
            if (hasFocus)
                quagoManager.onResume();
            else
                quagoManager.onPause();
        }

        public void Update()
        {
            if (!_initialized) return;

            /* ___________________________ MOUSE ___________________________ */
            /*
             * Collect Mouse movement only when there is a change in position.
             * Also check that a mouse is connected!
             */

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            bool listenToMouse = Mouse.current != null &&
                Mouse.current.wasUpdatedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            bool listenToMouse = Input.mousePresent;
#else
            bool listenToMouse = false;
#endif

            if (listenToMouse)
            {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                Vector3 mousePosition = Mouse.current.position.ReadValue();

                /* Button 0 - leftButton */
                if (Mouse.current.leftButton.wasPressedThisFrame)
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_DOWN,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_LEFT
                    );
                else if (Mouse.current.leftButton.wasReleasedThisFrame)
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_UP,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_LEFT
                    );

                /* Button 1 - rightButton */
                if (Mouse.current.rightButton.wasPressedThisFrame)
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_DOWN,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_RIGHT
                    );
                else if (Mouse.current.rightButton.wasReleasedThisFrame)
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_UP,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_RIGHT
                    );

                /* Button 2 - middleButton */
                if (Mouse.current.middleButton.wasPressedThisFrame)
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_DOWN,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_MIDDLE
                    );
                else if (Mouse.current.middleButton.wasReleasedThisFrame)
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_UP,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_MIDDLE
                    );

                /* Button 3 - forwardButton */
                if (Mouse.current.forwardButton.wasPressedThisFrame)
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_DOWN,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_FORWARD
                    );
                else if (Mouse.current.forwardButton.wasReleasedThisFrame)
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_UP,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_FORWARD
                    );

                /* Button 4 - backButton */
                if (Mouse.current.backButton.wasPressedThisFrame)
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_DOWN,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_BACKWARD
                    );
                else if (Mouse.current.backButton.wasReleasedThisFrame)
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_UP,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_BACKWARD
                    );

                /* Scroll Wheel */
                if (Mouse.current.scroll.x.value != 0 || Mouse.current.scroll.y.value != 0)
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOUSE_WHEEL,
                        Mouse.current.scroll.x.value,
                        Mouse.current.scroll.y.value,
                        MOUSE_WHEEL
                    );

#elif ENABLE_LEGACY_INPUT_MANAGER
                Vector3 mousePosition = Input.mousePosition;

                if (Input.GetMouseButtonDown(0))
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_DOWN,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_LEFT
                    );
                else if (Input.GetMouseButtonUp(0))
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_UP,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_LEFT
                    );

                if (Input.GetMouseButtonDown(1))
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_DOWN,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_RIGHT
                    );
                else if (Input.GetMouseButtonUp(1))
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_UP,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_RIGHT
                    );

                if (Input.GetMouseButtonDown(2))
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_DOWN,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_MIDDLE
                    );
                else if (Input.GetMouseButtonUp(2))
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_UP,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_MIDDLE
                    );

                if (Input.GetMouseButtonDown(3))
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_DOWN,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_FORWARD
                    );
                else if (Input.GetMouseButtonUp(3))
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_UP,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_FORWARD
                    );

                if (Input.GetMouseButtonDown(4))
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_DOWN,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_BACKWARD
                    );
                else if (Input.GetMouseButtonUp(4))
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOTION_UP,
                        mousePosition.x, mousePosition.y,
                        MOUSE_BUTTON_BACKWARD
                    );

                /* Scroll Wheel */
                if (Input.mouseScrollDelta.x != 0 || Input.mouseScrollDelta.y != 0)
                    quagoManager.dispatchTouch(
                        ElapsedMilliseconds(),
                        BiometricType.MOUSE_WHEEL,
                        Input.mouseScrollDelta.x,
                        Input.mouseScrollDelta.y,
                        MOUSE_WHEEL
                    );
#endif

                /* The first mouse event */
                if (MouseLastX != mousePosition.x || MouseLastY != mousePosition.y)
                {
                    long eventTime = ElapsedMilliseconds();
                    MouseLastX = mousePosition.x;
                    MouseLastY = mousePosition.y;
                    quagoManager.dispatchTouch(
                        eventTime,
                        BiometricType.MOTION_MOVE,
                        MouseLastX,
                        MouseLastY,
                        0
                    );
                }
            }

            /* __________________________ Keyboard __________________________ */
            /*
             * Iterates over our key binding dictionary and checks for presses.
             */
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (Keyboard.current != null)
            {
                lock (keyMap)
                {
                    foreach (var entry in keyMap)
                        if (Keyboard.current[entry.Value].wasPressedThisFrame)
                            quagoManager.dispatchKey(
                                ElapsedMilliseconds(),
                                KeyDataPoint.CODE_KEY_DOWN,
                                (int)entry.Value
                            );
                    foreach (var entry in keyMap)
                        if (Keyboard.current[entry.Value].wasReleasedThisFrame)
                            quagoManager.dispatchKey(
                                ElapsedMilliseconds(),
                                KeyDataPoint.CODE_KEY_UP,
                                (int)entry.Value
                            );
                }
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            lock (keyMap)
            {
                foreach (var entry in keyMap)
                    if (Input.GetKeyDown(entry.Value))
                        quagoManager.dispatchKey(
                            ElapsedMilliseconds(),
                            KeyDataPoint.CODE_KEY_DOWN,
                            (int)entry.Value
                        );
                foreach (var entry in keyMap)
                    if (Input.GetKeyUp(entry.Value))
                        quagoManager.dispatchKey(
                            ElapsedMilliseconds(),
                            KeyDataPoint.CODE_KEY_UP,
                            (int)entry.Value
                        );
            }
#endif
            /* ________________________ RESOLUTIONS ________________________ */
            /*
             * Check for resolution change, wait a certain amount of time before
             * updating the SDK for those cases when the resolution was set then
             * moved after a couple of frames - happends when user resizes window.
             */
            if (winWidth != Screen.width ||
                winHeight != Screen.height ||
                scrWidth != Screen.currentResolution.width ||
                scrHeight != Screen.currentResolution.height)
            {
                /* Screen */
                scrWidth = Screen.currentResolution.width;
                scrHeight = Screen.currentResolution.height;
                scrRefreshRate = Screen.currentResolution.refreshRate;

                /* Window */
                winWidth = Screen.width;
                winHeight = Screen.height;

                _resolutionChangeStartMillis = CurrentTimestampMilliseconds();
                _resolutionChanged = true;
            }
            else if (_resolutionChanged &&
                CurrentTimestampMilliseconds() >= _resolutionChangeStartMillis + _resolutionChangeDurationMillis)
            {
                _resolutionChanged = false;

                /* Add event for resolution change */
                quagoManager.onResume();
            }
        }
    }

    /*.....................................Segments.Methods......................................*/

    public static void BeginSegment(string name)
    {
        if (_initialized)
            quagoManager.beginSegment(name);
        else
            handler.SendMessage(
                Message.Obtain(
                    handler, (int)COMMAND.SEGMENT_BEGIN,
                    0, 0, name
                    )
                );
    }

    public static void EndSegment()
    {
        if (_initialized)
            quagoManager.endSegment();
        else
            handler.SendMessage(
                Message.Obtain(
                    handler, (int)COMMAND.SEGMENT_END
                    )
                );
    }

    public static void BeginTracking(string userId)
    {
        if (_initialized)
            quagoManager.beginTracking(userId);
        else
            handler.SendMessage(
                Message.Obtain(
                    handler, (int)COMMAND.TRACKING_BEGIN,
                    0, 0, userId
                    )
                );
    }

    public static void EndTracking()
    {
        if (_initialized)
            quagoManager.endTracking();
        else
            handler.SendMessage(
                Message.Obtain(
                    handler, (int)COMMAND.TRACKING_END
                    )
                );
    }

    /*......................................Meta.Data.Methods.....................................*/

    public static void SetKeyValues(string key, string value)
    {
        if (_initialized)
            quagoManager.setKeyValues(key, value);
        else
            handler.SendMessage(
                Message.Obtain(
                    handler, (int)COMMAND.SET_KEY_VALUE,
                    0, 0, new string[] { key, value }
                    )
                );
    }

    public static void SetUserId(string userId)
    {
        if (_initialized)
            quagoManager.setUserId(userId);
        else
            handler.SendMessage(
                Message.Obtain(
                    handler, (int)COMMAND.SET_USER_ID,
                    0, 0, userId
                    )
                );
    }

    public static void SetAdditionalId(string additionalId)
    {
        if (_initialized)
            quagoManager.setAdditionalId(additionalId);
        else
            handler.SendMessage(
                Message.Obtain(
                    handler, (int)COMMAND.SET_ADDITIONAL_ID,
                    0, 0, additionalId
                    )
                );
    }

    /*.......................................Getter.Methods......................................*/

    public static string GetSessionId()
    {
        return quagoManager.getSessionId();
    }

    /*......................................Private.Methods.......................................*/
    /*.......................................Public.Methods.......................................*/

    internal static ScreenData GetScreenData()
    {
        return new ScreenData(winWidth, winHeight, scrWidth, scrHeight, scrRefreshRate);
    }

    /**
     * Returns the number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.
     */
    internal static long CurrentTimestampMilliseconds()
    {
        return DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }

    /**
     * Returns the number of milliseconds since SDK initialization.
     */
    internal static long ElapsedMilliseconds()
    {
        return stopwatch.ElapsedMilliseconds;
    }

    /**
     * Returns a readu dictionary from key-bindings used for metadata.
     */
    internal static List<string[]> KeyMapToDictionary()
    {
        List<string[]> list = new();
        lock (keyMap)
        {
            foreach (var entry in keyMap)
                list.Add(new string[] { entry.Key, entry.Value.ToString() });
        }
        return list;
    }

    /**
     * Bind a key (Key or KeyCode) with a key name, deletes every key-binding with the selected
     * key parameter, also does so when keyName is null.
     * Doesn't allow key duplications (two names bound to one key).
     * 
     * Supports old and new Unity Input System
     */
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
    public static void KeyBind(string keyName, Key key)
    {
#elif ENABLE_LEGACY_INPUT_MANAGER
    public static void KeyBind(string keyName, KeyCode key)
    {
#endif
        lock (keyMap)
        {
            /* Remove evey key-binding to key */
            List<string> removableKeys = new();
            foreach (var entry in keyMap)
                if (entry.Value == key)
                    removableKeys.Add(entry.Key);
            foreach (string k in removableKeys)
                keyMap.Remove(k);

            /* Add a new key-binding to key */
            if (keyName == null) return;
            if (keyMap.ContainsKey(keyName)) keyMap.Remove(keyName);
            keyMap.Add(keyName, key);
        }
    }

    public static void Initialize(QuagoSettings settings)
    {
        lock (_lock)
        {
            if (_init_called || _initialized) return;
            stopwatch.Restart();

            handler = new();
            handlerThread = new HandlerThread("QuagoStandaloneHendler", handler);

            /* Get the current Scene. */
            Scene currentScene = SceneManager.GetActiveScene();

            /* 
             * Destroy Duplicated Quago GameObjects if present.
             * Relevant only if Reflection was used to manipulate the SDK.
             */
            if (quagoGameObject != null)
                GameObject.Destroy(quagoGameObject);

            /* Create our GameObject. */
            quagoGameObject = new GameObject(quagoGameObjectName);

            /* Add our MonoComponent to it. */
            quagoGameObject.AddComponent<QuagoMono>();

            /* Move our GameObject to the current Scene. */
            SceneManager.MoveGameObjectToScene(quagoGameObject, currentScene);

            /* Setup Logger */
            Logger.SetLoggerLevel(settings.getLogLevel());
            if (settings.GetLogger() != null)
                Logger.SetOnLoggerListener(settings.GetLogger());

            QuagoStandalone.settings = settings;
            handler.SendEmptyMessage((int)COMMAND.INIT);
            _init_called = true;
        }
    }

    /*.......................................Handler.Methods......................................*/

    protected class QuagoStandaloneHandler : HandlerThread.Handler
    {
        public override void HandleMessage(Message msg)
        {
            switch ((COMMAND)msg.What)
            {
                case COMMAND.INIT:
                    while (!_initialized)
                    {
                        /* Wait until sdk initializes on main thread */
                    }
                    SendEmptyMessage((int)COMMAND.INIT_COMPLETE);
                    break;
                case COMMAND.SEGMENT_BEGIN:
                    quagoManager.beginSegment((string)msg.Obj);
                    break;
                case COMMAND.SEGMENT_END:
                    quagoManager.endSegment();
                    break;
                case COMMAND.TRACKING_BEGIN:
                    quagoManager.beginTracking((string)msg.Obj);
                    break;
                case COMMAND.TRACKING_END:
                    quagoManager.endTracking();
                    break;
                case COMMAND.SET_USER_ID:
                    quagoManager.setUserId((string)msg.Obj);
                    break;
                case COMMAND.SET_ADDITIONAL_ID:
                    quagoManager.setAdditionalId((string)msg.Obj);
                    break;
                case COMMAND.SET_KEY_VALUE:
                    string[] keyValues = (string[])msg.Obj;
                    quagoManager.setKeyValues(keyValues[0], keyValues[1]);
                    break;
                case COMMAND.INIT_COMPLETE:
                    handlerThread.QuitSafely();
                    handlerThread = null;
                    handler = null;
                    break;
            }
        }
    }
}
#endif