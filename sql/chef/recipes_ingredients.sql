CREATE TABLE chef.recipes_ingredients
(
    recipe_id integer NOT NULL,
    ingredient_id integer NOT NULL,
    amount decimal(7,2),
    unit character varying(5) COLLATE pg_catalog."default",
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_recipes_ingredients PRIMARY KEY (recipe_id, ingredient_id),
    CONSTRAINT fk_recipes_ingredients_recipes_recipe_id FOREIGN KEY (recipe_id)
    REFERENCES chef.recipes (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_recipes_ingredients_ingredients_ingredient_id FOREIGN KEY (ingredient_id)
    REFERENCES chef.ingredients (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE chef.recipes_ingredients
    OWNER to personalassistant;

CREATE INDEX ix_recipes_ingredients_recipe_id
    ON chef.recipes_ingredients USING btree
    (recipe_id)
    TABLESPACE pg_default;

CREATE INDEX ix_recipes_ingredients_ingredient_id
    ON chef.recipes_ingredients USING btree
    (ingredient_id)
    TABLESPACE pg_default;
