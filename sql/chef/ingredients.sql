CREATE TABLE chef.ingredients
(
    id serial NOT NULL,
    parent_id integer,
    user_id integer NOT NULL,
    category_id integer,
    brand_id integer,
    name character varying(50) COLLATE pg_catalog."default" NOT NULL,
    serving_size smallint NOT NULL DEFAULT 100,
    serving_size_is_one_unit boolean NOT NULL DEFAULT FALSE,
    calories numeric(4, 1),
    fat numeric(4, 1),
    saturated_fat numeric(4, 1),
    carbohydrate numeric(4, 1),
    sugars numeric(4, 1),
    added_sugars numeric(4, 1),
    fiber numeric(4, 1),
    protein numeric(4, 1),
    sodium integer,
    cholesterol smallint,
    vitamin_a smallint,
    vitamin_c smallint,
    vitamin_d smallint,
    calcium smallint,
    iron smallint,
    potassium smallint,
    magnesium numeric(4, 1),
    is_product boolean NOT NULL,
    product_size smallint NOT NULL DEFAULT 100,
    product_size_is_one_unit boolean NOT NULL DEFAULT FALSE,
    price numeric(6, 2),
    currency character varying(3) COLLATE pg_catalog."default",
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_ingredients PRIMARY KEY (id),
    CONSTRAINT uq_ingredients_name_type UNIQUE (name, type),
    CONSTRAINT fk_ingredients_ingredients_parent_id FOREIGN KEY (parent_id)
    REFERENCES chef.ingredients (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_ingredients_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_ingredients_ingredient_categories_category_id FOREIGN KEY (category_id)
    REFERENCES chef.ingredient_categories (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL,
    CONSTRAINT fk_ingredients_ingredient_categories_brand_id FOREIGN KEY (brand_id)
    REFERENCES chef.ingredient_categories (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE chef.ingredients
    OWNER to personalassistant;

CREATE INDEX ix_ingredients_user_id
    ON chef.ingredients USING btree
    (user_id)
    TABLESPACE pg_default;
