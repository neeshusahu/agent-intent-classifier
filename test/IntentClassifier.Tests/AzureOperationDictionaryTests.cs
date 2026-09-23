namespace IntentClassifier.Tests;

public class AzureOperationDictionaryTests
{
    [TestCase("azure.read.status")]
    [TestCase("azure.read.logs")]
    [TestCase("azure.read.metrics")]
    [TestCase("azure.read.config")]
    [TestCase("azure.read.list-resources")]
    [TestCase("azure.read.diagnostics")]
    public void RequiresApproval_ReadOperations_ReturnsFalse(string operationKey)
    {
        Assert.That(AzureOperationDictionary.RequiresApproval(operationKey), Is.False);
    }

    [TestCase("azure.write.restart")]
    [TestCase("azure.write.scale")]
    [TestCase("azure.write.update-config")]
    [TestCase("azure.write.delete")]
    [TestCase("azure.write.deploy")]
    [TestCase("azure.write.start")]
    [TestCase("azure.write.stop")]
    public void RequiresApproval_WriteOperations_ReturnsTrue(string operationKey)
    {
        Assert.That(AzureOperationDictionary.RequiresApproval(operationKey), Is.True);
    }

    [Test]
    public void RequiresApproval_UnknownOperation_DefaultsToTrue()
    {
        Assert.That(AzureOperationDictionary.RequiresApproval("azure.write.unknown-op"), Is.True);
    }
}
