// © 2025 American Community Developers, Inc. All Rights Reserved. See LICENSE.txt for details.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[ExcludeFromCodeCoverage]
public static class ObjectApprover
{
    public static void Verify(object? obj, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
    {
        Approvals.Verify(obj, sourceFilePath, memberName);
    }
}
