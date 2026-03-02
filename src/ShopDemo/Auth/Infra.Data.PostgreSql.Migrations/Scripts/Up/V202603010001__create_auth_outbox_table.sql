CREATE TABLE auth_outbox (
    id UUID PRIMARY KEY,
    tenant_code UUID NOT NULL,
    correlation_id UUID NOT NULL,
    payload_type TEXT NOT NULL,
    content_type TEXT NOT NULL,
    payload BYTEA NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    status SMALLINT NOT NULL,
    processed_at TIMESTAMPTZ,
    retry_count SMALLINT NOT NULL DEFAULT 0,
    is_processing BOOLEAN NOT NULL DEFAULT FALSE,
    processing_expiration TIMESTAMPTZ
);

-- ClaimNextBatchAsync: Pending/Failed entries (Branch 1)
-- WHERE status IN (1=Pending, 4=Failed) ORDER BY created_at
CREATE INDEX IF NOT EXISTS idx_auth_outbox_status_created
    ON auth_outbox(status, created_at)
    WHERE status IN (1, 4);

-- ClaimNextBatchAsync: expired Processing lease (Branch 2)
-- WHERE status = 2 (Processing) AND processing_expiration < NOW() ORDER BY created_at
CREATE INDEX IF NOT EXISTS idx_auth_outbox_processing_expiration
    ON auth_outbox(processing_expiration, created_at)
    WHERE status = 2;
