CREATE TABLE public.accountant_deleted_entities
(
	user_id integer NOT NULL,
	entity_type smallint NOT NULL,
	entity_id integer NOT NULL,
    deleted_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_accountant_deleted_entities PRIMARY KEY (user_id, entity_type, entity_id),
	CONSTRAINT fk_accountant_deleted_entities_aspnetusers_user_id FOREIGN KEY (user_id)
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.accountant_deleted_entities
    OWNER to personalassistant;

CREATE INDEX ix_accountant_deleted_entities_user_id_entity_type
    ON public.accountant_deleted_entities USING btree
    (user_id, entity_type)
    TABLESPACE pg_default;
