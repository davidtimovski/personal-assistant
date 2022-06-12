CREATE TABLE public.todo_notifications
(
    id serial NOT NULL,
    user_id integer NOT NULL,
    action_user_id integer NOT NULL,
    list_id integer,
    task_id integer,
    message character varying(255) NOT NULL COLLATE pg_catalog."default",
    is_seen boolean NOT NULL DEFAULT false,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_todo_notifications PRIMARY KEY (id),
    CONSTRAINT fk_todo_notifications_aspnetusers_user_id FOREIGN KEY (user_id)
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_todo_notifications_aspmetusers_action_user_id FOREIGN KEY (action_user_id)
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_todo_notifications_todo_lists_list_id FOREIGN KEY (list_id)
    REFERENCES public.todo_lists (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL,
    CONSTRAINT fk_todo_notifications_todo_tasks_task_id FOREIGN KEY (task_id)
    REFERENCES public.todo_tasks (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.todo_notifications
    OWNER to personalassistant;

CREATE INDEX ix_todo_notifications_user_id
    ON public.todo_notifications USING btree
    (user_id)
    TABLESPACE pg_default;
