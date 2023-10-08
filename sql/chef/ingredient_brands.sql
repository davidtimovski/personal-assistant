CREATE TABLE chef.ingredient_brands
(
    id serial NOT NULL,
    name character varying(15) COLLATE pg_catalog."default",
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_ingredient_brands PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE chef.ingredient_brands
    OWNER to personalassistant;
