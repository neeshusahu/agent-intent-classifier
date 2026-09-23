namespace IntentClassifier.Tests;

public class IntentClassifierToolsTests
{
    private PlanStore _store = null!;
    private IntentClassifierTools _tools = null!;

    [SetUp]
    public void SetUp()
    {
        _store = new PlanStore();
        _tools = new IntentClassifierTools(_store);
    }

    [Test]
    public void ClassifyIntent_AllReadOperations_IsPlannedForExecution()
    {
        var operations = new List<PlanOperation>
        {
            new("azure.read.status", "document-api", "check health")
        };

        var plan = _tools.ClassifyIntent(operations);

        Assert.That(plan.ApprovalRequired, Is.False);
        Assert.That(plan.Status, Is.EqualTo(Status.Planned_For_Execution));
    }

    [Test]
    public void ClassifyIntent_ContainsWriteOperation_RequiresApproval()
    {
        var operations = new List<PlanOperation>
        {
            new("azure.read.status", "document-api", "check health"),
            new("azure.write.restart", "document-api", "recover from failure")
        };

        var plan = _tools.ClassifyIntent(operations);

        Assert.That(plan.ApprovalRequired, Is.True);
        Assert.That(plan.Status, Is.EqualTo(Status.Pending_Approval));
    }

    [Test]
    public void ClassifyIntent_EmptyOperations_ReturnsSuspendedPlan()
    {
        var plan = _tools.ClassifyIntent(new List<PlanOperation>());

        Assert.That(plan.Status, Is.EqualTo(Status.Suspended));
        Assert.That(plan.ApprovalRequired, Is.True);
    }

    [Test]
    public void ClassifyIntent_StoresPlanInPlanStore()
    {
        var operations = new List<PlanOperation>
        {
            new("azure.read.status", "document-api", "check health")
        };

        var plan = _tools.ClassifyIntent(operations);

        Assert.That(_store.GetPlan(plan.Id), Is.EqualTo(plan));
    }
}
