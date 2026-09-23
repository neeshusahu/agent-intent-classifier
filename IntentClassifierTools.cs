
using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
public class IntentClassifierTools
{
    private readonly PlanStore _store;

    public IntentClassifierTools(PlanStore store)
    {
        _store = store;
    }

    [McpServerTool]
    [Description(
          "Declare the Azure operations you intend to perform BEFORE acting. This tool does not execute anything; " +
        "it classifies your declared intent and returns an instruction you must follow. " +
        "Each operation must be one of: " + AzureOperationDictionary.ValidValuesText + ". " +
        "scope must be the actual Azure resource name or ID the operation targets, never a file path or task description. " +
        "reason should briefly say why the operation is needed. " +
        "Any write operation requires human approval: if the status is Pending_Approval, do not act; tell the user and wait.")]
    public Plan ClassifyIntent(List<PlanOperation> operations)
    {
        var planId = Guid.NewGuid();
        Plan? currentPlan = null;

        try
        {
            
            var planOperations = new List<PlanOperation>();
            foreach (var operation in operations)
            {
               
                planOperations.Add(new PlanOperation(operation.Operation, operation.Scope, operation.Reason));
            }
             if (operations is null || operations.Count == 0)
                throw new ArgumentException("Plan contains no operations.");
      

            bool approvalRequired = planOperations.Any(op => AzureOperationDictionary.RequiresApproval(op.Operation));

            currentPlan = new Plan
            {
                Id = planId,
                Operations = planOperations,
                ApprovalRequired = approvalRequired,
                CreatedAt = DateTime.UtcNow,
                Status = approvalRequired ? Status.Pending_Approval : Status.Planned_For_Execution,
                Instructions = approvalRequired
                    ? "STOP. Do not perform this action. It requires human approval — surface it to the user and wait."
                    : "Safe to proceed. Execute this operation yourself using your available tools."
            };
        }
        catch (Exception ex)
        {
            currentPlan = new Plan
            {
                Id = planId,
                Operations = new List<PlanOperation>()
                {
                    new PlanOperation("exception", ex.Message, "Exception occured")
                },
                ApprovalRequired = true,
                Status = Status.Suspended,
                CreatedAt = DateTime.UtcNow,
                Instructions = "STOP. An exception occurred while classifying this operation."
            };
        }
        finally
        {
            _store.AddPlan(currentPlan);
        }

        return currentPlan;
    }
}


