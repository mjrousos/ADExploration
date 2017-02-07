using System;
using System.Linq;
using System.DirectoryServices;
using System.Collections.Generic;

class Program
{
    static string ADPath = "LDAP://REDMOND/OU=UserAccounts,DC=REDMOND,DC=corp,DC=microsoft,DC=com";
    static string Alias = "mikerou";

    static object logLock = new object();
    static void Main(string[] args)
    {
        // TODO - read ADPath from args, config, or prompting
        Log("AD Exploratory Program");
        Log();

        Log($"Creating AD node object using path {ADPath}");
        DirectoryEntry deNode = new DirectoryEntry(ADPath);
        Log($"Loaded {deNode.Name}", ConsoleColor.Cyan);
        Log();

        Log($"Finding path for {Alias}");
        DirectorySearcher searcher = new DirectorySearcher(deNode);
        searcher.Filter = "(mailNickname=" + Alias + ")";
        searcher.PropertiesToLoad.Clear();
        searcher.PropertiesToLoad.Add("distinguishedName");

        SearchResultCollection results = searcher.FindAll();

        var userLdapPath = results?[0].Properties["distinguishedName"][0];
        var user = new DirectoryEntry($"GC://{userLdapPath}"); 
        Log($"Found LDAP path {userLdapPath}", ConsoleColor.Cyan);
        Log($"User information:", ConsoleColor.Cyan);

        int maxPropesToList = 10;
        int propsListed = 0;
        foreach (var name in user.Properties.PropertyNames)
        {
            if (propsListed++ >= maxPropesToList) break;
            if (propsListed == 1) continue;
            var value = user.Properties[name.ToString()];
            if (value.Count == 1)
            {
                Log($"{name}: {value.Value.ToString()}", ConsoleColor.DarkGray);
            }
            if (value.Count > 1)
            {
                Log($"{name}:", ConsoleColor.DarkGray);
                foreach (var val in value) Log($"\t\t{val.ToString()}", ConsoleColor.DarkGray);
            }
            //Log($"{name}: {.Value ?? ")
        }

        Log();

        Log("Searching for user's management chain");
        var managementChain = GetManagementChain(user);
        Log($"  {string.Join(" -> ", managementChain.Select(de => de.Properties["mailNickname"].Value.ToString()))}", ConsoleColor.Cyan);
        Log();
        Log("- Done -");
    }

    private static IEnumerable<DirectoryEntry> GetManagementChain(DirectoryEntry user)
    {
        string managerPath = null;
        var managerPathProperties = user.Properties["manager"];
        if (managerPathProperties?.Count > 0)
        {
            managerPath = managerPathProperties[0].ToString();
        }
        if (string.IsNullOrEmpty(managerPath)) return new DirectoryEntry[] { user };

        return Enumerable.Concat(new DirectoryEntry[] { user }, GetManagementChain(new DirectoryEntry($"GC://{managerPath}")));
    }

    static void Log(string message = "", ConsoleColor? color = null)
    {
        lock(logLock)
        {
            if (color.HasValue) Console.ForegroundColor = color.Value;
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] {message}");
            if (color.HasValue) Console.ResetColor();
        }
    }
}