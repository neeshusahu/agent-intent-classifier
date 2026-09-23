 using System.ComponentModel;
public record PlanOperation(
    [property: Description(
        "Must be one of the exact operation keys: azure.read.status, azure.read.logs, " +
        "azure.read.metrics, azure.read.config, azure.read.list-resources, " +
        "azure.read.diagnostics, azure.write.restart, azure.write.stop, azure.write.start, " +
        "azure.write.scale, azure.write.delete, azure.write.deploy, azure.write.update-config. " +
        "Do not use free-text descriptions.")]
    string Operation,

    [property: Description(
        "The exact Azure resource name or resource ID this operation targets " +
        "(e.g. an App Service name like 'document-api'). Never a local file path.")]
    string Scope,
    
    [property: Description("Briefly, why this operation is needed.")]
    string Reason);


