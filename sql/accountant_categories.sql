CREATE TABLE public.accountant_categories
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
    CONSTRAINT pk_accountant_categories PRIMARY KEY (id),
    CONSTRAINT fk_accountant_categories_accountant_categories_parent_id FOREIGN KEY (parent_id)
    REFERENCES public.accountant_categories (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL,
    CONSTRAINT fk_accountant_categories_aspnetusers_user_id FOREIGN KEY (user_id)
    REFERENCES public."AspNetUsers" ("Id") MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.accountant_categories
    OWNER to personalassistant;

CREATE INDEX ix_accountant_categories_parent_id
    ON public.accountant_categories USING btree
    (parent_id)
    TABLESPACE pg_default;

CREATE INDEX ix_accountant_categories_user_id
    ON public.accountant_categories USING btree
    (user_id)
    TABLESPACE pg_default;
