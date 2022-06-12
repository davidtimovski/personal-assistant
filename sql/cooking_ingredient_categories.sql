CREATE TABLE public.cooking_ingredient_categories
(
    id serial NOT NULL,
    parent_id integer,
    name character varying(15) COLLATE pg_catalog."default",
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_cooking_ingredient_categories PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE public.cooking_igredient_categories
    OWNER to personalassistant;
