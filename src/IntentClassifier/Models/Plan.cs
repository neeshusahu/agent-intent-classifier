public record Plan
{
    public Guid Id {get; set;}
    public List<PlanOperation> Operations {get;set;}

    public bool ApprovalRequired {get;set;}

    public Status Status {get;set;}
    public DateTime CreatedAt {get;set;}

   public string Instructions {get;set;}
  

}

public enum Status
{
    Planned_For_Execution,
    Pending_Approval,
    Suspended,
    Failed

}