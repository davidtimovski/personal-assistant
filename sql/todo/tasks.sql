CREATE TABLE todo.tasks
(
    id serial NOT NULL,
    list_id integer NOT NULL,
    name character varying(50) NOT NULL COLLATE pg_catalog."default",
	url character varying(1000) COLLATE pg_catalog."default",
    is_completed boolean NOT NULL DEFAULT FALSE,
    is_one_time boolean NOT NULL DEFAULT FALSE,
    is_high_priority boolean NOT NULL DEFAULT FALSE,
    private_to_user_id integer,
    assigned_to_user_id integer,
    order smallint NOT NULL,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_tasks PRIMARY KEY (id),
    CONSTRAINT fk_tasks_lists_list_id FOREIGN KEY (list_id)
    REFERENCES todo.lists (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_tasks_users_private_to_user_id FOREIGN KEY (private_to_user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_tasks_users_assigned_to_user_id FOREIGN KEY (assigned_to_user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE todo.tasks
    OWNER to personalassistant;

CREATE INDEX ix_tasks_list_id
    ON todo.tasks USING btree
    (list_id)
    TABLESPACE pg_default;
