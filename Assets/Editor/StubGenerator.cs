using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using ICSharpCode.SharpZipLib.Zip;
using Rhinox.Lightspeed;

public static class StubGenerator
{
    private static readonly Regex _usingsRegex = new Regex(@"^\s*((?:(?:using\s+.+?;\s*|\s*#.*|\s*)\s*)+)\s*([\s\S]*)$");
    private static readonly Regex _usingSplitterRegex = new Regex(@"\s*((?:using\s+.+?;|\s*#if\s+(.*)\s+([\s\S]*?)#endif|#.*))");
    private static readonly Regex _namespaceRegex = new Regex(@"(namespace .*?){([\s\S]*)}");

    private struct UsingStatement
    {
        public string Statement;
        public string Conditional;
        
        public UsingStatement(string statement, string conditional = null)
        {
            Statement = statement?.Trim();
            Conditional = conditional?.Trim();
        }

        public bool IsNullOrEmpty() => Statement.IsNullOrEmpty();

        public override string ToString()
        {
            if (Statement.StartsWith("#"))
                return Environment.NewLine + Statement + Environment.NewLine;
            if (Conditional.IsNullOrEmpty())
                return Statement;
            return $"{Environment.NewLine}#if {Conditional}{Environment.NewLine}" +
                   $"{Statement}" +
                   $"{Environment.NewLine}#endif{Environment.NewLine}";
        }
    }
    
    private class ManagedStringBuilder
    {
        protected StringBuilder _stringBuilder;
        public bool HasDataInLine { get; private set; }

        public ManagedStringBuilder()
        {
            _stringBuilder = new StringBuilder();
        }
        
        public void Append(string text)
        {
            _stringBuilder.Append(text);
            HasDataInLine = true;
        }
        
        public void AppendLine()
        {
            _stringBuilder.AppendLine();
            HasDataInLine = false;
        }

        public void AppendLine(string text)
        {
            _stringBuilder.AppendLine(text);
            HasDataInLine = false;
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
    }
    private class StubData : ManagedStringBuilder
    {
        private Dictionary<string, HashSet<UsingStatement>> _usingStatementsByNameSpace;
        private Dictionary<string, StringBuilder> _filesByNamespace;

        private bool _isResolved;

        public StubData()
        {
            _usingStatementsByNameSpace = new Dictionary<string, HashSet<UsingStatement>>();
            _filesByNamespace = new Dictionary<string, StringBuilder>();
        }

        public void RegisterUsing(string namespaceName, UsingStatement statement)
        {
            namespaceName = namespaceName.Trim(); 
            if (statement.IsNullOrEmpty())
                return;
            
            HashSet<UsingStatement> set;
            if (!_usingStatementsByNameSpace.ContainsKey(namespaceName))
            {
                set = new HashSet<UsingStatement>();
                _usingStatementsByNameSpace.Add(namespaceName, set);
            }
            else
                set = _usingStatementsByNameSpace[namespaceName];
            
            // TODO solve the same statement in other conditionals (or none)
            set.Add(statement);
        }

        public void RegisterCode(string namespaceName, string code)
        {
            namespaceName = namespaceName.Trim();
            
            StringBuilder sb;
            if (!_filesByNamespace.ContainsKey(namespaceName))
            {
                sb = new StringBuilder();
                _filesByNamespace.Add(namespaceName, sb);
            }
            else
                sb = _filesByNamespace[namespaceName];

            sb.AppendLine(code);
        }

        private void Resolve()
        {
            foreach (var namespaceName in _filesByNamespace.Keys)
            {
                _stringBuilder.AppendLine(namespaceName);
                _stringBuilder.AppendLine("{");

                if (_usingStatementsByNameSpace.ContainsKey(namespaceName))
                {
                    foreach (var usingStatement in _usingStatementsByNameSpace[namespaceName])
                        _stringBuilder.Append(usingStatement);
                    _stringBuilder.AppendLine();
                }

                var code = _filesByNamespace[namespaceName].ToString();
                _stringBuilder.AppendLine(code);
                
                _stringBuilder.AppendLine("}");

            }
        }

        public override string ToString()
        {
            if (!_isResolved)
            {
                AppendLine("#if !ODIN_INSPECTOR");
            
                Resolve();

                AppendLine();
                Append("#endif");
                _isResolved = true;
            }
            
            return _stringBuilder.ToString();
        }
    }
    
    const string SourceZip = @"Packages\com.rhinox.odininspector\Sirenix\Source\Source.zip";
    
