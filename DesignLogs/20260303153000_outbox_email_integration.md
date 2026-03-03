# Outbox + Email Integration – Assignment Created Notification

| Field | Value |
|-------|-------|
| **Date** | 2026-03-03 |
| **Author** | Copilot (antigravity) |
| **Significance** | 🟠 Moderate |
| **Status** | ✅ Done (Mock Data) |

---

## Summary

Wire the **EmailModule** into the **Outbox Pattern** so that creating an assignment automatically triggers an email notification to all group members — without blocking the HTTP request or coupling SMTP within the DB transaction.

---

## Motivation

The EmailModule (`20260301151928_poc_email_module.md`) was previously only reachable via standalone endpoints (`POST /api/email/send-test`, `POST /api/email/group-assignment-preview`). There was no integration with actual business operations.

The production requirement is: **when an admin creates an assignment, all group members should receive an email notification**. Sending emails directly inside `CreateAssignmentEndpoint` (synchronous, in-request) would:

1. Slow down the HTTP response (SMTP round-trip)
2. Couple a transient side-effect into the DB transaction
3. Provide no retry if SMTP fails

The Outbox Pattern already exists and solves all three. The email notification is a perfect fit for this channel.

---

## Design Decision

### Two-Stage Event Flow

```
CreateAssignmentEndpoint
  └─ Assignment.Create()
       └─ entity.Raise(AssignmentCreatedEvent)          ← synchronous, in-transaction
            └─ AssignmentCreatedEmailEventHandler       ← IDomainEventHandler<AssignmentCreatedEvent>
                 └─ outboxService.StoreManyAsync(       ← writes N GroupAssignmentEmailRequestedEvent rows
                      GroupAssignmentEmailRequestedEvent × N groups
                    )

[60s later — OutboxProcessorHostedService poll cycle]
  └─ GroupAssignmentEmailRequestedEventHandler          ← IDomainEventHandler<GroupAssignmentEmailRequestedEvent>
       └─ SmtpEmailService.SendGroupAssignmentNotificationAsync()  ← actual SMTP call
```

Two events are intentionally used:

| Event | Role |
|-------|------|
| `AssignmentCreatedEvent` | Domain event raised by the aggregate — signals that an assignment was saved |
| `GroupAssignmentEmailRequestedEvent` | Outbox event — one per group, carries all data needed to send the email without any DB query at send time |

This separation ensures SMTP handlers are always pure and isolated — they receive a self-contained payload and need no DB access.

### Why one outbox event per group (not per participant)

| Option | Tradeoff |
|--------|----------|
| One event per group | If send partially fails, retry resends to the whole group — acceptable for notifications |
| One event per participant | Finer retry granularity, but N×participants outbox rows (noisy at scale) |

One-per-group was chosen as the right balance for this use case.

---

## Files Added / Modified

| File | Change | Notes |
|------|--------|-------|
| `AssignmentModule/Domain/Events/AssignmentCreatedEvent.cs` | **New** | `record AssignmentCreatedEvent(Assignment Assignment) : IDomainEvent` |
| `AssignmentModule/Domain/Assignment.cs` | **Modified** | `Create()` now calls `entity.Raise(new AssignmentCreatedEvent(entity))` |
| `EmailModule/Events/GroupAssignmentEmailRequestedEvent.cs` | **New** | Flat-primitive outbox payload record; one per group |
| `EmailModule/Events/AssignmentCreatedEmailEventHandler.cs` | **New** | Bridges domain event → outbox rows; currently uses mock data |
| `EmailModule/Events/GroupAssignmentEmailRequestedEventHandler.cs` | **New** | Background SMTP handler; calls `SmtpEmailService` |

---

## Mock Data (current PoC state)

`AssignmentCreatedEmailEventHandler` uses hardcoded groups instead of querying the DB. The actual DB query is present but commented under the `// [Actual Code]` block.

| Group | Participant | Email |
|-------|-------------|-------|
| Alpha Team | Verio Renedominick | `verio.renedominick@gmail.com` |
| Alpha Team | Aber Ossy Verio | `aberossyverio@gmail.com` |
| Beta Team | Verio Renedominick (Student) | `verio.renedominick@student.matanauniversity.ac.id` |
| Beta Team | Verio Nobodyknows | `verio.nobodyknows@gmail.com` |

---

## Live Test Results

Tested on Docker (port 8080) against real Gmail SMTP on 2026-03-03.

### Test 1 — Assignment id=5 (first run)

```
POST /api/assignments
Body: {"title":"Test Assignment via Outbox","description":"Testing email notification pipeline","dueDate":null}
```

Appeared to produce 4× emails per address. **Root cause** was not a bug — assignment id=4 had been created 6 seconds prior in the same session from a terminal in a broken state, and both sets of outbox events were processed in the same 60s cycle. 2 assignments × 2 groups = 4 outbox events processed together.

### Test 2 — Assignment id=6 (clean run)

```
POST /api/assignments
Body: {"title":"Clean Test - One Assignment Only","description":"Verifying exactly 1 assignment = 2 outbox events","dueDate":null}
```

| Metric | Result |
|--------|--------|
| Outbox events written at create time | **2** (one per group) |
| `outbox_messages` before poll | 2 |
| Groups notified | Alpha Team ✓, Beta Team ✓ |
| SMTP calls | **4** (one per participant per group) |
| `outbox_messages` after poll | **0** |
| `outbox_processed_messages` inserted | **2** |
| Emails received in inbox | ✅ Confirmed |

```
[AssignmentEmail] Sending assignment notification for 'Clean Test - One Assignment Only' to group 'Alpha Team' (2 participants)
[AssignmentEmail] Sending assignment notification for 'Clean Test - One Assignment Only' to group 'Beta Team' (2 participants)
[AssignmentEmail] Notification sent successfully for group 'Alpha Team'
[AssignmentEmail] Notification sent successfully for group 'Beta Team'
DELETE FROM outbox_messages (×2)
INSERT INTO outbox_processed_messages (×2)
```

---

## Known Risk — At-Least-Once Delivery

The outbox processor sends emails first, then deletes the outbox rows in a single `SaveChangesAsync`. If the app crashes **after** SMTP delivery but **before** the delete commits, the message will be retried and the email will be sent again.

| Risk | Severity | Mitigation |
|------|----------|------------|
| Duplicate email on crash | Low for notifications | Acceptable tradeoff for PoC |
| Duplicate email on crash | High for transactional email (invoice, payment) | Requires idempotency key on recipient side |

This is a known property of at-least-once delivery and is standard in outbox pattern implementations.

---

## PoC vs Production

| Concern | PoC (current) | Production (future) |
|---------|---------------|---------------------|
| Recipient source | Hardcoded mock groups | Query `Groups.Include(Members).ThenInclude(User)` (code already written, commented out) |
| Assignment event | Raised on all creates | Same — domain event is always raised |
| SMTP retry | Via outbox `RetryCount` / `MaxResilienceCount=5` | Same |
| Email deduplication | None | Add idempotency key if transactional |

---

## Production Checklist

- [ ] Uncomment the `// [Actual Code]` block in `AssignmentCreatedEmailEventHandler.cs`
- [ ] Remove the mock group block above it
- [ ] Remove the retained `private readonly ApplicationDbContext _dbContext = dbContext;` suppression line
- [ ] Verify `Group → GroupMember → User.Email` navigation properties are correctly included
- [ ] Add `Roles("admin")` enforcement review for `CreateAssignmentEndpoint` (already present)
- [ ] Consider idempotency key if email type becomes transactional
