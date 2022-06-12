CREATE TABLE public.todo_shares
(
    list_id integer NOT NULL,
    user_id integer NOT NULL,
    is_admin boolean NOT NULL DEFAULT false,
    is_accepted boolean DEFAULT NULL,
    is_archived boolean NOT NULL DEFAULT false,
    order smallint,
    notifications_enabled boolean NOT NULL DEFAULT TRUE,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_todo_shares PRIMARY KEY (list_id, user_id),
    CONSTRAINT fk_todo_shares_todo_lists_list_id FOREIGN KEY (list_id)
    REFERENCES public.todo_lists (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_todo_shares_aspnetusers_user_id FOREIGN KEY (user_id)
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.todo_shares
    OWNER to personalassistant;

CREATE INDEX ix_todo_shares_list_id
    ON public.todo_shares USING btree
    (list_id)
    TABLESPACE pg_default;

CREATE INDEX ix_todo_shares_user_id
    ON public.todo_shares USING btree
    (user_id)
    TABLESPACE pg_default;
