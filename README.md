# IntentClassifier

An MCP tool that makes AI agents **declare the Azure operations they intend to perform before acting**. Reads are cleared to proceed; writes are stopped until a human approves.

## The problem

Coding agents run in your terminal with your permissions. If you have ever run `az login`, the Azure CLI has a cached session under `~/.azure/`, and any command the agent runs uses it. No password prompt and no approval step, just your identity.

When asked how it got access, Claude Code explained exactly that:
<img width="776" height="212" alt="problem-cached-credentials" src="https://github.com/user-attachments/assets/ea0c62b8-e518-416a-ac34-a07c297b1dd0" />


The agent didn't do anything wrong: it used what was available. That's the point. A single tool call can restart, scale, stop, or delete a production resource, and nothing asks you first.

IntentClassifier adds a checkpoint: before touching Azure, the agent states *what* it wants to do, *where*, and *why*, and gets back a clear instruction to proceed or stop.

## How it works

```
Agent ──classify_intent(operations)──▶ IntentClassifier
                                          │
                     reads only ◀─────────┼─────────▶ any write / unknown / error
                         │                                    │
          "Safe to proceed."                "STOP. Requires human approval."
          Agent runs the read                Agent surfaces it to the user and waits
```

1. The agent calls `classify_intent` with one or more operations, each with an `operation`, `scope`, and `reason`.
2. Each operation is checked against a fixed catalog.
3. If **any** operation is a write, the whole plan requires approval.
4. The plan is stored and returned with a status and an instruction.

The tool **does not execute anything**. The agent still performs the work with its own Azure tools.

## Demo

Tested with **GitHub Copilot** (VS Code agent mode) and **Claude Code**.

### Read: proceeds without approval

The agent declares `azure.read.status`, gets `Planned_For_Execution`, and fetches the status.

**GitHub Copilot**
<img width="981" height="780" alt="copilot-read" src="https://github.com/user-attachments/assets/4b17765b-a4b3-48b4-bcbe-1a3affcd32f0" />


**Claude Code**

<img width="904" height="496" alt="claude-code-read" src="https://github.com/user-attachments/assets/3ab00fab-fd00-488c-acf1-c7340fb90afc" />


### Write: stops and asks

The agent declares `azure.write.stop`, gets `Pending_Approval`, and asks the user before doing anything.

**GitHub Copilot**
<img width="967" height="803" alt="copilot-write" src="https://github.com/user-attachments/assets/2585f035-e75e-4462-b8fe-579bbc902589" />


**Claude Code**

<img width="915" height="328" alt="claude-code-write" src="https://github.com/user-attachments/assets/d7a619f0-410b-4b26-a01a-1594e26e3842" />


## Operations

| Read (auto-approved) | Write (requires approval) |
|---|---|
| `azure.read.status` | `azure.write.restart` |
| `azure.read.logs` | `azure.write.stop` |
| `azure.read.metrics` | `azure.write.start` |
| `azure.read.config` | `azure.write.scale` |
| `azure.read.diagnostics` | `azure.write.delete` |
| `azure.list.resources` | `azure.write.deploy` |
| | `azure.write.config` |

## Statuses

| Status | When | Instruction to the agent |
|---|---|---|
| `Planned_For_Execution` | All operations are reads | Safe to proceed |
| `Pending_Approval` | At least one write | Stop, surface to the user, wait |
| `Suspended` | Empty plan or classification error | Stop |

## Request and response

**Request**
```json
{
  "operations": [
    {
      "operation": "azure.write.stop",
      "scope": "document-api",
      "reason": "User requested to stop the document-api App Service"
    }
  ]
}
```

**Response**
```json
{
  "id": "dd1553b2-bda0-490e-a30b-cb76b049e3d8",
  "operations": [
    {
      "operation": "azure.write.stop",
      "scope": "document-api",
      "reason": "User requested to stop the document-api App Service"
    }
  ],
  "approvalRequired": true,
  "status": "Pending_Approval",
  "instructions": "STOP. Do not perform this action. It requires human approval — surface it to the user and wait."
}
```

## Getting started

**Requirements:** .NET 8 or later.

```bash
git clone https://github.com/<your-username>/IntentClassifier.git
cd IntentClassifier
dotnet run
```

The MCP endpoint is served over HTTP at `http://localhost:5050/mcp`.

### GitHub Copilot (VS Code agent mode)

Add the server to `.vscode/mcp.json`:

```json
{
  "servers": {
    "intent-classifier": {
      "type": "http",
      "url": "http://localhost:5050/mcp"
    }
  }
}
```

### Claude Code

```bash
claude mcp add --transport http intent-classifier http://localhost:5050/mcp
```

### Tell the agent to use it

Add an instruction to your agent's instructions file, for example `.github/copilot-instructions.md`:

```markdown
Before any Azure operation, call classify_intent and follow the returned instruction.
```

## Project structure

```
IntentClassifier/
├── PlanTools.cs                  # classify_intent MCP tool
├── AzureOperationDictionary.cs   # operation catalog and read/write classification
├── PlanStore.cs                  # stores submitted plans
├── Models.cs                     # Plan, PlanOperation, Status
└── Program.cs                    # MCP server host
```

## Limitations

IntentClassifier is a **cooperative** checkpoint, not an enforcement layer.

- **The agent still holds your credentials.** It checks what the agent intends; it does not take away the cached `az` session.
- **It relies on the agent to call it and follow it.** An agent that skips the tool, or is prompt-injected into ignoring it, is not blocked.
- **It classifies what the agent declares, not what it runs.** An agent may declare one read and also list subscriptions or resources while locating the target.
- **Scope is free text.** The resource name is not checked against Azure.
- **Plans are stored in memory** and are lost on restart.

## Roadmap

- **Compare declared plans against the Azure Activity Log.** Match stored plans to write events in the Activity Log and flag any write that had no matching plan, or that ran while its plan was still `Pending_Approval`. This keeps the tool advisory but makes skipped or ignored declarations visible.

