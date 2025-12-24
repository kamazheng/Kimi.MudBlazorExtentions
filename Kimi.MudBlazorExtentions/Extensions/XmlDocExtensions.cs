
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kimi.MudBlazorExtentions.Extensions;

public static class XmlDocExtensions
{
    private static XDocument? _doc;
    private static readonly object _lock = new();

    /// <summary>
    /// Blazor WASM 端初始化：从 wwwroot 通过 HttpClient 拉取 XML。
    /// </summary>
    public static async Task InitAsync(HttpClient httpClient, string xmlPath = "CDU_TMES.Shared.xml")
    {
        var xml = await httpClient.GetStringAsync(xmlPath);
        var doc = XDocument.Parse(xml);
        lock (_lock) _doc = doc;
    }

    /// <summary>
    /// 服务器端初始化：从绝对路径加载 XML 文档。
    /// </summary>
    public static void InitFromFile(string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
            throw new ArgumentException("XML path cannot be null or empty.", nameof(fullPath));

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"XML documentation file not found: {fullPath}", fullPath);

        var doc = XDocument.Load(fullPath);
        lock (_lock) _doc = doc;
    }



    /// <summary>
    /// 尝试在 BaseDirectory 及子目录中搜索并加载 XML；成功返回 true，失败返回 false。
    /// </summary>
    public static bool TryInitFromBaseDirectory(string xmlFileName)
    {
        if (string.IsNullOrWhiteSpace(xmlFileName))
            return false;
        var baseDir = AppContext.BaseDirectory;
        var directPath = Path.Combine(baseDir, xmlFileName);
        if (File.Exists(directPath))
        {
            InitFromFile(directPath);
            return true;
        }
        foreach (var path in Directory.EnumerateFiles(baseDir, "*", SearchOption.AllDirectories))
        {
            if (!string.Equals(Path.GetFileName(path), xmlFileName, StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                InitFromFile(path);
                return true;
            }
            catch
            {
                // 如果某个文件读失败，继续找下一个
                continue;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns the &lt;summary&gt; text for a Type (or null if missing).
    /// </summary>
    public static string? GetXmlSummary(this Type type)
        => GetSummaryById($"T:{type.FullName}");

    /// <summary>
    /// Returns the &lt;summary&gt; text for a Property (or null if missing).
    /// </summary>
    public static string? GetXmlSummary(this PropertyInfo prop)
    {
        var id = $"P:{prop.DeclaringType!.FullName}.{prop.Name}";
        return GetSummaryById(id);
    }

    /// <summary>
    /// Returns the &lt;summary&gt; text for a Field (or null if missing).
    /// </summary>
    public static string? GetXmlSummary(this FieldInfo field)
    {
        var id = $"F:{field.DeclaringType!.FullName}.{field.Name}";
        return GetSummaryById(id);
    }

    /// <summary>
    /// Returns the &lt;summary&gt; text for a Method (or null if missing).
    /// </summary>
    public static string? GetXmlSummary(this MethodInfo method)
    {
        var id = BuildMethodDocId(method);
        return GetSummaryById(id);
    }

    /// <summary>
    /// Returns the &lt;summary&gt; text for an Enum Type (or null if missing).
    /// </summary>
    public static string? GetXmlSummary(this Enum enumValue)
    {
        var enumType = enumValue.GetType();
        var memberName = Enum.GetName(enumType, enumValue);
        if (memberName == null) return null;

        var id = $"F:{enumType.FullName}.{memberName}";
        return GetSummaryById(id);
    }

    /// <summary>
    /// Returns the &lt;summary&gt; text for an Enum Type (or null if missing).
    /// </summary>
    public static string? GetXmlEnumSummary(this Type enumType)
    {
        if (!enumType.IsEnum) return null;
        return GetSummaryById($"T:{enumType.FullName}");
    }

    /// <summary>
    /// Returns the &lt;summary&gt; text for a specific enum field value (or null if missing).
    /// </summary>
    public static string? GetXmlEnumFieldSummary(this Type enumType, string fieldName)
    {
        if (!enumType.IsEnum) return null;
        var id = $"F:{enumType.FullName}.{fieldName}";
        return GetSummaryById(id);
    }

    /// <summary>
    /// Returns all enum field names and their summaries for an enum type.
    /// </summary>
    public static Dictionary<string, string?> GetXmlEnumFieldSummaries(this Type enumType)
    {
        if (!enumType.IsEnum) return [];

        var result = new Dictionary<string, string?>();
        var fieldNames = Enum.GetNames(enumType);

        foreach (var fieldName in fieldNames)
        {
            var summary = GetXmlEnumFieldSummary(enumType, fieldName);
            result[fieldName] = summary;
        }

        return result;
    }


    // ----------------- Internals ------------------------------------------------------------------------------------------------------

    private static string? GetSummaryById(string docId)
    {
        // Not initialized yet
        if (_doc is null) return null;

        var member = _doc.Descendants("member")
                         .FirstOrDefault(m => (string?)m.Attribute("name") == docId);
        var summary = member?
            .Elements("summary")
            .FirstOrDefault()?
            .Value?
            .Trim();

        return string.IsNullOrWhiteSpace(summary) ? null : NormalizeWhitespaceMultiline(summary);
    }

    private static string BuildMethodDocId(MethodInfo method)
    {
        var declaring = method.DeclaringType!.FullName;
        var name = method.Name;
        var parameters = method.GetParameters();

        if (parameters.Length == 0)
            return $"M:{declaring}.{name}";

        var paramList = string.Join(",", parameters.Select(p => ToDocTypeName(p.ParameterType)));
        return $"M:{declaring}.{name}({paramList})";
    }

    private static string ToDocTypeName(Type t)
    {
        // Covers common cases; refine for complex nested/generic/ByRef/pointer signatures if needed
        if (t.IsGenericType)
        {
            var baseName = t.GetGenericTypeDefinition().FullName!.Split('`')[0];
            var args = t.GetGenericArguments().Select(ToDocTypeName);
            return $"{baseName}{{{string.Join(",", args)}}}";
        }
        if (t.IsArray)
        {
            return $"{ToDocTypeName(t.GetElementType()!)}[]";
        }
        // Handle ByRef (e.g., ref int -> System.Int32&)
        if (t.IsByRef)
        {
            return $"{ToDocTypeName(t.GetElementType()!)}&";
        }
        return t.FullName!;
    }


    private static string? NormalizeWhitespaceMultiline(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;

        var normalizedNewlines = s.Replace("\r\n", "\n").Replace("\r", "\n");

        var lines = normalizedNewlines
            .Split('\n')
            .Select(line => Regex.Replace(line.Trim(), @"\s+", " "))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        return lines.Count == 0 ? null : string.Join(Environment.NewLine, lines);
    }
}
