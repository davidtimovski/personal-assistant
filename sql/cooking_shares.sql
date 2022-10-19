CREATE TABLE public.cooking_shares
(
    recipe_id integer NOT NULL,
    user_id integer NOT NULL,
    is_accepted boolean DEFAULT NULL,
    last_opened_date timestamp with time zone NOT NULL,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_cooking_shares PRIMARY KEY (recipe_id, user_id),
    CONSTRAINT fk_cooking_shares_cooking_recipes_recipe_id FOREIGN KEY (recipe_id)
    REFERENCES public.cooking_recipes (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_cooking_shares_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.cooking_shares
    OWNER to personalassistant;

CREATE INDEX ix_cooking_shares_recipe_id
    ON public.cooking_shares USING btree
    (recipe_id)
    TABLESPACE pg_default;

CREATE INDEX ix_cooking_shares_user_id
    ON public.cooking_shares USING btree
    (user_id)
    TABLESPACE pg_default;
