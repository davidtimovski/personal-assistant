CREATE TABLE todo.shares
(
    list_id integer NOT NULL,
    user_id integer NOT NULL,
    is_admin boolean NOT NULL DEFAULT FALSE,
    is_accepted boolean DEFAULT NULL,
    is_archived boolean NOT NULL DEFAULT FALSE,
    order smallint,
    notifications_enabled boolean NOT NULL DEFAULT FALSE,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_shares PRIMARY KEY (list_id, user_id),
    CONSTRAINT fk_shares_lists_list_id FOREIGN KEY (list_id)
    REFERENCES todo.lists (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_shares_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE todo.shares
    OWNER to personalassistant;

CREATE INDEX ix_shares_list_id
    ON todo.shares USING btree
    (list_id)
    TABLESPACE pg_default;

CREATE INDEX ix_shares_user_id
    ON todo.shares USING btree
    (user_id)
    TABLESPACE pg_default;
