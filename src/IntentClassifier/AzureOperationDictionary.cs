public static class AzureOperationDictionary
{
    public static readonly Dictionary<string, bool> ApprovalRequired = new()
    {
        // Reads — auto-execute, no approval needed
        { "azure.read.status",          false },
        { "azure.read.logs",            false },
        { "azure.read.metrics",         false },
        { "azure.read.config",          false },
        { "azure.read.list-resources",  false },
        { "azure.read.diagnostics",     false },

        // Writes — require human approval
        { "azure.write.restart",        true },
        { "azure.write.scale",          true },
        { "azure.write.update-config",  true },
        { "azure.write.delete",         true },
        { "azure.write.deploy",         true },
        { "azure.write.start",          true },
        { "azure.write.stop",           true },
    };

    public const string ValidValuesText =
        "azure.read.status, azure.read.logs, azure.read.metrics, azure.read.config, " +
        "azure.read.list-resources, azure.read.diagnostics, azure.write.restart, " +
        "azure.write.scale, azure.write.update-config, azure.write.delete, " +
        "azure.write.deploy, azure.write.start, azure.write.stop";

    public static bool RequiresApproval(string operationKey)
    {
        if (ApprovalRequired.TryGetValue(operationKey, out var needsApproval))
        {
            return needsApproval;
        }

        // Fail-safe default for unrecognized operations:
        // treat as requiring approval unless explicitly listed as safe.
        return true;
    }
}