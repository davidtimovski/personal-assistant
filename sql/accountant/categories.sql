CREATE TABLE accountant.categories
(
    id serial NOT NULL,
    parent_id integer,
    user_id integer NOT NULL,
    name character varying(30) NOT NULL COLLATE pg_catalog."default",
    type smallint NOT NULL,
    generate_upcoming_expense boolean NOT NULL DEFAULT FALSE,
    is_tax boolean NOT NULL DEFAULT FALSE,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_categories PRIMARY KEY (id),
    CONSTRAINT fk_categories_categories_parent_id FOREIGN KEY (parent_id)
    REFERENCES accountant.categories (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL,
    CONSTRAINT fk_categories_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE accountant.categories
    OWNER to personalassistant;

CREATE INDEX ix_categories_parent_id
    ON accountant.categories USING btree
    (parent_id)
    TABLESPACE pg_default;

CREATE INDEX ix_categories_user_id
    ON accountant.categories USING btree
    (user_id)
    TABLESPACE pg_default;
