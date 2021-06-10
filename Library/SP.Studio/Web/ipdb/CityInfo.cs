﻿using System.Linq;
using System.Text;

namespace ipdb
{

    public class CityInfo
    {

        public readonly string[] data;

        public CityInfo(string[] data)
        {
            this.data = data;
        }

        /// <summary>
        /// 国家
        /// </summary>
        /// <returns></returns>
        public string getCountryName()
        {
            return data[0];
        }

        /// <summary>
        /// 省
        /// </summary>
        /// <returns></returns>
        public string getRegionName()
        {
            return data[1];
        }

        /// <summary>
        /// 城市
        /// </summary>
        /// <returns></returns>
        public string getCityName()
        {
            return data[2];
        }

        public string getOwnerDomain()
        {
            return data[3];
        }

        public string getIspDomain()
        {
            return data[4];
        }

        public string getLatitude()
        {
            return data[5];
        }

        public string getLongitude()
        {
            return data[6];
        }

        public string getTimezone()
        {
            return data[7];
        }

        public string getUtcOffset()
        {
            return data[8];
        }

        public string getChinaAdminCode()
        {
            return data[9];
        }

        public string getIddCode()
        {
            return data[10];
        }

        public string getCountryCode()
        {
            return data[11];
        }

        public string getContinentCode()
        {
            return data[12];
        }

        public string getIDC()
        {
            return data[13];
        }

        public string getBaseStation()
        {
            return data[14];
        }

        public string getCountryCode3()
        {
            return data[15];
        }

        public string getEuropeanUnion()
        {
            return data[16];
        }

        public string getCurrencyCode()
        {
            return data[17];
        }

        public string getCurrencyName()
        {
            return data[18];
        }

        public string getAnycast()
        {
            return data[19];
        }

        public override string ToString()
        {
            return string.Join(" ", this.data.Where(t => !string.IsNullOrEmpty(t)).Distinct());
        }

        /// <summary>
        /// 默认转化成为字符串
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public static implicit operator string(CityInfo cityInfo)
        {
            return cityInfo.ToString();
        }
    }

}
