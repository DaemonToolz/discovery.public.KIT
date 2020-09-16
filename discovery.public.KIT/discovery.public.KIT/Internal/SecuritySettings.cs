using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using Windows.Storage.Streams;
using discovery.KIT.Security;

namespace discovery.KIT.Internal
{
    public static partial class SettingsSecurity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> FetchAllContainers()
        {
            return ApplicationData.Current.LocalSettings.Containers.Keys;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="key"></param>
        /// <param name="global"></param>
        /// <returns></returns>
        public static async Task<string> ReadSettings(string alias, string key, bool global = true)
        {
            if (global)
            {
                var deserializedArray = (byte[])ApplicationData.Current.LocalSettings.Values[key];
                return await UnprotectAsync(
                    CryptographicBuffer.CreateFromByteArray(deserializedArray), BinaryStringEncoding.Utf16BE);
            }
            else
            {

                if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(alias))
                {
                    return "";
                }
                var deserializedArray = (byte[])(ApplicationData.Current.LocalSettings.Containers[alias].Values[key]);
                return await UnprotectAsync(
                    CryptographicBuffer.CreateFromByteArray(deserializedArray), BinaryStringEncoding.Utf16BE);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="key"></param>
        /// <param name="global"></param>
        /// <returns></returns>
        public static bool DeleteSettings(string alias, string key, bool global = false)
        {
            return !global ? ApplicationData.Current.LocalSettings.Containers[alias].Values.Remove(key) : ApplicationData.Current.LocalSettings.Values.Remove(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias"></param>
        public static void DeleteUserSettings(string alias)
        {
            ApplicationData.Current.LocalSettings.DeleteContainer(alias);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <param name="global"></param>
        /// <returns></returns>
        public static async Task WriteSettings(string alias,string key, string content, bool global = true)
        {
            var data = await ProtectAsync(content, "LOCAL=machine");
            var reader = DataReader.FromBuffer(data);
            var array = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(array);
            if (!global)
            {
                if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(alias))
                {
                    ApplicationData.Current.LocalSettings.CreateContainer(alias,
                        ApplicationDataCreateDisposition.Always);
                }

                ApplicationData.Current.LocalSettings.Containers[alias].Values[key] = array;
            }
            else
            {

                ApplicationData.Current.LocalSettings.Values[key] = array;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="guid"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task SaveCredentials(string guid, string username, SecureString password)
        {
            var vault = new PasswordVault();
            await DeleteCredentials(guid);
            vault.Add(new PasswordCredential(guid, username, SecuritySettings.SecureStringToString(password)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static Task DeleteCredentials(string guid, string username, SecureString password) {
            return Task.Run(() =>
            {
                var vault = new PasswordVault();
                try
                {
                    if (vault.FindAllByResource(guid).Any())
                    {
                        vault.Remove(new PasswordCredential(guid, username, SecuritySettings.SecureStringToString(password)));
                    }
                }
                catch
                {

                }
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static Task DeleteCredentials(string guid, PasswordCredential credentials)
        {
            return Task.Run(() =>
            {
                var vault = new PasswordVault();
                try
                {
                    if (vault.FindAllByResource(guid).Any())
                    {
                        vault.Remove(credentials);
                    }
                }
                catch
                {

                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static Task DeleteCredentials(string guid)
        {
            return Task.Run(() =>
            {
                var vault = new PasswordVault();
                try
                {
                    if (vault.FindAllByResource(guid).Any())
                    {
                        var credentials = vault.FindAllByResource(guid).First();
                        vault.Remove(credentials);
                    }
                }
                catch
                {

                }
            });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static IReadOnlyList<PasswordCredential> GetCredentials(string guid)
        {
            var vault = new PasswordVault();
            return vault.FindAllByResource(guid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        private static async Task<IBuffer> ProtectAsync(
            string inputString,
            string descriptor)
        {
            var provider = new DataProtectionProvider(descriptor);
            const BinaryStringEncoding encoding = BinaryStringEncoding.Utf16BE;
            var buffMsg = CryptographicBuffer.ConvertStringToBinary(inputString, encoding);
            var buffProtected = await provider.ProtectAsync(buffMsg);
            return buffProtected;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffProtected"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        private static async Task<string> UnprotectAsync(
            IBuffer buffProtected,
            BinaryStringEncoding encoding)
        {
            var provider = new DataProtectionProvider();
            var buffUnprotected = await provider.UnprotectAsync(buffProtected);
            var outputStr = CryptographicBuffer.ConvertBinaryToString(encoding, buffUnprotected);
            return outputStr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="key"></param>
        /// <param name="global"></param>
        /// <returns></returns>
        public static bool HasSettings(string alias, string key, bool global = false)
        {
            if (!global)
            {
                return ApplicationData.Current.LocalSettings.Containers.ContainsKey(alias) && ApplicationData.Current.LocalSettings.Containers[alias].Values.ContainsKey(key);
            }
            else
            {
                return ApplicationData.Current.LocalSettings.Values.ContainsKey(key);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="key"></param>
        /// <param name="global"></param>
        /// <returns></returns>
        public static void DeleteContainer(string alias)
        {
            ApplicationData.Current.LocalSettings.DeleteContainer(alias);
        }
        
    

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static SecureString StringToSecureString(string input)
        {
            var securePassword = new SecureString();
            foreach (var c in input.ToCharArray())
            {
                securePassword.AppendChar(c);
            }
            securePassword.MakeReadOnly();
            input = string.Empty;
            return securePassword;
        }
    }
}
