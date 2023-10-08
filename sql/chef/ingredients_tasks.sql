CREATE TABLE IF NOT EXISTS chef.ingredients_tasks
(
    ingredient_id integer NOT NULL,
    user_id integer NOT NULL,
    task_id integer NOT NULL,
    created_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_ingredients_tasks PRIMARY KEY (ingredient_id, user_id),
    CONSTRAINT fk_ingredients_tasks_ingredients_ingredient_id FOREIGN KEY (ingredient_id)
        REFERENCES chef.ingredients (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT fk_ingredients_tasks_users_user_id FOREIGN KEY (user_id)
        REFERENCES public.users (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE,
    CONSTRAINT fk_ingredients_tasks_tasks_task_id FOREIGN KEY (task_id)
        REFERENCES chef.tasks (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE chef.ingredients_tasks
    OWNER to personalassistant;
    
CREATE INDEX ix_ingredients_tasks_ingredient_id
    ON chef.ingredients_tasks USING btree
    (ingredient_id)
    TABLESPACE pg_default;
    
CREATE INDEX ix_ingredients_tasks_user_id
    ON chef.ingredients_tasks USING btree
    (user_id)
    TABLESPACE pg_default;

CREATE INDEX ix_ingredients_tasks_task_id
    ON chef.ingredients_tasks USING btree
    (task_id)
    TABLESPACE pg_default;
