CREATE TABLE IF NOT EXISTS reference_documents (
    id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ) PRIMARY KEY,
    title VARCHAR(255) NOT NULL,                     -- Display title
    description VARCHAR(511) NOT NULL,                        -- Optional description
    file_name VARCHAR(255) NOT NULL,                 -- Original filename
    uploaded_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

ALTER TABLE IF EXISTS reference_documents
    OWNER to postgres;

REVOKE ALL ON TABLE reference_documents FROM forbiddenknowledgeappuser;

GRANT SELECT ON TABLE reference_documents TO forbiddenknowledgeappuser;

GRANT ALL ON TABLE reference_documents TO postgres;