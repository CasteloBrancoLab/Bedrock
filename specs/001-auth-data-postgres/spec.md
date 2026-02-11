# Feature Specification: Auth User Data Access Layer (PostgreSQL)

**Feature Branch**: `001-auth-data-postgres`
**Created**: 2026-02-11
**Status**: Draft
**Input**: User description: "Implementar a camada de acesso a dados com PostgreSQL para a entidade User do domínio Auth, seguindo o projeto de template existente"
**Parent Issue**: [#138 — Auth: User + Credentials](https://github.com/CasteloBrancoLab/Bedrock/issues/138)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Persist a New User (Priority: P1)

When a new user registers, the system must be able to persist the User aggregate root (with all its properties: username, email, password hash, and status) to the database. The persisted data must include full audit trail information (who created, when, tenant, correlation) and support optimistic concurrency control.

**Why this priority**: Without the ability to persist new users, no other data operation is possible. This is the foundational capability that enables registration flows.

**Independent Test**: Can be fully tested by creating a User domain entity via `RegisterNew`, persisting it through the repository, and retrieving it back — verifying all fields are correctly round-tripped.

**Acceptance Scenarios**:

1. **Given** a valid User aggregate root created via `RegisterNew`, **When** the repository `RegisterNewAsync` is called, **Then** the user is persisted to the database and the method returns success.
2. **Given** a valid User aggregate root, **When** persisted and then retrieved by ID, **Then** all domain properties (username, email, password hash, status) and all audit fields (created by, created at, tenant, version) match the original entity.
3. **Given** a User with a specific tenant code, **When** persisted, **Then** the tenant code is stored and enforced in all subsequent queries.

---

### User Story 2 - Retrieve User by Email (Priority: P1)

The system must support looking up a user by their email address. This is the primary query path for authentication flows (login), where the user provides an email and password.

**Why this priority**: Email-based lookup is equally critical to persistence — it enables the login flow. Without it, persisted users cannot authenticate.

**Independent Test**: Can be tested by persisting a user with a known email, then retrieving by that email and verifying the returned entity matches.

**Acceptance Scenarios**:

1. **Given** a persisted User with email "user@example.com", **When** `GetByEmailAsync` is called with that email, **Then** the correct User aggregate root is returned with all properties intact.
2. **Given** no user exists with email "unknown@example.com", **When** `GetByEmailAsync` is called, **Then** null is returned.
3. **Given** multiple users exist in the same tenant, **When** `GetByEmailAsync` is called with a specific email, **Then** only the user matching both the email and the current tenant is returned.

---

### User Story 3 - Retrieve User by Username (Priority: P2)

The system must support looking up a user by their username. This supports display name resolution, profile lookups, and alternative login flows.

**Why this priority**: Username lookup is a secondary query path. While important for user management, it is not on the critical authentication path (email-based login is primary).

**Independent Test**: Can be tested by persisting a user with a known username, then retrieving by that username.

**Acceptance Scenarios**:

1. **Given** a persisted User with username "john_doe", **When** `GetByUsernameAsync` is called, **Then** the correct User aggregate root is returned.
2. **Given** no user with username "nonexistent", **When** `GetByUsernameAsync` is called, **Then** null is returned.

---

### User Story 4 - Check User Existence by Email and Username (Priority: P2)

The system must support lightweight existence checks by email and by username, without loading the full aggregate root. This supports uniqueness validation during registration (e.g., "this email is already in use").

**Why this priority**: Existence checks are performance-critical for registration validation. They avoid loading full entities when only a boolean answer is needed.

**Independent Test**: Can be tested by persisting a user and verifying `ExistsByEmailAsync` and `ExistsByUsernameAsync` return true, and return false for non-existent values.

**Acceptance Scenarios**:

1. **Given** a persisted User with email "user@example.com", **When** `ExistsByEmailAsync` is called, **Then** true is returned.
2. **Given** no user with email "new@example.com", **When** `ExistsByEmailAsync` is called, **Then** false is returned.
3. **Given** a persisted User with username "john_doe", **When** `ExistsByUsernameAsync` is called, **Then** true is returned.
4. **Given** no user with username "new_user", **When** `ExistsByUsernameAsync` is called, **Then** false is returned.

---

### User Story 5 - Retrieve User by ID (Priority: P2)

The system must support retrieving a user by their unique identifier. This is the standard lookup path used by the framework's base repository contract.

**Why this priority**: ID-based lookup is inherited from the base repository contract and is needed for generic entity operations, authorization checks, and internal cross-references.

**Independent Test**: Can be tested by persisting a user and retrieving by ID.

**Acceptance Scenarios**:

1. **Given** a persisted User, **When** `GetByIdAsync` is called with the user's ID, **Then** the correct aggregate root is returned.
2. **Given** a non-existent ID, **When** `GetByIdAsync` is called, **Then** null is returned.

---

### User Story 6 - Update User State (Priority: P3)

The system must support updating an existing User's mutable properties (username, password hash, status) with optimistic concurrency control. When a user changes their password or an admin suspends an account, the updated state must be persisted safely.

**Why this priority**: Updates are important but secondary to initial creation and retrieval. The domain model supports status transitions (Active, Suspended, Blocked) and password changes that need persistence.

**Independent Test**: Can be tested by persisting a user, modifying via domain methods (e.g., `ChangeStatus`), calling update, and verifying changes are reflected on re-retrieval.

**Acceptance Scenarios**:

1. **Given** a persisted User with status Active, **When** the user is suspended via `ChangeStatus` and updated in the repository, **Then** re-retrieving the user shows status Suspended.
2. **Given** a persisted User, **When** an update is attempted with a stale entity version, **Then** the update fails (optimistic concurrency violation) and the method returns failure.
3. **Given** a persisted User, **When** the password hash is changed and updated, **Then** the new password hash is correctly persisted and retrievable.

---

### Edge Cases

- What happens when a database connection failure occurs during persist or retrieve? The repository must handle the error gracefully, log the exception with distributed tracing context, and return a failure indicator (false or null) instead of throwing.
- What happens when the password hash byte array is empty or has unexpected length? The data model must persist the raw bytes as-is without interpretation — validation is the domain layer's responsibility.
- What happens when a user is queried in a tenant different from where they were created? The query must return null (tenant isolation).
- What happens when two concurrent updates target the same user? Optimistic concurrency via entity version must prevent lost updates.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST persist the User aggregate root with all domain properties: username, email, password hash (as binary data), and status.
- **FR-002**: System MUST persist all audit trail fields inherited from the base data model: created by, created at, last changed by, last changed at, execution origin, correlation ID, business operation code, and entity version.
- **FR-003**: System MUST support retrieving a User by unique identifier, returning a fully reconstituted domain aggregate root.
- **FR-004**: System MUST support retrieving a User by email address, scoped to the current tenant.
- **FR-005**: System MUST support retrieving a User by username, scoped to the current tenant.
- **FR-006**: System MUST support lightweight existence checks by email and by username (returning boolean, not full entity).
- **FR-007**: System MUST enforce multi-tenancy isolation: all queries MUST be scoped to the tenant in the execution context.
- **FR-008**: System MUST support updating a User's mutable fields with optimistic concurrency control using the entity version field.
- **FR-009**: System MUST handle database errors gracefully — logging exceptions with distributed tracing context and returning safe failure indicators (false or null) without propagating exceptions to callers.
- **FR-010**: System MUST transform between domain entities and data models using dedicated factory/adapter classes, keeping domain and persistence concerns separated.
- **FR-011**: System MUST store the password hash as raw binary data without any interpretation, encoding, or transformation at the persistence layer.
- **FR-012**: System MUST follow the established project template pattern: DataModel, Mapper, Factory (both directions), Adapter, DataModelRepository, and PostgreSqlRepository.

### Key Entities

- **User (Domain Entity)**: Already implemented aggregate root representing a system user. Properties: Username, Email (value object), PasswordHash (value object with binary data), UserStatus (enum: Active, Suspended, Blocked). Includes full EntityInfo with audit trail and multi-tenancy.
- **UserDataModel (Persistence)**: Database-facing representation of the User entity. Extends the base data model with user-specific columns: username (text), email (text), password hash (binary), status (integer). Includes all inherited audit/tenant fields.
- **IUserRepository (Domain Contract)**: Already defined repository interface with custom query methods (GetByEmail, GetByUsername, ExistsByEmail, ExistsByUsername) extending the base repository contract.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All repository operations (persist, retrieve by ID/email/username, existence checks, update) pass unit tests with 100% code coverage and 100% mutation test score.
- **SC-002**: A User persisted and retrieved maintains perfect data fidelity — all domain properties and audit fields round-trip without data loss or corruption.
- **SC-003**: Tenant isolation is enforced — queries never return data from a different tenant, verified by dedicated test scenarios.
- **SC-004**: Optimistic concurrency is enforced — concurrent modifications with stale versions are rejected, verified by test scenarios.
- **SC-005**: All database errors are handled gracefully — no unhandled exceptions propagate from the repository layer, verified by error-injection test scenarios.
- **SC-006**: The implementation follows 100% of the established template patterns (DataModel, Mapper, Factory, Adapter, DataModelRepository, PostgreSqlRepository), verified by structural review.

## Assumptions

- The User domain entity, value objects (Email, PasswordHash), and IUserRepository interface are already implemented and available from the domain layer (issue #138 domain scope).
- The auth project scaffolding (issue #137) provides the project structure where persistence code will be placed.
- The PostgreSQL database schema uses snake_case naming convention for tables and columns (consistent with the template project).
- The password hash is stored as a binary column (bytea in PostgreSQL), not as a text/base64 representation.
- UserStatus enum is stored as a smallint/byte value in the database.
- Email is stored as a text/varchar column in the database (the EmailAddress value object handles validation, not the persistence layer).
