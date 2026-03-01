# Assignment Submission — Backend Implementation Divergence

| Field            | Value                 |
| ---------------- | --------------------- |
| **Date**         | 2026-03-01            |
| **Author**       | Copilot (antigravity) |
| **Significance** | 🟡 Minor              |
| **Status**       | ✅ APPROVED           |

---

## Summary

During frontend implementation, four divergences were discovered between the original/revised design logs (`20260223143000_group_and_assignment_module.md`, revision 2026-02-27) and the actual backend implementation. None of the divergences break the design intent — they are clarifications or practical simplifications. This log documents the actual backend shapes so the frontend can be built against reality.

---

## Divergence 1 — `GET /api/assignments/my` Returns a Flat Response

### Design Expected

Each item in the list would carry a nested `submission` object:

```json
{
	"id": 1,
	"title": "...",
	"submission": {
		"id": 42,
		"isCompleted": false,
		"completedAt": null,
		"files": []
	}
}
```

### Actual Implementation

`GetMyAssignmentsEndpoint` returns a **flat** `MyAssignmentItem` record. No nested `submission` object. Files are not returned inline — only a count.

```csharp
public record MyAssignmentItem(
    int Id,
    string Title,
    string? Description,
    DateTime? DueDate,
    int SubmissionId,
    bool IsCompleted,
    DateTime? CompletedAt,
    int FileCount);
```

**JSON shape:**

```json
{
	"id": 1,
	"title": "...",
	"description": null,
	"dueDate": null,
	"submissionId": 42,
	"isCompleted": false,
	"completedAt": null,
	"fileCount": 0
}
```

### Rationale

Avoids over-fetching file data on a list view. The full submission (with files) is fetched separately via `GET /api/assignments/{assignmentId}/submission/my` when viewing the detail page.

### Behaviour Change — Eager Submission Creation

`GET /api/assignments/my` **eagerly creates** `AssignmentSubmission` rows for every assignment that does not yet have one for the caller's group. This satisfies the one-submission-per-group-per-assignment invariant at list-query time rather than lazily on first file upload. Every item in the response is therefore guaranteed to have a real `submissionId`.

---

## Divergence 2 — `GET /api/assignments/{id}` Returns Metadata Only (No Submissions)

### Design Expected

Implicit assumption (carried from the admin UI design) that the detail endpoint returned submissions inline.

### Actual Implementation

`GetAssignmentDetailEndpoint` returns **only assignment metadata**:

```csharp
public record AssignmentDetail(
    int Id,
    string Title,
    string? Description,
    DateTime? DueDate,
    DateTime CreateTime,
    DateTime? UpdateTime);
```

Submissions must be fetched separately via `GET /api/assignments/{id}/submissions`.

### Admin Submission List Shape

`QueryAssignmentSubmissionsEndpoint` (`GET /api/assignments/{id}/submissions`) returns a paginated list of `SubmissionListItem`:

```csharp
public record SubmissionListItem(
    int? SubmissionId,
    int GroupId,
    string GroupName,
    bool IsCompleted,
    DateTime? CompletedAt,
    int FileCount,
    DateTime? CreateTime);
```

- `SubmissionId` is nullable — groups that have not yet submitted will appear with `null`.
- `GroupName` is a flat string (no nested `group` object).
- `FileCount` is an integer (no nested `files` array).

---

## Divergence 3 — `SubmissionFileDetail` Uses Flat `uploadedByName`, Not a Nested Object

### Design Expected

Not explicitly specified, but frontend was written expecting:

```ts
uploadedByUser: { id: number, name: string }
```

### Actual Implementation

`SubmissionFileDetail` record (returned by both `GetMySubmissionEndpoint` and `GetAssignmentSubmissionDetailEndpoint`):

```csharp
public record SubmissionFileDetail(
    int FileId,
    string FileName,
    string? FileUri,
    string? ExternalLink,
    int UploadedByUserId,
    string UploadedByName,
    DateTime UploadedAt);
```

