-- ============================================================================
-- V202602180001: Create all Auth tables
-- ============================================================================

-- 1. tenant_lookup
CREATE TABLE tenant_lookup (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    name VARCHAR NOT NULL,
    domain VARCHAR NOT NULL,
    schema_name VARCHAR NOT NULL,
    status SMALLINT NOT NULL,
    tier SMALLINT NOT NULL,
    db_version VARCHAR NULL
);

CREATE INDEX ix_tenant_lookup_tenant_code ON tenant_lookup (tenant_code);
CREATE UNIQUE INDEX uq_tenant_lookup_tenant_code_domain ON tenant_lookup (tenant_code, domain);

-- 2. auth_users
CREATE TABLE auth_users (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    username VARCHAR NOT NULL,
    email VARCHAR NOT NULL,
    password_hash BYTEA NOT NULL,
    status SMALLINT NOT NULL
);

CREATE INDEX ix_auth_users_tenant_code ON auth_users (tenant_code);
CREATE UNIQUE INDEX uq_auth_users_tenant_code_email ON auth_users (tenant_code, email);
CREATE UNIQUE INDEX uq_auth_users_tenant_code_username ON auth_users (tenant_code, username);

-- 3. auth_roles
CREATE TABLE auth_roles (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    name VARCHAR NOT NULL,
    description VARCHAR NULL
);

CREATE INDEX ix_auth_roles_tenant_code ON auth_roles (tenant_code);
CREATE UNIQUE INDEX uq_auth_roles_tenant_code_name ON auth_roles (tenant_code, name);

-- 4. auth_claims
CREATE TABLE auth_claims (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    name VARCHAR NOT NULL,
    description VARCHAR NULL
);

CREATE INDEX ix_auth_claims_tenant_code ON auth_claims (tenant_code);
CREATE UNIQUE INDEX uq_auth_claims_tenant_code_name ON auth_claims (tenant_code, name);

