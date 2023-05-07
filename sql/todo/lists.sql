CREATE TABLE todo.lists
(
    id serial NOT NULL,
    user_id integer NOT NULL,
    name character varying(50) NOT NULL COLLATE pg_catalog."default",
    icon character varying(15) COLLATE pg_catalog."default" NOT NULL DEFAULT 'Regular'::character varying,
    order smallint,
    notifications_enabled boolean NOT NULL DEFAULT FALSE,
    is_one_time_toggle_default boolean NOT NULL DEFAULT FALSE,
    is_archived boolean NOT NULL DEFAULT FALSE,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_lists PRIMARY KEY (id),
    CONSTRAINT fk_lists_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE todo.lists
    OWNER to personalassistant;

CREATE INDEX ix_lists_user_id
    ON todo.lists USING btree
    (user_id)
    TABLESPACE pg_default;
