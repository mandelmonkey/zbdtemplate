/*
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 *  Copyright (c) 2023. Quago Technologies LTD - All Rights Reserved. Proprietary and confidential.
 *             Unauthorized copying of this file, via any medium is strictly prohibited.
 * ═════════════════════════════════════════════════════════════════════════════════════════════════
 */
#if UNITY_STANDALONE //Windows/MacOSx/Linux
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class QuagoMetaInformation : QuaggaSerializable
{
    public UnityData unity;

    public QuagoMetaInformation()
    {
        unity = new();
    }

    protected QuagoMetaInformation(QuagoMetaInformation quagoMeta)
    {
        unity = quagoMeta.unity;
    }

    public class UnityData
    {
        public string unity_version;
        public UnityApplication application;
        public UnitySystemInfo system_info;
        public UnityGraphics graphics;

        public UnityData()
        {
            unity_version = Application.unityVersion;
            application = new UnityApplication();
            system_info = new UnitySystemInfo();
            graphics = new UnityGraphics();
        }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = new();
            map.Add("unity_version", unity_version);
            map.Add("application", application.ToDictionary());
            map.Add("system_info", system_info.ToDictionary());
            map.Add("graphics", graphics.ToDictionary());
            return map;
        }
    }

    public class UnityApplication
    {
        public readonly string cloud_project_id = Application.cloudProjectId;
        public readonly string company_name = Application.companyName;
        public readonly bool genuine = Application.genuine;
        public readonly bool genuine_check_available = Application.genuineCheckAvailable;
        public readonly string[] build_tags = Application.GetBuildTags();
        public readonly bool has_pro_license = Application.HasProLicense();
        public readonly string identifier = Application.identifier;
        public readonly string version = Application.version;
        public readonly string installer_name = Application.installerName;
        public readonly string install_mode = Enum.GetName(typeof(ApplicationInstallMode), Application.installMode);
        public readonly string platform = Enum.GetName(typeof(RuntimePlatform), Application.platform);
        public readonly string product_name = Application.productName;
        public readonly string system_language = Enum.GetName(typeof(SystemLanguage), Application.systemLanguage);
        public readonly int target_frame_rate = Application.targetFrameRate;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        public readonly int input_system = 2;
#elif ENABLE_LEGACY_INPUT_MANAGER
        public readonly int input_system = 1;
#else
        public readonly int input_system = 0;
#endif

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = new();
            map.Add("cloud_project_id", cloud_project_id);
            map.Add("company_name", company_name);
            map.Add("genuine", genuine);
            map.Add("genuine_check_available", genuine_check_available);
            map.Add("build_tags", build_tags);
            map.Add("has_pro_license", has_pro_license);
            map.Add("installer_name", installer_name);
            map.Add("install_mode", install_mode);
            map.Add("platform", platform);
            map.Add("product_name", product_name);
            map.Add("system_language", system_language);
            map.Add("target_frame_rate", target_frame_rate);
            map.Add("input_system", input_system);
            return map;
        }
    }

    public class UnitySystemInfo
    {
        public readonly string operating_system = SystemInfo.operatingSystem;
        public readonly string device_model = SystemInfo.deviceModel;
        public readonly string device_type = Enum.GetName(typeof(DeviceType), SystemInfo.deviceType);
        public readonly string device_unique_identifier = SystemInfo.deviceUniqueIdentifier;
        public readonly string operating_system_family = Enum.GetName(typeof(OperatingSystemFamily), SystemInfo.operatingSystemFamily);
        public readonly int processor_count = SystemInfo.processorCount;
        public readonly int processor_frequency = SystemInfo.processorFrequency;
        public readonly string processor_type = SystemInfo.processorType;
        public readonly int system_memory_size = SystemInfo.systemMemorySize;

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = new();
            map.Add("operating_system", operating_system);
            map.Add("device_model", device_model);
            map.Add("device_type", device_type);
            map.Add("device_unique_identifier", device_unique_identifier);
            map.Add("operating_system_family", operating_system_family);
            map.Add("processor_count", processor_count);
            map.Add("processor_frequency", processor_frequency);
            map.Add("processor_type", processor_type);
            map.Add("system_memory_size", system_memory_size);
            return map;
        }
    }

    public class UnityGraphics
    {
        public readonly string graphics_device_name = SystemInfo.graphicsDeviceName;
        public readonly int graphics_device_id = SystemInfo.graphicsDeviceID;
        public readonly string graphics_device_type = Enum.GetName(typeof(GraphicsDeviceType), SystemInfo.graphicsDeviceType);
        public readonly string graphics_device_vendor = SystemInfo.graphicsDeviceVendor;
        public readonly int graphics_device_vendor_id = SystemInfo.graphicsDeviceVendorID;
        public readonly string graphics_device_version = SystemInfo.graphicsDeviceVersion;
        public readonly int graphics_memory_size = SystemInfo.graphicsMemorySize;

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> map = new();
            map.Add("graphics_device_name", graphics_device_name);
            map.Add("graphics_device_id", graphics_device_id);
            map.Add("graphics_device_type", graphics_device_type);
            map.Add("graphics_device_vendor", graphics_device_vendor);
            map.Add("graphics_device_vendor_id", graphics_device_vendor_id);
            map.Add("graphics_device_version", graphics_device_version);
            map.Add("graphics_memory_size", graphics_memory_size);
            return map;
        }
    }

    public class ScreenData
    {
        public readonly int winWidth;
        public readonly int winHeight;
        public readonly int scrWidth;
        public readonly int scrHeight;
        public readonly int scrRefreshRate;

        public ScreenData(int winWidth, int winHeight, int scrWidth, int scrHeight, int scrRefreshRate)
        {
            this.winWidth = winWidth;
            this.winHeight = winHeight;
            this.scrWidth = scrWidth;
            this.scrHeight = scrHeight;
            this.scrRefreshRate = scrRefreshRate;
        }

        public bool Equals(ScreenData obj)
        {
            if (obj == null) return false;
            return winWidth == obj.winWidth &&
                winHeight == obj.winHeight &&
                scrWidth == obj.scrWidth &&
                scrHeight == obj.scrHeight &&
                scrRefreshRate == obj.scrRefreshRate;
        }

        public ScreenData Copy()
        {
            return new ScreenData(winWidth, winHeight, scrWidth, scrHeight, scrRefreshRate);
        }

        public string StrCompare(ScreenData obj)
        {
            string same = " == ", diff = " <> ";
            return string.Format(
                "\nwinWidth: {0}{1}{2}\nwinHeight: {3}{4}{5}\nscrWidth: {6}{7}{8}\nscrHeight: {9}{10}{11}\nscrRefreshRate: {12}{13}{14}\n",

                winWidth,
                winWidth != obj.winWidth ? diff : same,
                obj.winWidth,

                winHeight,
                (winHeight != obj.winHeight ? diff : same),
                obj.winHeight,

                scrWidth,
                scrWidth != obj.scrWidth ? diff : same,
                obj.scrWidth,

                scrHeight,
                (scrHeight != obj.scrHeight ? diff : same),
                obj.scrHeight,

                scrRefreshRate,
                (scrRefreshRate != obj.scrRefreshRate ? diff : same),
                obj.scrRefreshRate
            );
        }
    }

    public QuagoMetaInformation Clone()
    {
        return new QuagoMetaInformation(this);
    }

    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> map = new();
        map.Add("unity", unity.ToDictionary());
        return map;
    }
}
#endif