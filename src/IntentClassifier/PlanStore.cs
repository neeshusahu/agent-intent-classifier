using System.Collections.Concurrent;

public class PlanStore
{
    private readonly ConcurrentDictionary<Guid, Plan> planStore;

    public PlanStore()
    {
        planStore = new ConcurrentDictionary<Guid, Plan>();
    }

    public void AddPlan(Plan plan)
    {
        if (plan == null)
            return;

        planStore.TryAdd(plan.Id, plan);
    }

    public Plan? GetPlan(Guid id)
    {
        planStore.TryGetValue(id, out var plan);
        return plan;
    }

    public IReadOnlyList<Plan> GetAllPlans()
    {
        return planStore.Values.OrderBy(p => p.CreatedAt).ToList();
    }

    public void ClearStore()
    {
        planStore.Clear();
    }
}
