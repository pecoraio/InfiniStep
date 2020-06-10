using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Data;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Numerics;
using System.Globalization;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Automation;
using System.Dynamic;
using System.Collections;

namespace OperationInfinity
{
    public static class ControlExtensions
    {
        public static T Clone<T>(this T controlToClone)
            where T : Control
        {
            PropertyInfo[] controlProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            T instance = Activator.CreateInstance<T>();

            foreach (PropertyInfo propInfo in controlProperties)
            {
                if (propInfo.CanWrite)
                {
                    if (propInfo.Name != "WindowTarget")
                        propInfo.SetValue(instance, propInfo.GetValue(controlToClone, null), null);
                }
            }

            return instance;
        }
        public static T Copy<T>(this T target)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                binaryFormatter.Serialize(stream, target);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(stream);
            }
        }
        public static void Do<TControl>(this TControl control, Action<TControl> action) where TControl : Control
        {
            if (control.InvokeRequired)
                control.Invoke(action, control);
            else
                action(control);
        }
    }
    public static class StringExtensions
    {
        public static string Left(this string s, int count)
        {
            return s.Substring(0, count);
        }

        public static string Right(this string s, int count)
        {
            return s.Substring(s.Length - count, count);
        }
        public static string Remove(this string s, int count)
        {
            return s.Substring(0,s.Length - count);
        }

        public static int ToInteger(this string s)
        {
            int integerValue = 0;
            int.TryParse(s, out integerValue);
            return integerValue;
        }
        public static BigInteger ToBigInteger(this string s)
        {
            BigInteger integerValue = 0;
            BigInteger.TryParse(s, out integerValue);
            return integerValue;
        }
        public static bool IsInteger(this string s)
        {
            Regex regularExpression = new Regex("^-[0-9]+$|^[0-9]+$");
            return regularExpression.Match(s).Success;
        }
        public static int LastIndexOf(this string str, char[] chars)
        {
            int ret = str.Length - 1;
            return chars.Max(x => str.LastIndexOf(x));
        }
        public static int GetNumberOfDigits(this string str)
        {
            Regex regex = new Regex(@"[0-9]+$", RegexOptions.IgnoreCase);
            if (!regex.IsMatch(str)) return 0;
            for (int charindex = str.Length - 1; charindex >= 0; charindex--)
            {
                if (!regex.IsMatch(str[charindex].ToString()))
                {
                    return str.Length - 1 - charindex;
                }
            }
            return str.Length;
        }
        public static string RemoveAll(this string str, params string[] words)
        {
            string ret = str;
            words.ToList().ForEach(x => ret = ret.Replace(x, ""));
            return ret;
        }
        public static string AddMaskNumber(this string str, BigInteger num, string Mask)
        {
            int lastDigit = GetLastDigit(str);
            return str.Left(lastDigit) + num.ToString("D" + (str.Length - lastDigit).ToString());
        }
        public static bool AnyLike(this string s1, params string[] ss)
        {
            return ss.Any(x => x.IndexOf(s1) > -1);
        }
        public static int GetLastDigit(this string str)
        {
            return Regex.Match(str.RemoveAll(" ", ">", "<"), "([?0-9])[0-9]+$").Index;
        }


        public static string RegReplace(this string str, string ptn, string rep, bool Ignore = true)
        {
            RegexOptions opt;
            if (Ignore)
            {
                opt = RegexOptions.IgnoreCase;
            }
            else
            {
                opt = RegexOptions.None;
            }
            return Regex.Replace(str, ptn, rep, opt);
        }
        public static string[] parse(this string csvLine)
        {
            return Regex.Split(csvLine, "[\t,](?=(?:[^\"]|\"[^\"]*\")*$)")
                .Select(s => Regex.Replace(s.Replace("\"\"", "\""), "^\"|\"$", "")).ToArray();
        }
        public static string Repeat(this string str, int count)
        {
            return string.Concat(Enumerable.Repeat(str, count));
        }
        public static string GetSafeFilename(this string filename)
        {
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(filename, "");
        }
    }
    public static class Extensions
    {
        public static DataSet ToDataSet<T>(this IList<T> list)
        {
            Type elementType = typeof(T);
            DataSet ds = new DataSet();
            DataTable t = new DataTable();
            ds.Tables.Add(t);

            //add a column to table for each public property on T
            foreach (var propInfo in elementType.GetProperties())
            {
                Type ColType = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;

                t.Columns.Add(propInfo.Name, ColType);
            }

            //go through each property on T and add each value to the table
            foreach (T item in list)
            {
                DataRow row = t.NewRow();

                foreach (var propInfo in elementType.GetProperties())
                {
                    row[propInfo.Name] = propInfo.GetValue(item, null) ?? DBNull.Value;
                }

                t.Rows.Add(row);
            }

            return ds;
        }
        public static List<T> ToList<T>(this DataTable table) where T : new()
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            List<T> result = new List<T>();

            foreach (var row in table.Rows)
            {
                var item = CreateItemFromRow<T>((DataRow)row, properties);
                result.Add(item);
            }

            return result;
        }

        private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            T item = new T();
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(System.DayOfWeek))
                {
                    DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), row[property.Name].ToString());
                    property.SetValue(item, day, null);
                }
                else
                {
                    property.SetValue(item, row[property.Name], null);
                }
            }
            return item;
        }
        public static List<string> reverseStringFormat(string template, string str)
        {
            string pattern = "^" + Regex.Replace(template, @"\{[0-9]+\}", "(.*?)") + "$";

            Regex r = new Regex(pattern);
            Match m = r.Match(str);

            List<string> ret = new List<string>();

            for (int i = 1; i < m.Groups.Count; i++)
            {
                ret.Add(m.Groups[i].Value);
            }

            return ret;
        }
        public static List<string> GetBraces(string str)
        {
            var grp = Regex.Match(str, @"\{([^)]*)\}").Groups;
            return grp.Cast<Group>().Select(x => x.Value.Replace("{","").Replace("}","")).ToList();
        }
        public static List<String> GetTokens(string str="")
        {
            if(string.IsNullOrEmpty(str))
            {
                return new List<string>();
            }
            Regex regex = new Regex(@"(?<=\{)[^}]*(?=\})", RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(str);

            // Results include braces (undesirable)
            return matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();
        }
        public static string GetMemberName<MemberType>(Expression<Func<MemberType>> expression)
        {
            return ((MemberExpression)expression.Body).Member.Name;
        }
        public static bool ContainsLike(this IEnumerable<string> ss, string s1)
        {
            foreach (string q in ss)
            {
                if (s1.IndexOf(q) > -1) return true;
            }
            return false;
        }

        public static int RowIndex(this DataRow row,DataTable dt)
        {
            return dt.Rows.IndexOf(row);
        }
        public static bool isBetween<T>(this T current, T lower, T higher, bool inclusive = true) where T : IComparable
        {
            if (lower.CompareTo(higher) > 0) Swap(ref lower, ref higher);

            return inclusive ?
                (lower.CompareTo(current) <= 0 && current.CompareTo(higher) <= 0) :
                (lower.CompareTo(current) < 0 && current.CompareTo(higher) < 0);
        }
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
        public static IEnumerable<AutomationElement> FindInRawView(this AutomationElement root)
        {
            TreeWalker rawViewWalker = TreeWalker.RawViewWalker;
            Queue<AutomationElement> queue = new Queue<AutomationElement>();
            queue.Enqueue(root);
            while (queue.Count > 0)
            {
                var element = queue.Dequeue();
                yield return element;

                var sibling = rawViewWalker.GetNextSibling(element);
                if (sibling != null)
                {
                    queue.Enqueue(sibling);
                }

                var child = rawViewWalker.GetFirstChild(element);
                if (child != null)
                {
                    queue.Enqueue(child);
                }
            }
        }
        public static List<AutomationElement> GetAllChildren(this AutomationElement element)
        {
            var allChildren = new List<AutomationElement>();
            AutomationElement sibling = TreeWalker.RawViewWalker.GetFirstChild(element);

            while (sibling != null)
            {
                allChildren.Add(sibling);
                sibling = TreeWalker.RawViewWalker.GetNextSibling(sibling);
            }

            return allChildren;
        }
        public static string GetText(this AutomationElement element)
        {
            object patternObj;
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out patternObj))
            {
                var valuePattern = (ValuePattern)patternObj;
                return valuePattern.Current.Value;
            }
            else if (element.TryGetCurrentPattern(TextPattern.Pattern, out patternObj))
            {
                var textPattern = (TextPattern)patternObj;
                return textPattern.DocumentRange.GetText(-1).TrimEnd('\r'); // often there is an extra '\r' hanging off the end.
            }
            else
            {
                return element.Current.Name;
            }
        }
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)
        {
            if (source == null || source.Count == 0)
                return default(TValue);

            TValue result;
            if (source.TryGetValue(key, out result))
                return result;
            return default(TValue);
        }
        public static bool Exsits<T>(this T d, string key) where T: IDictionary
        {
            return ((IDictionary<String, object>)d).ContainsKey(key);
        }
        public static bool Exsits(this ExpandoObject d,string key)
        {
            return ((IDictionary<String, object>)d).ContainsKey(key);
        }
        public static void AddValue<TKey, TList, TValue>(this IDictionary<TKey, TList> source, TKey key, TValue value) where TList : ICollection<TValue>
        {
            if (source == null)
                return;

            if (!source.ContainsKey(key))
            {
                source[key] = Activator.CreateInstance<TList>();
            }
            source[key].Add(value);
        }

        public static bool In<T>(this T obj, params T[] ary)
        {// like a sql in.
            return ary.Contains(obj);
        }
        public static bool InByType<T>(this T obj, params Type[] types)
        {// like a sql in.
            return types.Contains(obj.GetType());
        }
        public static T[] Repeat<T>(this T obj, int count)
        {
            return Enumerable.Repeat(obj, count).ToArray();
        }
        public static  string ToJsonObj(this DataTable dt)
        {
            DataSet ds = new DataSet();
            ds.Merge(dt);
            StringBuilder JsonString = new StringBuilder();
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                JsonString.Append("[");
                for (int row = 0; row < ds.Tables[0].Rows.Count; row++)
                {
                    JsonString.Append("{");
                    for (int col = 0; col < ds.Tables[0].Columns.Count; col++)
                    {
                        if (col < ds.Tables[0].Columns.Count - 1)
                        {
                            JsonString.Append("\"" + ds.Tables[0].Columns[col].ColumnName.ToString() + "\":" + "\"" + ds.Tables[0].Rows[row][col].ToString() + "\",");
                        }
                        else if (col == ds.Tables[0].Columns.Count - 1)
                        {
                            JsonString.Append("\"" + ds.Tables[0].Columns[col].ColumnName.ToString() + "\":" + "\"" + ds.Tables[0].Rows[row][col].ToString() + "\"");
                        }
                    }
                    if (row == ds.Tables[0].Rows.Count - 1)
                    {
                        JsonString.Append("}");
                    }
                    else
                    {
                        JsonString.Append("},");
                    }
                }
                JsonString.Append("]");
                return JsonString.ToString();
            }
            else
            {
                return null;
            }
        }
        public static bool HasProperty(this object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName) != null;
        }

    }//Extensions

    public static class SerializationExtensions
    {
        public static string ToXml<T>(this List<T> list)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<T>));

            StringWriter stringWriter = new StringWriter();
            XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);

            xmlWriter.Formatting = Formatting.Indented;
            xmlSerializer.Serialize(xmlWriter, list);
            return stringWriter.ToString();
        }
        public static void ToXml<T>(this List<T> list,string filename)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<T>));

            using (FileStream fs = new FileStream(filename, FileMode.Create))
            using (XmlTextWriter xmlWriter = new XmlTextWriter(fs,Encoding.UTF8))
            {
                xmlWriter.Formatting = Formatting.Indented;
                xmlSerializer.Serialize(fs, list);
            }

        }
    }
}
