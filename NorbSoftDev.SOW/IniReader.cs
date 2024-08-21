using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NorbSoftDev.SOW {
public class IniReader
{
    //http://stackoverflow.com/questions/217902/reading-writing-an-ini-file
    Dictionary<string, Dictionary<string, string>> ini = new Dictionary<string, Dictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);

    public IniReader(string file)
    {
        if (!File.Exists(file))
        {
            Log.Warn(this, "Unable to find " + file);
            return;
        }
        var txt = File.ReadAllText(file, Config.TextFileEncoding);

        Dictionary<string, string> currentSection = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        ini[""] = currentSection;

        foreach(var line in txt.Split(new[]{"\n"}, StringSplitOptions.RemoveEmptyEntries)
                               // .Where(t => !string.IsNullOrWhiteSpace(t))
                               .Where(t => (t!=null && t!= string.Empty))
                               .Select(t => t.Trim()))
        {
            if (line.StartsWith(";"))
                continue;

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                string sectionName = line.Substring(1, line.LastIndexOf("]") - 1);
                if (!ini.TryGetValue(sectionName, out currentSection))
                {
                    currentSection = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    ini[sectionName] = currentSection;
                }
                continue;
            }

            var idx = line.IndexOf("=");
            if (idx == -1)
                currentSection[line] = "";
            else
                currentSection[line.Substring(0, idx).ToLower()] = line.Substring(idx + 1);
        }
    }

    public void SetValue(string key, string section, object value) {
        if (!ini.ContainsKey(section))
            ini[section] = new Dictionary<string, string>();

        ini[section][key] = value.ToString();
    }

    public string GetValue(string key)
    {
        return GetValue(key, "", "");
    }

    public string GetValue(string key, string section)
    {
        return GetValue(key, section, "");
    }

    public string GetValue(string key, string section, string @default)
    {
        if (!ini.ContainsKey(section))
            return @default;

        if (!ini[section].ContainsKey(key))
            return @default;

        return ini[section][key];
    }

    public string[] GetKeys(string section)
    {
        if (!ini.ContainsKey(section))
            return new string[0];

        List<string> list = ini[section].Keys.ToList<string>();
        list.Sort();
        return list.ToArray();
    }

    public string[] GetSections()
    {
        return ini.Keys.Where(t => t != "").ToArray();
    }

    public string Pretty() {
        string result = "";
        foreach (string section in GetSections()) {
            result += "["+section+"]"+System.Environment.NewLine;

            foreach(string key in GetKeys(section)) {
                if (key == null || key == String.Empty) continue;
                result += key+"="+GetValue(key,section)+System.Environment.NewLine;
            }


        }
        return result;

    }
}
}