-- 5. auth_signing_keys
CREATE TABLE auth_signing_keys (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    kid VARCHAR NOT NULL,
    algorithm VARCHAR NOT NULL,
    public_key VARCHAR NOT NULL,
    encrypted_private_key VARCHAR NOT NULL,
    status SMALLINT NOT NULL,
    rotated_at TIMESTAMPTZ NULL,
    expires_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX ix_auth_signing_keys_tenant_code ON auth_signing_keys (tenant_code);

-- 6. auth_consent_terms
CREATE TABLE auth_consent_terms (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    type SMALLINT NOT NULL,
    version VARCHAR NOT NULL,
    content VARCHAR NOT NULL,
    published_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX ix_auth_consent_terms_tenant_code ON auth_consent_terms (tenant_code);

-- 7. auth_deny_list_entries
CREATE TABLE auth_deny_list_entries (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    type SMALLINT NOT NULL,
    value VARCHAR NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    reason VARCHAR NULL
);

CREATE INDEX ix_auth_deny_list_entries_tenant_code ON auth_deny_list_entries (tenant_code);

-- 8. auth_idempotency_records
CREATE TABLE auth_idempotency_records (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    idempotency_key VARCHAR NOT NULL,
    request_hash VARCHAR NOT NULL,
    response_body VARCHAR NULL,
    status_code INTEGER NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX ix_auth_idempotency_records_tenant_code ON auth_idempotency_records (tenant_code);

-- 9. auth_login_attempts
CREATE TABLE auth_login_attempts (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    username VARCHAR NOT NULL,
    ip_address VARCHAR NULL,
    attempted_at TIMESTAMPTZ NOT NULL,
    is_successful BOOLEAN NOT NULL,
    failure_reason VARCHAR NULL
);

CREATE INDEX ix_auth_login_attempts_tenant_code ON auth_login_attempts (tenant_code);

-- 10. auth_refresh_tokens
CREATE TABLE auth_refresh_tokens (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    user_id UUID NOT NULL,
    token_hash BYTEA NOT NULL,
    family_id UUID NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    status SMALLINT NOT NULL,
    revoked_at TIMESTAMPTZ NULL,
    replaced_by_token_id UUID NULL
);

CREATE INDEX ix_auth_refresh_tokens_tenant_code ON auth_refresh_tokens (tenant_code);

-- 11. auth_dpop_keys
CREATE TABLE auth_dpop_keys (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    user_id UUID NOT NULL,
    jwk_thumbprint VARCHAR NOT NULL,
    public_key_jwk VARCHAR NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    status SMALLINT NOT NULL,
    revoked_at TIMESTAMPTZ NULL
);

CREATE INDEX ix_auth_dpop_keys_tenant_code ON auth_dpop_keys (tenant_code);

-- 12. auth_mfa_setups
CREATE TABLE auth_mfa_setups (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    user_id UUID NOT NULL,
    encrypted_shared_secret VARCHAR NOT NULL,
    is_enabled BOOLEAN NOT NULL,
    enabled_at TIMESTAMPTZ NULL
);

CREATE INDEX ix_auth_mfa_setups_tenant_code ON auth_mfa_setups (tenant_code);

-- 13. auth_recovery_codes
CREATE TABLE auth_recovery_codes (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    user_id UUID NOT NULL,
    code_hash VARCHAR NOT NULL,
    is_used BOOLEAN NOT NULL,
    used_at TIMESTAMPTZ NULL
);

CREATE INDEX ix_auth_recovery_codes_tenant_code ON auth_recovery_codes (tenant_code);

-- 14. auth_password_reset_tokens
CREATE TABLE auth_password_reset_tokens (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    user_id UUID NOT NULL,
    token_hash VARCHAR NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    is_used BOOLEAN NOT NULL,
    used_at TIMESTAMPTZ NULL
);

CREATE INDEX ix_auth_password_reset_tokens_tenant_code ON auth_password_reset_tokens (tenant_code);

-- 15. auth_password_histories
CREATE TABLE auth_password_histories (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    user_id UUID NOT NULL,
    password_hash VARCHAR NOT NULL,
    changed_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX ix_auth_password_histories_tenant_code ON auth_password_histories (tenant_code);

-- 16. auth_external_logins
CREATE TABLE auth_external_logins (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    user_id UUID NOT NULL,
    provider VARCHAR NOT NULL,
    provider_user_id VARCHAR NOT NULL,
    email VARCHAR NULL
);

CREATE INDEX ix_auth_external_logins_tenant_code ON auth_external_logins (tenant_code);

-- 17. auth_key_chains
CREATE TABLE auth_key_chains (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    user_id UUID NOT NULL,
    key_id VARCHAR NOT NULL,
    public_key VARCHAR NOT NULL,
    encrypted_shared_secret VARCHAR NOT NULL,
    status SMALLINT NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX ix_auth_key_chains_tenant_code ON auth_key_chains (tenant_code);

-- 18. auth_sessions
CREATE TABLE auth_sessions (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    user_id UUID NOT NULL,
    refresh_token_id UUID NOT NULL,
    device_info VARCHAR NULL,
    ip_address VARCHAR NULL,
    user_agent VARCHAR NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    status SMALLINT NOT NULL,
    last_activity_at TIMESTAMPTZ NOT NULL,
    revoked_at TIMESTAMPTZ NULL
);

CREATE INDEX ix_auth_sessions_tenant_code ON auth_sessions (tenant_code);

-- 19. auth_role_claims
CREATE TABLE auth_role_claims (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    role_id UUID NOT NULL,
    claim_id UUID NOT NULL,
    value SMALLINT NOT NULL
);

CREATE INDEX ix_auth_role_claims_tenant_code ON auth_role_claims (tenant_code);

-- 20. auth_user_roles
CREATE TABLE auth_user_roles (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    user_id UUID NOT NULL,
    role_id UUID NOT NULL
);

CREATE INDEX ix_auth_user_roles_tenant_code ON auth_user_roles (tenant_code);

-- 21. auth_claim_dependencies
CREATE TABLE auth_claim_dependencies (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    claim_id UUID NOT NULL,
    depends_on_claim_id UUID NOT NULL
);

CREATE INDEX ix_auth_claim_dependencies_tenant_code ON auth_claim_dependencies (tenant_code);

-- 22. auth_role_hierarchies
CREATE TABLE auth_role_hierarchies (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    role_id UUID NOT NULL,
    parent_role_id UUID NOT NULL
);

CREATE INDEX ix_auth_role_hierarchies_tenant_code ON auth_role_hierarchies (tenant_code);

-- 23. auth_service_clients
CREATE TABLE auth_service_clients (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    client_id VARCHAR NOT NULL,
    client_secret_hash BYTEA NOT NULL,
    name VARCHAR NOT NULL,
    status SMALLINT NOT NULL,
    created_by_user_id UUID NOT NULL,
    expires_at TIMESTAMPTZ NULL,
    revoked_at TIMESTAMPTZ NULL
);

CREATE INDEX ix_auth_service_clients_tenant_code ON auth_service_clients (tenant_code);
CREATE UNIQUE INDEX uq_auth_service_clients_tenant_code_client_id ON auth_service_clients (tenant_code, client_id);

-- 24. auth_api_keys
CREATE TABLE auth_api_keys (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    service_client_id UUID NOT NULL,
    key_prefix VARCHAR NOT NULL,
    key_hash VARCHAR NOT NULL,
    status SMALLINT NOT NULL,
    expires_at TIMESTAMPTZ NULL,
    last_used_at TIMESTAMPTZ NULL,
    revoked_at TIMESTAMPTZ NULL
);

CREATE INDEX ix_auth_api_keys_tenant_code ON auth_api_keys (tenant_code);

-- 25. auth_service_client_scopes
CREATE TABLE auth_service_client_scopes (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    service_client_id UUID NOT NULL,
    scope VARCHAR NOT NULL
);

CREATE INDEX ix_auth_service_client_scopes_tenant_code ON auth_service_client_scopes (tenant_code);

-- 26. auth_service_client_claims
CREATE TABLE auth_service_client_claims (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    service_client_id UUID NOT NULL,
    claim_id UUID NOT NULL,
    value SMALLINT NOT NULL
);

CREATE INDEX ix_auth_service_client_claims_tenant_code ON auth_service_client_claims (tenant_code);

-- 27. auth_impersonation_sessions
CREATE TABLE auth_impersonation_sessions (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    operator_user_id UUID NOT NULL,
    target_user_id UUID NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    status SMALLINT NOT NULL,
    ended_at TIMESTAMPTZ NULL
);

CREATE INDEX ix_auth_impersonation_sessions_tenant_code ON auth_impersonation_sessions (tenant_code);

-- 28. auth_token_exchanges
CREATE TABLE auth_token_exchanges (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    user_id UUID NOT NULL,
    subject_token_jti VARCHAR NOT NULL,
    requested_audience VARCHAR NOT NULL,
    issued_token_jti VARCHAR NOT NULL,
    issued_at TIMESTAMPTZ NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL
);

CREATE INDEX ix_auth_token_exchanges_tenant_code ON auth_token_exchanges (tenant_code);

-- 29. auth_user_consents
CREATE TABLE auth_user_consents (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    created_by VARCHAR NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    created_correlation_id UUID NOT NULL,
    created_execution_origin VARCHAR NOT NULL,
    created_business_operation_code VARCHAR NOT NULL,
    last_changed_by VARCHAR NULL,
    last_changed_at TIMESTAMPTZ NULL,
    last_changed_execution_origin VARCHAR NULL,
    last_changed_correlation_id UUID NULL,
    last_changed_business_operation_code VARCHAR NULL,
    entity_version BIGINT NOT NULL DEFAULT 0,
    user_id UUID NOT NULL,
    consent_term_id UUID NOT NULL,
    accepted_at TIMESTAMPTZ NOT NULL,
    status SMALLINT NOT NULL,
    revoked_at TIMESTAMPTZ NULL,
    ip_address VARCHAR NULL
);

CREATE INDEX ix_auth_user_consents_tenant_code ON auth_user_consents (tenant_code);
