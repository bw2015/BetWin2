using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace SP.Studio.Component
{
    /// <summary>
    /// 注册表操作类
    /// </summary>
    public class Regedit
    {
        /// <summary>
        /// 创建子项
        /// </summary>
        public static void CreateSubKey(KeyType type, string subkey)
        {
            RegistryKey key = GetKey(type);
            if (string.IsNullOrEmpty(subkey)) return;
            try
            {
                RegistryKey sKey = key.OpenSubKey(subkey);
                if (sKey != null) return;

                key.CreateSubKey(subkey);
            }
            finally
            {
                key.Close();
            }
        }


        /// <summary>
        /// 获取值（路径或者名字不存在的时候返回null）
        /// </summary>
        public static object GetValue(KeyType type, string subkey, string name)
        {
            RegistryKey key = GetKey(type);
            try
            {
                RegistryKey sKey = key.OpenSubKey(subkey);
                if (sKey == null) return null;
                return sKey.GetValue(name);
            }
            finally
            {
                key.Close();
            }
        }

        /// <summary>
        /// 设置值
        /// </summary>
        public static void SetValue(KeyType type, string subkey, string name, object value)
        {
            RegistryKey key = GetKey(type);
            try
            {
                RegistryKey sKey = key.CreateSubKey(subkey);
                sKey.SetValue(name, value);
            }
            finally
            {
                key.Close();
            }
        }

        /// <summary>
        /// 根据枚举获取当前主键
        /// </summary>
        private static RegistryKey GetKey(KeyType type)
        {
            RegistryKey key;
            switch (type)
            {
                case KeyType.HKEY_CLASS_ROOT:
                    key = Registry.ClassesRoot;
                    break;
                case KeyType.HKEY_CURRENT_USER:
                    key = Registry.CurrentUser;
                    break;
                case KeyType.HKEY_LOCAL_MACHINE:
                    key = Registry.LocalMachine;
                    break;
                case KeyType.HKEY_USERS:
                    key = Registry.Users;
                    break;
                case KeyType.HKEY_CURRENT_CONFIG:
                    key = Registry.CurrentConfig;
                    break;
                default:
                    key = Registry.LocalMachine;
                    break;
            }
            return key;
        }

        /// <summary>
        /// 主键类型
        /// </summary>
        public enum KeyType
        {
            /// <summary>
            /// 注册表基项 HKEY_CLASSES_ROOT
            /// </summary>
            HKEY_CLASS_ROOT,
            /// <summary>
            /// 注册表基项 HKEY_CURRENT_USER
            /// </summary>
            HKEY_CURRENT_USER,
            /// <summary>
            /// 注册表基项 HKEY_LOCAL_MACHINE
            /// </summary>
            HKEY_LOCAL_MACHINE,
            /// <summary>
            /// 注册表基项 HKEY_USERS
            /// </summary>
            HKEY_USERS,
            /// <summary>
            /// 注册表基项 HKEY_CURRENT_CONFIG
            /// </summary>
            HKEY_CURRENT_CONFIG
        }
    }
}
