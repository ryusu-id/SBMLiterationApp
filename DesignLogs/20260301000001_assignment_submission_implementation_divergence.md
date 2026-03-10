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

## Route Redesign — 2026-02-27: Participant Submission Endpoints

### Motivation

The original design (`20260223143000_group_and_assignment_module.md`) used `/submissions/{submissionId}/` path segments for participant routes, requiring the client to know the submission ID to attach files or toggle completion. This is unnecessary — participants always operate on *their own group's* submission for a given assignment. The group can be resolved server-side from the JWT `sub` claim via `GroupMember`, making `submissionId` in the path redundant.

A standalone `POST /api/assignments/{id}/submission` endpoint for creating a bare submission record was also dropped — submission creation is handled implicitly on the first file upload.

### Removed

| Method | Route | Reason |
|--------|-------|--------|
| `POST` | `/api/assignments/{id}/submission` | Standalone submission creation is unnecessary. `AssignmentSubmission` is auto-created on first file upload. |

### Changed (Participant routes only — Admin routes unchanged)

| Old Route | New Route | Change |
|-----------|-----------|--------|
| `POST /api/assignments/{id}/submissions/{submissionId}/complete` | `POST /api/assignments/{assignmentId}/submission/my/complete` | Removed `submissionId`; group resolved via JWT claim. Singular `submission`. |
| `DELETE /api/assignments/{id}/submissions/{submissionId}/complete` | `DELETE /api/assignments/{assignmentId}/submission/my/complete` | Same as above. |
| `POST /api/assignments/{id}/submissions/{submissionId}/files` | `POST /api/assignments/{assignmentId}/submission/my/files` | Removed `submissionId`; group resolved via JWT claim. Singular `submission`. |
| `DELETE /api/assignments/{id}/submissions/{submissionId}/files/{fileId}` | `DELETE /api/assignments/{assignmentId}/submission/my/files/{fileId}` | Removed `submissionId`; group resolved via JWT claim. Singular `submission`. |

### Added

| Method | Route | Role | Purpose |
|--------|-------|------|---------|
| `GET` | `/api/assignments/{assignmentId}/submission/my` | participant | Retrieve the calling user's group submission (with files) for a given assignment. Returns `null` data if no submission exists yet. |

### Key Design Rules

- All participant submission routes use `/submission/my/` (singular, no `submissionId` in path).
- The participant's group is always resolved server-side via `GroupMember` lookup using the JWT `sub` claim. Clients never pass a submission ID or group ID.
- Admin submission routes retain the plural `submissions` path and are unchanged.

### Backend Files Changed

| File | Change |
|------|--------|
| `Backend/Features/AssignmentModule/Endpoints/SubmissionEndpoints/AddSubmissionFileEndpoint.cs` | Route `{id}/submissions/files` → `{assignmentId}/submission/my/files`; request `Id` → `AssignmentId` |
| `Backend/Features/AssignmentModule/Endpoints/SubmissionEndpoints/RemoveSubmissionFileEndpoint.cs` | Route `{id}/submissions/my/files/{fileId}` → `{assignmentId}/submission/my/files/{fileId}`; request `Id` → `AssignmentId` |
| `Backend/Features/AssignmentModule/Endpoints/SubmissionEndpoints/MarkSubmissionCompleteEndpoint.cs` | Route `{id}/submissions/my/complete` → `{assignmentId}/submission/my/complete`; request `Id` → `AssignmentId` |
| `Backend/Features/AssignmentModule/Endpoints/SubmissionEndpoints/UnmarkSubmissionCompleteEndpoint.cs` | Route `{id}/submissions/my/complete` → `{assignmentId}/submission/my/complete`; request `Id` → `AssignmentId` |
| `Backend/Features/AssignmentModule/Endpoints/SubmissionEndpoints/GetMySubmissionEndpoint.cs` | Route `{id}/submissions/my` → `{assignmentId}/submission/my`; request `Id` → `AssignmentId` |

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

---

## Revision — 2026-03-01: Admin Submission File Viewing

| Field            | Value                 |
| ---------------- | --------------------- |
| **Date**         | 2026-03-01            |
| **Author**       | Copilot (antigravity) |
| **Significance** | 🟡 Minor              |
| **Status**       | ✅ APPROVED           |

### Motivation

The admin submission table previously only showed a file count and a two-state badge (Completed / In Progress). The admin needs to:

1. See a **three-state submission status** at a glance: Completed, In Progress, or Not Yet.
2. **Open and download the actual files** submitted by each group without leaving the assignment detail page.

### Three-State Status Logic

| Condition | Label | Badge Colour |
| --------- | ----- | ------------ |
| `isCompleted === true` | **Completed** | `success` (green) |
| `isCompleted === false && fileCount > 0` | **In Progress** | `warning` (yellow) |
| `isCompleted === false && fileCount === 0` | **Not Yet** | `error` (red) |

`submissionId` may be `null` for groups that have never triggered `GET /assignments/my`. These groups always have `fileCount === 0` and `isCompleted === false`, so they fall into "Not Yet".

### File Viewing Flow

1. Admin opens `GET /api/assignments/{id}` page.
2. The submission table row shows a download-icon **Action** button only when `submissionId` is not null (i.e. the group has an existing submission record, regardless of file count).
3. Clicking the Action button calls `GET /api/assignments/{id}/submissions/{submissionId}` and opens a **modal** showing the full `SubmissionDetail` with its `files` array.
4. Each file entry renders:
   - The `fileName` as label.
   - An **Open** link (`target="_blank"`) pointing to `fileUri` (MinIO URL) or `externalLink`, whichever is set.
   - `uploadedByName` and formatted `uploadedAt` as metadata.

### Backend Endpoint Used

`GET /api/assignments/{id}/submissions/{submissionId}` — already implemented, returns `SubmissionDetail` (see Divergence 4 for shape).

### Files Changed

| File | Change |
| ---- | ------ |
| `Frontend/app/pages/admin/assignments/[id].vue` | Add three-state status badge; add Action column with per-row file viewer modal; fetch `SubmissionDetail` on demand. |
