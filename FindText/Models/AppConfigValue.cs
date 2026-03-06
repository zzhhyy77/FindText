using System.Linq;

namespace FindText.Models
{
    internal class AppConfigValue
    {

        public TextSearchOption? SearchOption { get; set; }

        public double MainLeft { get; set; }

        public double MainTop { get; set; }

        public double MainHeight { get; set; }

        public double MainWidth { get; set; }

        public string Language { get; set; }

        public string Theme { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;

            AppConfigValue? val = obj as AppConfigValue;
            if (val == null) return false;

            if (SearchOption == null) return false;
            if (val.SearchOption == null) return false;

            try
            {
                if (MainLeft != val.MainLeft) return false;
                if (MainTop != val.MainTop) return false;
                if (MainHeight != val.MainHeight) return false;
                if (MainWidth != val.MainWidth) return false;
                if (Language != val.Language) return false;
                if (Theme != val.Theme) return false;

                #region 比对SearchOption值

                string[] names = { "SearchText", "Tag", "IsWholeWord", "FileEncoding" };
                System.Type typeB = val.SearchOption.GetType();

                var soA = SearchOption.GetType().GetProperties();
                var soB = val.SearchOption.GetType().GetProperties();
                foreach (var pi in soA)
                {
                    if (names.Contains(pi.Name))
                        continue;
                    object? valA = pi.GetValue(this.SearchOption);
                    object? valB = typeB.GetProperty(pi.Name).GetValue(val.SearchOption);

                    if ($"{valA}" != $"{valB}") //直接==总返回false
                    {
                        return false;
                    }
                }
                #endregion

                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
