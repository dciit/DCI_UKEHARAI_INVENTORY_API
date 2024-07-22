﻿namespace DCI_UKEHARAI_INVENTORY_API
{
    public class Helper
    {
        public string SetDigit(string value, int digit)
        {
            try
            {
                return Convert.ToInt32(value).ToString($"D{digit}");
            }
            catch
            {
                return value;
            }
        }
        public decimal ConvStrToDec(string val)
        {
            try
            {
                return val != "" ? Convert.ToDecimal(val) : 0;
            }
            catch
            {
                return 0;
            }
        }
        public int ConvDecToInt(decimal? val)
        {
            try
            {
                return val != null ? (int)val : 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