**JSON shape:**

```json
{
	"fileId": 7,
	"fileName": "report.pdf",
	"fileUri": "submissions/3/uuid.pdf",
	"externalLink": null,
	"uploadedByUserId": 5,
	"uploadedByName": "John Doe",
	"uploadedAt": "2026-03-01T10:00:00Z"
}
```

No nested object. Use `file.uploadedByName` directly.

---

## Divergence 4 — `SubmissionDetail` Returned by `GET /api/assignments/{assignmentId}/submission/my`

The participant detail endpoint introduced in the 2026-02-27 revision returns the following shape (matching `GetMySubmissionEndpoint`):

```csharp
public record SubmissionDetail(
    int SubmissionId,
    int AssignmentId,
    int GroupId,
    string GroupName,
    bool IsCompleted,
    DateTime? CompletedAt,
    DateTime CreateTime,
    List<SubmissionFileDetail> Files);
```

**JSON shape:**

```json
{
	"submissionId": 42,
	"assignmentId": 1,
	"groupId": 3,
	"groupName": "Kelompok 1",
	"isCompleted": false,
	"completedAt": null,
	"createTime": "2026-03-01T09:00:00Z",
	"files": [
		{
			"fileId": 7,
			"fileName": "report.pdf",
			"fileUri": "submissions/42/uuid.pdf",
			"externalLink": null,
			"uploadedByUserId": 5,
			"uploadedByName": "John Doe",
			"uploadedAt": "2026-03-01T10:00:00Z"
		}
	]
}
```

Returns `null` data (not 404) if the participant has no group membership.

---

## Canonical API Route Table (as implemented)

### Participant Routes

| Method   | Route                                                          | Response Shape                                             |
| -------- | -------------------------------------------------------------- | ---------------------------------------------------------- |
| `GET`    | `/api/assignments/my`                                          | `List<MyAssignmentItem>` (flat, eager-creates submissions) |
| `GET`    | `/api/assignments/{assignmentId}/submission/my`                | `SubmissionDetail` or `null`                               |
| `POST`   | `/api/assignments/{assignmentId}/submission/my/files`          | `AddSubmissionFileResponse`                                |
| `DELETE` | `/api/assignments/{assignmentId}/submission/my/files/{fileId}` | `ApiResponse`                                              |
| `POST`   | `/api/assignments/{assignmentId}/submission/my/complete`       | `ApiResponse`                                              |
| `DELETE` | `/api/assignments/{assignmentId}/submission/my/complete`       | `ApiResponse`                                              |

### Admin Routes

| Method | Route                                              | Response Shape                     |
| ------ | -------------------------------------------------- | ---------------------------------- |
| `GET`  | `/api/assignments/{id}`                            | `AssignmentDetail` (metadata only) |
| `GET`  | `/api/assignments/{id}/submissions`                | `PagingResult<SubmissionListItem>` |
| `GET`  | `/api/assignments/{id}/submissions/{submissionId}` | `SubmissionDetail`                 |

---

## Frontend Impact

The following frontend files must be updated to align with the actual backend shapes:

| File                                            | Changes Required                                                                                                                                                                                                                                                                                   |
| ----------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Frontend/app/pages/assignments/index.vue`      | Fix interfaces — `subscription` is flat on each item, not nested. Use `assignment.isCompleted` and `assignment.fileCount` directly.                                                                                                                                                                |
| `Frontend/app/pages/assignments/[id].vue`       | Fetch assignment from `GET /assignments/{id}` and submission from `GET /assignments/{assignmentId}/submission/my` separately. Fix all participant submission API routes (remove `submissionId`). Fix `SubmissionFileDetail` interface — use `uploadedByName: string` not `uploadedByUser: object`. |
| `Frontend/app/pages/admin/assignments/[id].vue` | Fetch submissions from `GET /assignments/{id}/submissions` separately. Fix `SubmissionListItem` interface — no nested `group` or `files` objects; use flat `groupName` and `fileCount`.                                                                                                            |
