CREATE TABLE accountant.upcoming_expenses
(
    id serial NOT NULL,
    user_id integer NOT NULL,
    category_id integer,
    amount decimal(10, 2) NOT NULL,
    currency character varying(3) COLLATE pg_catalog."default" NOT NULL DEFAULT 'EUR',
    description character varying(255) COLLATE pg_catalog."default",
    date date NOT NULL,
    generated boolean NOT NULL DEFAULT FALSE,
    created_date timestamp with time zone NOT NULL,
    modified_date timestamp with time zone NOT NULL,
    CONSTRAINT pk_upcoming_expenses PRIMARY KEY (id),
    CONSTRAINT fk_upcoming_expenses_users_user_id FOREIGN KEY (user_id)
    REFERENCES public.users (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE,
    CONSTRAINT fk_upcoming_expenses_categories_category_id FOREIGN KEY (category_id)
    REFERENCES accountant.categories (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE accountant.upcoming_expenses
    OWNER to personalassistant;

CREATE INDEX ix_upcoming_expenses_user_id
    ON accountant.upcoming_expenses USING btree
    (user_id)
    TABLESPACE pg_default;

CREATE INDEX ix_upcoming_expenses_category_id
    ON accountant.upcoming_expenses USING btree
    (category_id)
    TABLESPACE pg_default;
