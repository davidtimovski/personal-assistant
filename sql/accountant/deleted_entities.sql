CREATE TABLE accountant.deleted_entities
(
    user_id integer NOT NULL,
    entity_type smallint NOT NULL,
    entity_id integer NOT NULL,
    deleted_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_deleted_entities PRIMARY KEY (user_id, entity_type, entity_id),
    CONSTRAINT fk_deleted_entities_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE accountant.deleted_entities
    OWNER to personalassistant;

CREATE INDEX ix_deleted_entities_user_id_entity_type
    ON accountant.deleted_entities USING btree
    (user_id, entity_type)
    TABLESPACE pg_default;
