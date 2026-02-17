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
