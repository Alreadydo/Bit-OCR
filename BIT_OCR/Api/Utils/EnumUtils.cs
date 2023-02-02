using System;
using System.Collections.Generic;

namespace BiT.Central.OCR.Api.Utils
{
    public static class EnumUtils
    {
        public static IEnumerable<KeyValuePair<string, int>> AsEnumerable(Type enumType)
        {
            var result = new List<KeyValuePair<string, int>>();
            var names = Enum.GetNames(enumType);
            var values = Enum.GetValues(enumType) as int[];

            for (int i = 0; i < names.Length; i++)
            {
                result.Add(new KeyValuePair<string, int>(
                    names[i],
                    values[i]
                ));
            }

            return result;
        }
    }
}