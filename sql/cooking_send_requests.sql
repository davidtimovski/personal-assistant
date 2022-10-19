CREATE TABLE public.cooking_send_requests
(
    recipe_id integer NOT NULL,
    user_id integer NOT NULL,
    is_declined boolean NOT NULL DEFAULT FALSE,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_cooking_send_requests PRIMARY KEY (recipe_id, user_id),
    CONSTRAINT fk_cooking_send_requests_cooking_recipes_recipe_id FOREIGN KEY (recipe_id)
    REFERENCES public.cooking_recipes (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_cooking_send_requests_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.cooking_send_requests
    OWNER to personalassistant;

CREATE INDEX ix_cooking_send_requests_recipe_id
    ON public.cooking_send_requests USING btree
    (recipe_id)
    TABLESPACE pg_default;

CREATE INDEX ix_cooking_send_requests_user_id
    ON public.cooking_send_requests USING btree
    (user_id)
    TABLESPACE pg_default;
