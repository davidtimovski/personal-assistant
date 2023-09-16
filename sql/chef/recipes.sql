CREATE TABLE chef.recipes
(
    id serial NOT NULL,
    user_id integer NOT NULL,
    name character varying(50) NOT NULL COLLATE pg_catalog."default",
    description character varying(255) COLLATE pg_catalog."default",
    instructions character varying(5000) COLLATE pg_catalog."default",
    prep_duration interval(0),
    cook_duration interval(0),
    servings smallint NOT NULL,
    image_uri character varying(255) NOT NULL COLLATE pg_catalog."default",
    video_url character varying(1024) COLLATE pg_catalog."default",
    last_opened_date timestamp with time zone NOT NULL,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_recipes PRIMARY KEY (id),
    CONSTRAINT fk_recipes_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE chef.recipes
    OWNER to personalassistant;

CREATE INDEX ix_recipes_user_id
    ON chef.recipes USING btree
    (user_id)
    TABLESPACE pg_default;
