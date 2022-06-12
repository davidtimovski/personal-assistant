CREATE TABLE IF NOT EXISTS public.cooking_ingredients_tasks
(
    ingredient_id integer NOT NULL,
    user_id integer NOT NULL,
    task_id integer NOT NULL,
    created_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_cooking_ingredients_tasks PRIMARY KEY (ingredient_id, user_id),
    CONSTRAINT fk_cooking_ingredients_tasks_cooking_ingredients_ingredient_id FOREIGN KEY (ingredient_id)
        REFERENCES public.cooking_ingredients (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT fk_cooking_ingredients_tasks_aspnetusers_user_id FOREIGN KEY (user_id)
        REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT fk_cooking_ingredients_tasks_todo_tasks_task_id FOREIGN KEY (task_id)
        REFERENCES public.todo_tasks (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.cooking_ingredients_tasks
    OWNER to personalassistant;
    
CREATE INDEX ix_cooking_ingredients_tasks_ingredient_id
    ON public.cooking_ingredients_tasks USING btree
    (ingredient_id)
    TABLESPACE pg_default;
    
CREATE INDEX ix_cooking_ingredients_tasks_user_id
    ON public.cooking_ingredients_tasks USING btree
    (user_id)
    TABLESPACE pg_default;

CREATE INDEX ix_cooking_ingredients_tasks_task_id
    ON public.cooking_ingredients_tasks USING btree
    (task_id)
    TABLESPACE pg_default;
