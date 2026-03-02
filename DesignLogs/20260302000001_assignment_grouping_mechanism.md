# Assignment Grouping Mechanism

**Date:** 2026-03-02  
**Scope:** Assignment Module — API response restructure and frontend grouping

---

## Context

Previously, `GET /assignments/my` returned a flat array of all assignments belonging to the user's group under a single `data` key. The frontend was responsible for deriving status (active vs. completed) by applying client-side filters based on `isCompleted` and `dueDate`.

---

## Decision

The API response was restructured to return three pre-categorised groups, with server-side authoritative classification:

```json
{
  "data": {
    "active":  [ ...MyAssignmentItem ],
    "done":    [ ...MyAssignmentItem ],
    "missing": [ ...MyAssignmentItem ]
  }
}
```

### Grouping Rules (applied server-side in `GetMyAssignmentsEndpoint`)

| Group     | Condition                                                                    |
| --------- | ---------------------------------------------------------------------------- |
| `done`    | `IsCompleted == true` — regardless of due date                               |
| `missing` | `IsCompleted == false` AND `DueDate` is in the past                          |
| `active`  | `IsCompleted == false` AND (`DueDate` is null OR `DueDate` is in the future) |

Each group is ordered by `DueDate` descending.

---

## Motivation

- **Correctness:** Client-side classification was fragile — edge cases around file uploads without explicit completion and due-date timezone handling led to incorrect status badges.
- **Single source of truth:** The server owns the business logic for what "done", "active", and "missing" means. Clients simply render.
- **Dashboard relevance:** The dashboard only needs to surface `active` assignments to prompt action. The server now makes this trivial without a client-side filter.

---

## Frontend Impact

### `dashboard.vue`

- Type updated from `{ data: MyAssignmentItem[] }` to `{ data: MyAssignmentsResponse }`.
- `activeAssignments` computed now reads directly from `data.active` — no client filtering.
- Removed client-side `isCompleted` predicate.

### `assignments/index.vue`

- Type updated to match the new grouped response.
- Template updated to render three distinct sections: **Active**, **Not Submitted** (missing), and **Done**.
- Removed `isAssignmentCompleted()` helper function — classification is now entirely server-driven.
- Empty state condition updated to check total count across all three groups.

---

## Trade-offs

- If classification rules change, only the backend needs updating — no frontend deploy required.
- The `missing` category explicitly separates "past due and unsubmitted" from "done", making it clearer to students and admins which assignments need attention.
