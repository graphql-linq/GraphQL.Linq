// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

[ExcludeFromCodeCoverage]
public static class Approvals
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions {
        WriteIndented = true,
        MaxDepth = 100,
    };

    public static void Verify(object? obj, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
    {
        var str = obj switch {
            null => "",
            string s => s,
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture),
            double d => d.ToString(CultureInfo.InvariantCulture),
            float f => f.ToString(CultureInfo.InvariantCulture),
            decimal m => m.ToString(CultureInfo.InvariantCulture),
            bool b => b.ToString(CultureInfo.InvariantCulture),
            _ => JsonSerializer.Serialize(obj, _jsonOptions),
        };
        str.ShouldMatchApproved(sourceFilePath, memberName);
    }

    public class ShouldMatchApprovedOptions
    {
        public string Discriminator { get; set; } = string.Empty;
    }

    public static void ShouldMatchApproved(this string received, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
    {
        ShouldMatchApproved(received, _ => { }, sourceFilePath, memberName);
    }

    private static readonly ConcurrentDictionary<string, object> _approvalFileLocks = new();
    public static void ShouldMatchApproved(this string received, Action<ShouldMatchApprovedOptions> configurator, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
    {
        var options = new ShouldMatchApprovedOptions();
        configurator(options);

        // Get the directory and base filename
        var directory = Path.GetDirectoryName(sourceFilePath) ?? throw new InvalidOperationException("Could not determine source file directory");
        var sourceFileName = Path.GetFileNameWithoutExtension(sourceFilePath);

        // Construct approved and received file paths
        var discriminator = string.IsNullOrEmpty(options.Discriminator) ? "" : $".{new string(options.Discriminator.Where(c => char.IsLetterOrDigit(c) || c == '.').ToArray())}";
        var approvedFile = Path.Combine(directory, $"{sourceFileName}.{memberName}{discriminator}.approved.txt");
        var receivedFile = Path.Combine(directory, $"{sourceFileName}.{memberName}{discriminator}.received.txt");

        // Ensure thread-safe access per approval file (as some test files, and hence approval files, are shared across projects)
        var fileLock = _approvalFileLocks.GetOrAdd(approvedFile, _ => new object());
        lock (fileLock) {
            // Normalize line endings to \r\n for consistency
            var normalizedReceived = received.Replace("\r\n", "\n").Replace("\n", "\r\n");

            // Write the received content
            File.WriteAllText(receivedFile, normalizedReceived);

            // Check if approved file exists
            if (!File.Exists(approvedFile)) {
#if CI
            throw new InvalidOperationException($"Approved file not found: {approvedFile}\nReceived content written to: {receivedFile}");
#else
                // In local development, copy received to approved
                File.Copy(receivedFile, approvedFile, overwrite: true);
                return;
#endif
            }

            // Read and normalize approved content
            var approved = File.ReadAllText(approvedFile);
            var normalizedApproved = approved.Replace("\r\n", "\n").Replace("\n", "\r\n");

            // Compare the contents
            if (normalizedApproved != normalizedReceived) {
                var diffContext = GetDiffContext(normalizedApproved, normalizedReceived);
                var message = $"Approval test failed for {memberName}\n" +
                             $"Approved file: {approvedFile}\n" +
                             $"Received file: {receivedFile}\n" +
                             $"The received content does not match the approved content.\n" +
                             diffContext;
                throw new InvalidOperationException(message);
            }

            // If they match, delete the received file
            if (File.Exists(receivedFile)) {
                File.Delete(receivedFile);
            }
        }
    }

    private static string GetDiffContext(string approved, string received)
    {
        // Find the first position where the strings differ
        int diffIndex = 0;
        int minLength = Math.Min(approved.Length, received.Length);

        while (diffIndex < minLength && approved[diffIndex] == received[diffIndex]) {
            diffIndex++;
        }

        // If strings are identical up to the shorter length, the difference is at the end
        if (diffIndex == minLength && approved.Length != received.Length) {
            diffIndex = minLength;
        }

        // Go back 20 characters from the diff point (or to start if less than 20)
        int contextStart = Math.Max(0, diffIndex - 20);

        // Take 60 characters from the context start (or to end if less available)
        int approvedContextLength = Math.Min(60, approved.Length - contextStart);
        int receivedContextLength = Math.Min(60, received.Length - contextStart);

        string approvedContext = approved.Substring(contextStart, approvedContextLength);
        string receivedContext = received.Substring(contextStart, receivedContextLength);

        // Escape special characters for better readability
        approvedContext = approvedContext.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
        receivedContext = receivedContext.Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");

        return $"\nDifference at position {diffIndex}:\n" +
               $"Expected: ...{approvedContext}...\n" +
               $"Actual:   ...{receivedContext}...";
    }
}
