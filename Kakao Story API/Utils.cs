using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryApi
{
    public static class Utils
    {

        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long ToUnixTime(DateTime date)
        {
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }
        public static string GetTimeString(DateTime created_at)
        {
            int offset = DateTimeOffset.Now.Offset.Hours;
            string dateText = created_at.AddHours(offset).ToString();
            var diffTime = DateTime.Now.Subtract(created_at.AddHours(offset));
            if (diffTime.TotalSeconds < 60)
            {
                dateText = "방금 전";
            }
            else if (diffTime.TotalMinutes < 60)
            {
                dateText = ((int)diffTime.TotalMinutes).ToString() + "분 전";
            }
            else if (diffTime.TotalHours < 24)
            {
                dateText = ((int)diffTime.TotalHours).ToString() + "시간 전";
            }
            return dateText;
        }
    }
}