    [MenuItem("Tools/Generate Stub File")]
    public static void CreateAttributeStubFile()
    {
        var paths = new string[]
        {
            // @".\Sirenix.OdinInspector.Attributes\Attributes",
            // @".\Sirenix.OdinInspector.Attributes\Misc",
            // @".\Assets\GUIUtils\Odin\Attributes",
        };

        StubData data = new StubData();
        
        HandleZip(SourceZip, new []
        {
            @"Source/Sirenix.OdinInspector.Attributes/Attributes",
            @"Source/Sirenix.OdinInspector.Attributes/Misc"
        }, ref data);
        
        foreach (string path in paths)
        {
            FindFiles(path, ref data);
        }

        var outPath = @".\Assets\Sirenix.OdinInspector.Attributes.Stub.cs";
        File.WriteAllText(outPath, data.ToString());
        
        Debug.Log($"Stub created @ {Path.GetFullPath(outPath)}");
    }

    private static void AppendAttributeFile(string name, string[] lines, ref StubData data)
    {
        var sb = new ManagedStringBuilder();
        
        foreach (string line in lines)
        {
            var trimmed = line.Split(new[] {"//"}, StringSplitOptions.None)[0];
            trimmed = trimmed.Trim();

            if (trimmed.IsNullOrEmpty()) continue;

            if (trimmed.StartsWith("//")) continue;

            if (trimmed.StartsWith("#")) // Special line i.e. #if UNITY_EDITOR, etc, needs to be a new line
            {
                if (sb.HasDataInLine)
                    sb.AppendLine();

                sb.AppendLine(trimmed);
            }
            else
            {
                if (sb.HasDataInLine)
                    sb.Append(" ");
                
                sb.Append(trimmed);
            }
        }

        var str = sb.ToString();
        Match match;
        string usingStatements = string.Empty;

        str = FilterUsingStatements(str, ref usingStatements);

        match = _namespaceRegex.Match(str);

        if (!match.Success)
        {
            Debug.Log($"uhhhhh ??? {name}");
            return;
        }
        
        var namespaceName = match.Groups[1].Value;
        str = match.Groups[2].Value;

        str = FilterUsingStatements(str, ref usingStatements);

        var matches = _usingSplitterRegex.Matches(usingStatements);

        foreach (Match statementMatch in matches)
        {
            string statement;
            string conditional = string.Empty;
            if (!statementMatch.Groups[2].Value.IsNullOrEmpty()) // Conditional
            {
                // TODO solve multiple statements in 1 conditional
                conditional = statementMatch.Groups[2].Value;
                statement = statementMatch.Groups[3].Value;
            }
            else
                statement = statementMatch.Value;
            data.RegisterUsing(namespaceName, new UsingStatement(statement, conditional));
        }

        data.RegisterCode(namespaceName, $"// {name}\n{str}");
    }

    private static string FilterUsingStatements(string str, ref string usingStatements)
    {
        Match match;
        match = _usingsRegex.Match(str);

        if (match.Success) // Find initial using statements
        {
            // 1st group is the entire string
            usingStatements += match.Groups[1].Value;
            str = match.Groups[2].Value;
        }

        return str;
    }

    private static void FindFiles(string path, ref StubData data)
    {
        foreach (var file in Directory.GetFiles(path, "*.cs"))
        {
            var lines = File.ReadAllLines(file);

            AppendAttributeFile(Path.GetFileName(file), lines, ref data);
        }
    }

    private static void HandleZip(string path, string[] pathsToCheck, ref StubData data)
    {
        ZipFile file = null;
        try
        {
            FileStream fs = File.OpenRead(path);
            file = new ZipFile(fs);
            
            foreach (ZipEntry zipEntry in file)
            {
                if (!zipEntry.IsFile) // Ignore directories
                    continue;

                var name = zipEntry.Name.Replace("\\", "/");

                if (!pathsToCheck.Any(x => name.StartsWith(x)))
                    continue;

                var filename = Path.GetFileName(name);

                if (!filename.EndsWith(".cs"))
                    continue;

                Stream zipStream = file.GetInputStream(zipEntry);

                var bytes = ReadEntryFromStream(zipStream);
                var lines = SplitLines(Encoding.UTF8.GetString(bytes));

                AppendAttributeFile(filename, lines, ref data);
            }
        }
        finally
        {
            if (file != null)
            {
                file.IsStreamOwner = true; // Makes close also shut the underlying stream
                file.Close(); // Ensure we release resources
            }
        }
    }
    
    private static byte[] ReadEntryFromStream(Stream stream)
    {
        bool canRead = true;
        List<byte> bytes = new List<byte>();
        while (canRead)
        {
            const int size = 4096;
            byte[] buffer = new byte[size];
            int readCount = stream.Read(buffer, 0, size);
            if (readCount == -1)
                break;

            if (readCount > 0 && readCount < size)
            {
                buffer = buffer.Take(readCount).ToArray();
                canRead = false;
            }
            bytes.AddRange(buffer);
        }

        return bytes.ToArray();
    }
    
    public static string[] SplitLines(this string input)
    {
        return input.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    }
}
